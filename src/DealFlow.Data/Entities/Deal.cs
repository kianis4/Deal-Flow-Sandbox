namespace DealFlow.Data.Entities;

public class Deal
{
    // === Existing fields (unchanged) ===
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

    // === Vision-aligned fields ===
    public int? AppNumber { get; set; }
    public string? AppStatus { get; set; }
    public string? CustomerLegalName { get; set; }
    public string? PrimaryVendor { get; set; }
    public string? DealFormat { get; set; }
    public string? Lessor { get; set; }
    public string? AccountManager { get; set; }
    public string? PrimaryEquipmentCategory { get; set; }

    // === Financial fields ===
    public decimal EquipmentCost { get; set; }
    public decimal GrossContract { get; set; }
    public decimal NetInvest { get; set; }
    public decimal MonthlyPayment { get; set; }
    public int PaymentsMade { get; set; }
    public int RemainingPayments { get; set; }
    public DateTimeOffset? BookingDate { get; set; }
    public DateTimeOffset? FinalPaymentDate { get; set; }
    public bool IsActive { get; set; } = true;

    // === NSF & Delinquency ===
    public int NsfCount { get; set; }
    public DateTimeOffset? LastNsfDate { get; set; }
    public int DaysPastDue { get; set; }
    public decimal Past1 { get; set; }
    public decimal Past31 { get; set; }
    public decimal Past61 { get; set; }
    public decimal Past91 { get; set; }

    // === Navigation ===
    public ICollection<DealEvent> Events { get; set; } = new List<DealEvent>();
}
