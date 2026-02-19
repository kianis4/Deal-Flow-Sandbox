namespace DealFlow.Contracts.Messages;

public record DealScored
{
    public Guid CorrelationId { get; init; }
    public Guid DealId { get; init; }
    public int Score { get; init; }
    public string RiskFlag { get; init; } = default!;
    public DateTimeOffset ScoredAt { get; init; }
}
