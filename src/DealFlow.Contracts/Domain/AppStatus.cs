namespace DealFlow.Contracts.Domain;

public static class AppStatus
{
    public const string CreditValidation = "CREDIT_VALIDATION";
    public const string CreditReview = "CREDIT_REVIEW";
    public const string AutoscoringApproved = "AUTOSCORING_APPROVED";
    public const string AutoscoringDeclined = "AUTOSCORING_DECLINED";
    public const string MissingInfo = "MISSING_INFO";
    public const string DealDeclined = "DEAL_DECLINED";
    public const string Funded = "FUNDED";
    public const string PaidOff = "PAID_OFF";
}
