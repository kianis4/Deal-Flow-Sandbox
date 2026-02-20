namespace DealFlow.Data.Entities;

public class Deal
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public string EquipmentType { get; set; } = default!;
    public int EquipmentYear { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Industry { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string CreditRating { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int? Score { get; set; }
    public string? RiskFlag { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<DealEvent> Events { get; set; } = new List<DealEvent>();
}
