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

        // Credit rating risk (CR1 = best, CR5 = worst)
        score += deal.CreditRating switch
        {
            "CR1" =>   0,
            "CR2" =>  -5,
            "CR3" => -15,
            "CR4" => -25,
            "CR5" => -35,
            _     =>   0
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
