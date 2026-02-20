using DealFlow.ReportingApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DealFlow.ReportingApi.Tests;

public class DocumentRequirementsTests
{
    private readonly DocumentRequirementsService _svc = new(
        Options.Create(new ExposureThresholdOptions
        {
            EnhancedThreshold = 250_000m,
            FullReviewThreshold = 1_000_000m
        }));

    [Fact]
    public void Below_250k_returns_Standard()
    {
        var result = _svc.Evaluate(120_000m);
        result.Tier.Should().Be("Standard");
        result.Requirements.Should().BeEmpty();
    }

    [Fact]
    public void At_250k_returns_Enhanced()
    {
        var result = _svc.Evaluate(250_000m);
        result.Tier.Should().Be("Enhanced");
        result.Requirements.Should().ContainSingle()
            .Which.Should().Contain("Bank statements");
    }

    [Fact]
    public void Between_250k_and_1M_returns_Enhanced()
    {
        var result = _svc.Evaluate(475_000m);
        result.Tier.Should().Be("Enhanced");
    }

    [Fact]
    public void At_1M_returns_FullReview()
    {
        var result = _svc.Evaluate(1_000_000m);
        result.Tier.Should().Be("FullReview");
        result.Requirements.Should().Contain(r => r.Contains("3-year"));
    }

    [Fact]
    public void Above_1M_returns_FullReview()
    {
        var result = _svc.Evaluate(1_500_000m);
        result.Tier.Should().Be("FullReview");
    }

    [Fact]
    public void Zero_exposure_returns_Standard()
    {
        var result = _svc.Evaluate(0m);
        result.Tier.Should().Be("Standard");
    }
}
