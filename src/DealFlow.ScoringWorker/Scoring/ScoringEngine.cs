using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;

namespace DealFlow.ScoringWorker.Scoring;

public static class ScoringEngine
{
    public static (int Score, string RiskFlag) Score(DealSubmitted deal)
    {
        var score = 100;

        // Amount risk
        score += deal.Amount switch
        {
            > 1_000_000 => -35,
            > 500_000   => -20,
            _           => 0
        };

        // Term risk
        if (deal.TermMonths > 60) score -= 10;

        // Equipment age risk
        if (deal.EquipmentYear < 2018) score -= 15;

        // Vendor tier risk
        score += deal.VendorTier switch
        {
            "C" => -20,
            "B" => -10,
            _   => 0
        };

        score = Math.Clamp(score, 0, 100);

        var flag = score switch
        {
            < 50 => RiskFlag.High,
            < 75 => RiskFlag.Medium,
            _    => RiskFlag.Low
        };

        return (score, flag);
    }
}
