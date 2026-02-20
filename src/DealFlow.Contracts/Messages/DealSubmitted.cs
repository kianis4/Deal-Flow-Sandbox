namespace DealFlow.Contracts.Messages;

public record DealSubmitted
{
    public Guid CorrelationId { get; init; }
    public Guid DealId { get; init; }
    public decimal Amount { get; init; }
    public int TermMonths { get; init; }
    public int EquipmentYear { get; init; }
    public string CreditRating { get; init; } = default!;
    public string Industry { get; init; } = default!;
    public string Province { get; init; } = default!;
}
