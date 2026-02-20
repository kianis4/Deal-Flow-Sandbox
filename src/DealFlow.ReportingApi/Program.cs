using DealFlow.Data;
using DealFlow.ReportingApi.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "deal-reporting-api" }));

// GET /api/v1/deals with optional filters
app.MapGet("/api/v1/deals", async (
    DealFlowDbContext db,
    string? status,
    decimal? minAmount,
    string? creditRating) =>
{
    var query = db.Deals.AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(d => d.Status == status.ToUpper());

    if (minAmount.HasValue)
        query = query.Where(d => d.Amount >= minAmount.Value);

    if (!string.IsNullOrWhiteSpace(creditRating))
        query = query.Where(d => d.CreditRating == creditRating.ToUpper());

    var deals = await query
        .OrderByDescending(d => d.CreatedAt)
        .Select(d => new DealSummary(
            d.Id, d.EquipmentType, d.Amount, d.CreditRating,
            d.Status, d.Score, d.RiskFlag, d.CreatedAt))
        .ToListAsync();

    return Results.Ok(deals);
})
.WithName("ListDeals")
;

// GET /api/v1/deals/{id}/timeline
app.MapGet("/api/v1/deals/{id:guid}/timeline", async (Guid id, DealFlowDbContext db) =>
{
    var exists = await db.Deals.AnyAsync(d => d.Id == id);
    if (!exists) return Results.NotFound();

    var events = await db.DealEvents
        .Where(e => e.DealId == id)
        .OrderBy(e => e.OccurredAt)
        .Select(e => new TimelineEvent(e.EventType, e.Payload, e.OccurredAt))
        .ToListAsync();

    return Results.Ok(events);
})
.WithName("GetTimeline")
;

app.Run();

public partial class Program { }
