using DealFlow.ReportingApi.Models;
using Microsoft.Extensions.Options;

namespace DealFlow.ReportingApi.Services;

public class ExposureThresholdOptions
{
    public decimal EnhancedThreshold { get; set; } = 250_000m;
    public decimal FullReviewThreshold { get; set; } = 1_000_000m;
}

public class DocumentRequirementsService(IOptions<ExposureThresholdOptions> options)
{
    private readonly ExposureThresholdOptions _opts = options.Value;

    public DocumentRequirements Evaluate(decimal totalNetExposure)
    {
        if (totalNetExposure >= _opts.FullReviewThreshold)
        {
            return new DocumentRequirements(
                Tier: "FullReview",
                TotalNetExposure: totalNetExposure,
                Requirements:
                [
                    "3-year financial statements required",
                    "Interim financial statements required",
                    "Spreads analysis required"
                ],
                Note: $"Exposure ${totalNetExposure:N2} exceeds ${_opts.FullReviewThreshold:N0} threshold"
            );
        }

        if (totalNetExposure >= _opts.EnhancedThreshold)
        {
            return new DocumentRequirements(
                Tier: "Enhanced",
                TotalNetExposure: totalNetExposure,
                Requirements:
                [
                    "Bank statements or financial statements required"
                ],
                Note: $"Exposure ${totalNetExposure:N2} exceeds ${_opts.EnhancedThreshold:N0} threshold"
            );
        }

        return new DocumentRequirements(
            Tier: "Standard",
            TotalNetExposure: totalNetExposure,
            Requirements: [],
            Note: "No additional documents required"
        );
    }
}
