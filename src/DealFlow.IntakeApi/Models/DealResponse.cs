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
    DateTimeOffset UpdatedAt,
    int? AppNumber = null,
    string? AppStatus = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? DealFormat = null,
    string? Lessor = null,
    string? AccountManager = null,
    string? PrimaryEquipmentCategory = null,
    decimal? NetInvest = null,
    bool IsActive = true
);
