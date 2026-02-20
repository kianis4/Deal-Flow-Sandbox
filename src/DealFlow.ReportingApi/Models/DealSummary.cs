namespace DealFlow.ReportingApi.Models;

public record DealSummary(
    Guid Id,
    string EquipmentType,
    decimal Amount,
    string CreditRating,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt,
    int? AppNumber = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? Lessor = null,
    bool IsActive = true
);

public record TimelineEvent(
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt
);
