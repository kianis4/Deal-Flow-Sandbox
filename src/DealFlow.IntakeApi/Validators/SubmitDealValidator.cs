using DealFlow.IntakeApi.Models;
using FluentValidation;

namespace DealFlow.IntakeApi.Validators;

public class SubmitDealValidator : AbstractValidator<SubmitDealRequest>
{
    private static readonly string[] ValidRatings = ["CR1", "CR2", "CR3", "CR4", "CR5"];

    public SubmitDealValidator()
    {
        RuleFor(x => x.EquipmentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EquipmentYear).InclusiveBetween(1990, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.TermMonths).InclusiveBetween(6, 120);
        RuleFor(x => x.Industry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CreditRating).Must(r => ValidRatings.Contains(r))
            .WithMessage("CreditRating must be CR1, CR2, CR3, CR4, or CR5 (CR1 = best)");
    }
}
