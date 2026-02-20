using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.Data;
using DealFlow.Data.Entities;
using DealFlow.ScoringWorker.Scoring;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DealFlow.ScoringWorker.Consumers;

public class DealSubmittedConsumer(
    DealFlowDbContext db,
    IPublishEndpoint publisher,
    ILogger<DealSubmittedConsumer> logger) : IConsumer<DealSubmitted>
{
    public async Task Consume(ConsumeContext<DealSubmitted> context)
    {
        var msg = context.Message;
        logger.LogInformation("Scoring deal {DealId} [correlation: {CorrelationId}]",
            msg.DealId, msg.CorrelationId);

        var deal = await db.Deals
            .FirstOrDefaultAsync(d => d.Id == msg.DealId);

        if (deal is null)
        {
            logger.LogWarning("Deal {DealId} not found — skipping", msg.DealId);
            return;
        }

        // Idempotency: skip only if already scored or beyond
        if (deal.Status == DealStatus.Scored || deal.Status == DealStatus.Notified)
        {
            logger.LogWarning("Deal {DealId} already in status {Status} — skipping (idempotency)",
                msg.DealId, deal.Status);
            return;
        }

        var (score, riskFlag) = ScoringEngine.Score(msg);

        deal.Score = score;
        deal.RiskFlag = riskFlag;
        deal.Status = DealStatus.Scored;
        deal.UpdatedAt = DateTimeOffset.UtcNow;

        db.DealEvents.Add(new DealEvent
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            EventType = "DealScored",
            Payload = JsonSerializer.Serialize(new { score, riskFlag }),
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        await publisher.Publish(new DealScored
        {
            CorrelationId = msg.CorrelationId,
            DealId = msg.DealId,
            Score = score,
            RiskFlag = riskFlag,
            ScoredAt = DateTimeOffset.UtcNow
        });

        logger.LogInformation("Deal {DealId} scored: {Score} ({RiskFlag})", msg.DealId, score, riskFlag);
    }
}
