namespace DealFlow.IntakeApi.Models;

public record SubmitDealRequest(
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string VendorTier
);
