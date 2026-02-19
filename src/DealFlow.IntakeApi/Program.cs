using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.Data;
using DealFlow.Data.Entities;
using DealFlow.IntakeApi.Models;
using DealFlow.IntakeApi.Validators;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());

// Database
builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<SubmitDealValidator>();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DealFlowDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "deal-intake-api" }));

// POST /api/v1/deals
app.MapPost("/api/v1/deals", async (
    SubmitDealRequest request,
    IValidator<SubmitDealRequest> validator,
    DealFlowDbContext db,
    IPublishEndpoint publisher,
    ILogger<Program> logger) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var correlationId = Guid.NewGuid();
    var deal = new Deal
    {
        Id = Guid.NewGuid(),
        CorrelationId = correlationId,
        EquipmentType = request.EquipmentType,
        EquipmentYear = request.EquipmentYear,
        Amount = request.Amount,
        TermMonths = request.TermMonths,
        Industry = request.Industry,
        Province = request.Province,
        VendorTier = request.VendorTier,
        Status = DealStatus.Received,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    deal.Events.Add(new DealEvent
    {
        Id = Guid.NewGuid(),
        DealId = deal.Id,
        EventType = "DealSubmitted",
        Payload = JsonSerializer.Serialize(request),
        OccurredAt = DateTimeOffset.UtcNow
    });

    db.Deals.Add(deal);
    await db.SaveChangesAsync();

    await publisher.Publish(new DealSubmitted
    {
        CorrelationId = correlationId,
        DealId = deal.Id,
        Amount = deal.Amount,
        TermMonths = deal.TermMonths,
        EquipmentYear = deal.EquipmentYear,
        VendorTier = deal.VendorTier,
        Industry = deal.Industry,
        Province = deal.Province
    });

    logger.LogInformation("Deal {DealId} submitted with correlation {CorrelationId}", deal.Id, correlationId);

    return Results.Created($"/api/v1/deals/{deal.Id}", ToResponse(deal));
})
.WithName("SubmitDeal")
.WithOpenApi();

// GET /api/v1/deals/{id}
app.MapGet("/api/v1/deals/{id:guid}", async (Guid id, DealFlowDbContext db) =>
{
    var deal = await db.Deals.FindAsync(id);
    return deal is null ? Results.NotFound() : Results.Ok(ToResponse(deal));
})
.WithName("GetDeal")
.WithOpenApi();

app.Run();

static DealResponse ToResponse(Deal d) => new(
    d.Id, d.CorrelationId, d.EquipmentType, d.EquipmentYear,
    d.Amount, d.TermMonths, d.Industry, d.Province,
    d.VendorTier, d.Status, d.Score, d.RiskFlag,
    d.CreatedAt, d.UpdatedAt);

// Required for WebApplicationFactory in tests
public partial class Program { }
