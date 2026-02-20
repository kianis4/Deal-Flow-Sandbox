namespace DealFlow.ReportingApi.Models;

public record ExposureResponse(
    string PartyName,
    string SearchType,
    ExposureSummary Summary,
    DocumentRequirements DocumentRequirements,
    List<ExposureDeal> Deals
);

public record ExposureSummary(
    int TotalDeals,
    int ActiveDeals,
    int PaidOffDeals,
    decimal TotalNetExposure,
    decimal TotalGrossContract,
    int TotalNsfCount,
    DateTimeOffset? LastNsfDate,
    int DealsWithNsfs,
    int DealsDelinquent,
    decimal TotalPastDue
);

public record DocumentRequirements(
    string Tier,
    decimal TotalNetExposure,
    List<string> Requirements,
    string Note
);

public record ExposureDeal(
    Guid Id,
    int? AppNumber,
    string? AppStatus,
    string? CustomerLegalName,
    string? PrimaryVendor,
    string? DealFormat,
    string? Lessor,
    string? AccountManager,
    string? PrimaryEquipmentCategory,
    string CreditRating,
    decimal EquipmentCost,
    decimal GrossContract,
    decimal NetInvest,
    decimal MonthlyPayment,
    int TermMonths,
    int PaymentsMade,
    int RemainingPayments,
    DateTimeOffset? BookingDate,
    bool IsActive,
    int NsfCount,
    DateTimeOffset? LastNsfDate,
    int DaysPastDue,
    decimal Past1,
    decimal Past31,
    decimal Past61,
    decimal Past91
);
