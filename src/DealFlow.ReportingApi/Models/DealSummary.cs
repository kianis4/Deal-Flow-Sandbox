namespace DealFlow.ReportingApi.Models;

public record DealSummary(
    Guid Id,
    string EquipmentType,
    decimal Amount,
    string CreditRating,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt
);

public record TimelineEvent(
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt
);
