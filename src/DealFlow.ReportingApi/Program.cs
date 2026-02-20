using DealFlow.Data;
using DealFlow.ReportingApi.Models;
using DealFlow.ReportingApi.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ExposureThresholdOptions>(
    builder.Configuration.GetSection("ExposureThresholds"));
builder.Services.AddSingleton<DocumentRequirementsService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseStaticFiles();
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
            d.Status, d.Score, d.RiskFlag, d.CreatedAt,
            d.AppNumber, d.CustomerLegalName, d.PrimaryVendor,
            d.Lessor, d.IsActive))
        .ToListAsync();

    return Results.Ok(deals);
})
.WithName("ListDeals");

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
.WithName("GetTimeline");

// GET /api/v1/exposure — Party Exposure Lookup
app.MapGet("/api/v1/exposure", async (
    string searchType,
    string name,
    bool? includePastDeals,
    DealFlowDbContext db,
    DocumentRequirementsService docReqs) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { error = "name parameter is required" });

    if (searchType is not ("customer" or "vendor"))
        return Results.BadRequest(new { error = "searchType must be 'customer' or 'vendor'" });

    var include = includePastDeals ?? false;
    var nameLower = name.ToLower();

    var query = searchType == "customer"
        ? db.Deals.Where(d => d.CustomerLegalName != null && d.CustomerLegalName.ToLower().Contains(nameLower))
        : db.Deals.Where(d => d.PrimaryVendor != null && d.PrimaryVendor.ToLower().Contains(nameLower));

    if (!include)
        query = query.Where(d => d.IsActive);

    var deals = await query
        .OrderByDescending(d => d.IsActive)
        .ThenByDescending(d => d.NetInvest)
        .Select(d => new ExposureDeal(
            d.Id, d.AppNumber, d.AppStatus, d.CustomerLegalName, d.PrimaryVendor,
            d.DealFormat, d.Lessor, d.AccountManager, d.PrimaryEquipmentCategory,
            d.CreditRating, d.EquipmentCost, d.GrossContract, d.NetInvest,
            d.MonthlyPayment, d.TermMonths, d.PaymentsMade, d.RemainingPayments,
            d.BookingDate, d.IsActive, d.NsfCount, d.LastNsfDate,
            d.DaysPastDue, d.Past1, d.Past31, d.Past61, d.Past91))
        .ToListAsync();

    if (deals.Count == 0)
        return Results.Ok(new { message = "No deals found", searchType, name });

    var activeDeals = deals.Where(d => d.IsActive).ToList();
    var totalNetExposure = activeDeals.Sum(d => d.NetInvest);

    var summary = new ExposureSummary(
        TotalDeals: deals.Count,
        ActiveDeals: activeDeals.Count,
        PaidOffDeals: deals.Count(d => !d.IsActive),
        TotalNetExposure: totalNetExposure,
        TotalGrossContract: activeDeals.Sum(d => d.GrossContract),
        TotalNsfCount: deals.Sum(d => d.NsfCount),
        LastNsfDate: deals.Where(d => d.LastNsfDate.HasValue)
            .Select(d => d.LastNsfDate!.Value)
            .OrderDescending().FirstOrDefault(),
        DealsWithNsfs: deals.Count(d => d.NsfCount > 0),
        DealsDelinquent: deals.Count(d => d.DaysPastDue > 0),
        TotalPastDue: deals.Sum(d => d.Past1 + d.Past31 + d.Past61 + d.Past91)
    );

    var partyName = searchType == "customer"
        ? deals.First().CustomerLegalName ?? name
        : deals.First().PrimaryVendor ?? name;

    var response = new ExposureResponse(
        PartyName: partyName,
        SearchType: searchType,
        Summary: summary,
        DocumentRequirements: docReqs.Evaluate(totalNetExposure),
        Deals: deals
    );

    return Results.Ok(response);
})
.WithName("GetExposure");

app.Run();

public partial class Program { }
