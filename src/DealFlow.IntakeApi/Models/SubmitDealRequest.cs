namespace DealFlow.IntakeApi.Models;

public record SubmitDealRequest(
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string CreditRating,
    int? AppNumber = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? DealFormat = null,
    string? Lessor = null,
    string? AccountManager = null,
    string? PrimaryEquipmentCategory = null,
    decimal? EquipmentCost = null,
    decimal? GrossContract = null,
    decimal? NetInvest = null,
    decimal? MonthlyPayment = null
);
