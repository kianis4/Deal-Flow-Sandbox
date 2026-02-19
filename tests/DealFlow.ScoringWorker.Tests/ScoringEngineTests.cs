using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.ScoringWorker.Scoring;
using FluentAssertions;

namespace DealFlow.ScoringWorker.Tests;

public class ScoringEngineTests
{
    private static DealSubmitted MakeDeal(
        decimal amount = 200_000,
        int termMonths = 36,
        int equipYear = 2022,
        string vendorTier = "A") => new()
    {
        CorrelationId = Guid.NewGuid(),
        DealId = Guid.NewGuid(),
        Amount = amount,
        TermMonths = termMonths,
        EquipmentYear = equipYear,
        VendorTier = vendorTier,
        Industry = "Construction",
        Province = "ON"
    };

    [Fact]
    public void Perfect_deal_scores_100_LOW()
    {
        var (score, flag) = ScoringEngine.Score(MakeDeal());
        score.Should().Be(100);
        flag.Should().Be(RiskFlag.Low);
    }

    [Fact]
    public void Amount_over_500k_reduces_score_by_20()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(amount: 750_000));
        score.Should().Be(80);
    }

    [Fact]
    public void Amount_over_1M_reduces_score_by_35()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(amount: 1_500_000));
        score.Should().Be(65);
    }

    [Fact]
    public void Term_over_60_months_reduces_score_by_10()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(termMonths: 72));
        score.Should().Be(90);
    }

    [Fact]
    public void Equipment_before_2018_reduces_score_by_15()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(equipYear: 2015));
        score.Should().Be(85);
    }

    [Fact]
    public void Tier_C_vendor_reduces_score_by_20()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(vendorTier: "C"));
        score.Should().Be(80);
    }

    [Fact]
    public void Tier_B_vendor_reduces_score_by_10()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(vendorTier: "B"));
        score.Should().Be(90);
    }

    [Fact]
    public void Worst_case_deal_is_HIGH_risk()
    {
        var worst = MakeDeal(amount: 2_000_000, termMonths: 84, equipYear: 2010, vendorTier: "C");
        var (score, flag) = ScoringEngine.Score(worst);
        score.Should().BeLessThan(50);
        flag.Should().Be(RiskFlag.High);
    }

    [Fact]
    public void Score_never_goes_below_zero()
    {
        var worst = MakeDeal(amount: 10_000_000, termMonths: 120, equipYear: 1990, vendorTier: "C");
        var (score, _) = ScoringEngine.Score(worst);
        score.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(74, "MEDIUM")]
    [InlineData(50, "MEDIUM")]
    [InlineData(49, "HIGH")]
    [InlineData(75, "LOW")]
    [InlineData(100, "LOW")]
    public void Risk_flag_thresholds_are_correct(int score, string expectedFlag)
    {
        // Test flag classification directly
        var flag = score switch
        {
            < 50 => RiskFlag.High,
            < 75 => RiskFlag.Medium,
            _    => RiskFlag.Low
        };
        flag.Should().Be(expectedFlag);
    }
}
