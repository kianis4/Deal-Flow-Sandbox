using DealFlow.IntakeApi.Models;
using FluentValidation;

namespace DealFlow.IntakeApi.Validators;

public class SubmitDealValidator : AbstractValidator<SubmitDealRequest>
{
    private static readonly string[] ValidTiers = ["A", "B", "C"];

    public SubmitDealValidator()
    {
        RuleFor(x => x.EquipmentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EquipmentYear).InclusiveBetween(1990, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.TermMonths).InclusiveBetween(6, 120);
        RuleFor(x => x.Industry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VendorTier).Must(t => ValidTiers.Contains(t))
            .WithMessage("VendorTier must be A, B, or C");
    }
}
