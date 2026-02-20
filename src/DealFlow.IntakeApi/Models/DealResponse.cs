namespace DealFlow.IntakeApi.Models;

public record DealResponse(
    Guid Id,
    Guid CorrelationId,
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string CreditRating,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
