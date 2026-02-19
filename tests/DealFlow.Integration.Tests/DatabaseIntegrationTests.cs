using DealFlow.Contracts.Domain;
using DealFlow.Data;
using DealFlow.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DealFlow.Integration.Tests;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("dealflow")
        .WithUsername("dealflow")
        .WithPassword("dealflow")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    private DealFlowDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<DealFlowDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var ctx = new DealFlowDbContext(opts);
        ctx.Database.Migrate();
        return ctx;
    }

    [Fact]
    public async Task Can_persist_and_retrieve_deal()
    {
        await using var ctx = CreateContext();

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            EquipmentType = "Excavator",
            EquipmentYear = 2021,
            Amount = 250_000,
            TermMonths = 48,
            Industry = "Construction",
            Province = "ON",
            VendorTier = "A",
            Status = DealStatus.Received,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        ctx.Deals.Add(deal);
        await ctx.SaveChangesAsync();

        var retrieved = await ctx.Deals.FindAsync(deal.Id);
        retrieved.Should().NotBeNull();
        retrieved!.EquipmentType.Should().Be("Excavator");
        retrieved.Status.Should().Be(DealStatus.Received);
        retrieved.Amount.Should().Be(250_000);
    }

    [Fact]
    public async Task Deal_events_are_persisted_with_deal()
    {
        await using var ctx = CreateContext();

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            EquipmentType = "Forklift",
            EquipmentYear = 2022,
            Amount = 100_000,
            TermMonths = 36,
            Industry = "Logistics",
            Province = "BC",
            VendorTier = "B",
            Status = DealStatus.Received,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        deal.Events.Add(new DealEvent
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            EventType = "DealSubmitted",
            Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow
        });

        ctx.Deals.Add(deal);
        await ctx.SaveChangesAsync();

        var events = await ctx.DealEvents.Where(e => e.DealId == deal.Id).ToListAsync();
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be("DealSubmitted");
    }

    [Fact]
    public async Task Deal_status_can_be_updated()
    {
        await using var ctx = CreateContext();

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            EquipmentType = "Crane",
            EquipmentYear = 2019,
            Amount = 500_000,
            TermMonths = 60,
            Industry = "Construction",
            Province = "AB",
            VendorTier = "C",
            Status = DealStatus.Received,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        ctx.Deals.Add(deal);
        await ctx.SaveChangesAsync();

        deal.Status = DealStatus.Scored;
        deal.Score = 60;
        deal.RiskFlag = "MEDIUM";
        deal.UpdatedAt = DateTimeOffset.UtcNow;
        await ctx.SaveChangesAsync();

        await using var ctx2 = CreateContext();
        var updated = await ctx2.Deals.FindAsync(deal.Id);
        updated!.Status.Should().Be(DealStatus.Scored);
        updated.Score.Should().Be(60);
        updated.RiskFlag.Should().Be("MEDIUM");
    }
}
