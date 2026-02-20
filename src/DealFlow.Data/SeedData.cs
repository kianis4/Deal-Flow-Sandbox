using DealFlow.Contracts.Domain;
using DealFlow.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealFlow.Data;

public static class SeedData
{
    public static async Task SeedAsync(DealFlowDbContext db)
    {
        if (await db.Deals.AnyAsync(d => d.AppNumber != null))
            return;

        var deals = BuildDeals();
        db.Deals.AddRange(deals);
        await db.SaveChangesAsync();
    }

    private static List<Deal> BuildDeals()
    {
        var deals = new List<Deal>();
        var now = DateTimeOffset.UtcNow;

        const string repEdwin = "Edwin Van Schepen";
        const string repDaniel = "Daniel De Luca";
        const string repSarah = "Sarah Mitchell";
        const string repJames = "James Wong";

        // ===== HIGH-EXPOSURE CUSTOMER: TransCanada Hauling Ltd. (~$535K active, Full Review) =====
        deals.Add(MakeDeal(119001, AppStatus.Funded, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Semi-Truck (Kenworth T680)", equipYear: 2024, equipCost: 185000, grossContract: 234500,
            netInvest: 195000, monthly: 4885.42m, term: 48, made: 12, remaining: 36,
            booking: now.AddMonths(-12), finalPmt: now.AddMonths(36),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true, nsfCount: 2, lastNsf: now.AddMonths(-3)));

        deals.Add(MakeDeal(119002, AppStatus.Funded, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Semi-Truck (Peterbilt 579)", equipYear: 2023, equipCost: 175000, grossContract: 221800,
            netInvest: 185000, monthly: 4620.83m, term: 48, made: 18, remaining: 30,
            booking: now.AddMonths(-18), finalPmt: now.AddMonths(30),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true, nsfCount: 1, lastNsf: now.AddMonths(-6)));

        deals.Add(MakeDeal(119003, AppStatus.Funded, "TransCanada Hauling Ltd.", "Fleet Equipment Corp.",
            DealFormat.Vendor, Lessor.MHCCA, repEdwin, "Transportation (TRAN)",
            equipType: "Dry Van Trailer (Utility 4000D-X)", equipYear: 2024, equipCost: 56400, grossContract: 71371,
            netInvest: 58143, monthly: 1486.69m, term: 48, made: 6, remaining: 42,
            booking: now.AddMonths(-6), finalPmt: now.AddMonths(42),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true));

        deals.Add(MakeDeal(119004, AppStatus.Funded, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Reefer Trailer (Carrier X4 7500)", equipYear: 2025, equipCost: 92000, grossContract: 116560,
            netInvest: 97300, monthly: 2427.50m, term: 48, made: 2, remaining: 46,
            booking: now.AddMonths(-2), finalPmt: now.AddMonths(46),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true, daysPastDue: 45, past31: 2427.50m));

        deals.Add(MakeDeal(118500, AppStatus.PaidOff, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Semi-Truck (Freightliner Cascadia)", equipYear: 2020, equipCost: 155000, grossContract: 196350,
            netInvest: 0, monthly: 4090.63m, term: 48, made: 48, remaining: 0,
            booking: now.AddMonths(-52), finalPmt: now.AddMonths(-4),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: false));

        deals.Add(MakeDeal(118501, AppStatus.PaidOff, "TransCanada Hauling Ltd.", "Fleet Equipment Corp.",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Transportation (TRAN)",
            equipType: "Flatbed Trailer (Fontaine Revolution)", equipYear: 2019, equipCost: 48000, grossContract: 60800,
            netInvest: 0, monthly: 1266.67m, term: 48, made: 48, remaining: 0,
            booking: now.AddMonths(-54), finalPmt: now.AddMonths(-6),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: false));

        // ===== MID-EXPOSURE: Excavation Pro Québec Inc. (~$476K active, Enhanced) =====
        deals.Add(MakeDeal(119010, AppStatus.Funded, "Excavation Pro Québec Inc.", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Construction (CONS)",
            equipType: "Excavator (CAT 320)", equipYear: 2024, equipCost: 210000, grossContract: 266000,
            netInvest: 222000, monthly: 5541.67m, term: 48, made: 10, remaining: 38,
            booking: now.AddMonths(-10), finalPmt: now.AddMonths(38),
            creditRating: "CR3", province: "QC", industry: "Construction",
            isActive: true));

        deals.Add(MakeDeal(119011, AppStatus.Funded, "Excavation Pro Québec Inc.", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Construction (CONS)",
            equipType: "Wheel Loader (CAT 950M)", equipYear: 2023, equipCost: 145000, grossContract: 183700,
            netInvest: 153500, monthly: 3062.50m, term: 60, made: 16, remaining: 44,
            booking: now.AddMonths(-16), finalPmt: now.AddMonths(44),
            creditRating: "CR3", province: "QC", industry: "Construction",
            isActive: true, nsfCount: 1, lastNsf: now.AddMonths(-2)));

        deals.Add(MakeDeal(119012, AppStatus.Funded, "Excavation Pro Québec Inc.", "Équipements Nordiques Ltée",
            DealFormat.Broker, Lessor.MHCCA, repDaniel, "Construction (CONS)",
            equipType: "Backhoe (John Deere 310SL)", equipYear: 2024, equipCost: 95000, grossContract: 120350,
            netInvest: 100500, monthly: 2510.42m, term: 48, made: 4, remaining: 44,
            booking: now.AddMonths(-4), finalPmt: now.AddMonths(44),
            creditRating: "CR3", province: "QC", industry: "Construction",
            isActive: true));

        deals.Add(MakeDeal(118600, AppStatus.PaidOff, "Excavation Pro Québec Inc.", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Construction (CONS)",
            equipType: "Mini Excavator (Kubota KX040)", equipYear: 2019, equipCost: 55000, grossContract: 69700,
            netInvest: 0, monthly: 1937.50m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-42), finalPmt: now.AddMonths(-6),
            creditRating: "CR3", province: "QC", industry: "Construction",
            isActive: false));

        // ===== LOW-EXPOSURE: Prairie Grain Services Ltd. (~$116K active, Standard) =====
        deals.Add(MakeDeal(119020, AppStatus.Funded, "Prairie Grain Services Ltd.", "Brandt Tractor Ltd.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Agriculture (AGRI)",
            equipType: "Grain Dryer (GSI TopDry 1228)", equipYear: 2025, equipCost: 68000, grossContract: 86150,
            netInvest: 72000, monthly: 1795.83m, term: 48, made: 3, remaining: 45,
            booking: now.AddMonths(-3), finalPmt: now.AddMonths(45),
            creditRating: "CR1", province: "SK", industry: "Agriculture",
            isActive: true));

        deals.Add(MakeDeal(119021, AppStatus.Funded, "Prairie Grain Services Ltd.", "Brandt Tractor Ltd.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Agriculture (AGRI)",
            equipType: "Grain Auger (Convey-All BTS 1645)", equipYear: 2024, equipCost: 42000, grossContract: 53200,
            netInvest: 44500, monthly: 1479.17m, term: 36, made: 8, remaining: 28,
            booking: now.AddMonths(-8), finalPmt: now.AddMonths(28),
            creditRating: "CR1", province: "SK", industry: "Agriculture",
            isActive: true));

        // ===== PAID-OFF ONLY: Maritime Medical Supplies Inc. ($0 active) =====
        deals.Add(MakeDeal(117800, AppStatus.PaidOff, "Maritime Medical Supplies Inc.", "Medline Canada Corporation",
            DealFormat.Vendor, Lessor.MHCCL, repJames, "Medical (MED)",
            equipType: "MRI Scanner (Siemens Magnetom)", equipYear: 2018, equipCost: 320000, grossContract: 405440,
            netInvest: 0, monthly: 6757.33m, term: 60, made: 60, remaining: 0,
            booking: now.AddMonths(-66), finalPmt: now.AddMonths(-6),
            creditRating: "CR1", province: "NS", industry: "Medical",
            isActive: false));

        deals.Add(MakeDeal(117801, AppStatus.PaidOff, "Maritime Medical Supplies Inc.", "Medline Canada Corporation",
            DealFormat.Vendor, Lessor.MHCCL, repJames, "Medical (MED)",
            equipType: "Ultrasound System (GE Voluson)", equipYear: 2019, equipCost: 85000, grossContract: 107700,
            netInvest: 0, monthly: 2991.67m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-42), finalPmt: now.AddMonths(-6),
            creditRating: "CR1", province: "NS", industry: "Medical",
            isActive: false));

        deals.Add(MakeDeal(117802, AppStatus.PaidOff, "Maritime Medical Supplies Inc.", "Canadian Medical Equipment Ltd.",
            DealFormat.Broker, Lessor.MHCCA, repJames, "Medical (MED)",
            equipType: "Patient Monitor (Philips IntelliVue)", equipYear: 2020, equipCost: 45000, grossContract: 57020,
            netInvest: 0, monthly: 1583.89m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-40), finalPmt: now.AddMonths(-4),
            creditRating: "CR1", province: "NS", industry: "Medical",
            isActive: false));

        // ===== PROBLEMATIC: Coastal Demolition Corp. (~$265K active, Enhanced, 10 NSFs) =====
        deals.Add(MakeDeal(119030, AppStatus.Funded, "Coastal Demolition Corp.", "Finning International Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Demolition Excavator (Volvo EC380E)", equipYear: 2023, equipCost: 178000, grossContract: 225540,
            netInvest: 188500, monthly: 3760.00m, term: 60, made: 14, remaining: 46,
            booking: now.AddMonths(-14), finalPmt: now.AddMonths(46),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: true, nsfCount: 5, lastNsf: now.AddDays(-15),
            daysPastDue: 62, past31: 3760.00m, past61: 3760.00m));

        deals.Add(MakeDeal(119031, AppStatus.Funded, "Coastal Demolition Corp.", "Finning International Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Skid Steer (Bobcat T770)", equipYear: 2024, equipCost: 72000, grossContract: 91220,
            netInvest: 76200, monthly: 1270.28m, term: 72, made: 6, remaining: 66,
            booking: now.AddMonths(-6), finalPmt: now.AddMonths(66),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: true, nsfCount: 2, lastNsf: now.AddDays(-30),
            daysPastDue: 35, past31: 1270.28m));

        deals.Add(MakeDeal(118700, AppStatus.PaidOff, "Coastal Demolition Corp.", "Pacific Equipment Brokers",
            DealFormat.Broker, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Concrete Crusher (MB BF90.3)", equipYear: 2020, equipCost: 55000, grossContract: 69700,
            netInvest: 0, monthly: 1937.50m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-42), finalPmt: now.AddMonths(-6),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: false, nsfCount: 3, lastNsf: now.AddMonths(-8)));

        // ===== NEW CUSTOMER: TechVault Solutions Inc. (~$61K active, Standard) =====
        deals.Add(MakeDeal(119040, AppStatus.Funded, "TechVault Solutions Inc.", "CDW Canada Corp.",
            DealFormat.Vendor, Lessor.MHCCL, repJames, "Technology (TECH)",
            equipType: "Server Rack (Dell PowerEdge R760)", equipYear: 2025, equipCost: 58000, grossContract: 73480,
            netInvest: 61400, monthly: 2041.11m, term: 36, made: 1, remaining: 35,
            booking: now.AddMonths(-1), finalPmt: now.AddMonths(35),
            creditRating: "CR2", province: "ON", industry: "Technology",
            isActive: true));

        // ===== VENDOR DIVERSITY =====
        deals.Add(MakeDeal(119050, AppStatus.Funded, "Northern Logistics Group Inc.", "Fleet Equipment Corp.",
            DealFormat.Broker, Lessor.MHCCA, repEdwin, "Transportation (TRAN)",
            equipType: "Refrigerated Truck (Isuzu NPR-HD)", equipYear: 2024, equipCost: 78000, grossContract: 98840,
            netInvest: 82600, monthly: 2057.50m, term: 48, made: 8, remaining: 40,
            booking: now.AddMonths(-8), finalPmt: now.AddMonths(40),
            creditRating: "CR2", province: "AB", industry: "Transportation",
            isActive: true));

        deals.Add(MakeDeal(119051, AppStatus.Funded, "Northern Logistics Group Inc.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Box Truck (Hino 338)", equipYear: 2023, equipCost: 95000, grossContract: 120350,
            netInvest: 100600, monthly: 2510.42m, term: 48, made: 14, remaining: 34,
            booking: now.AddMonths(-14), finalPmt: now.AddMonths(34),
            creditRating: "CR2", province: "AB", industry: "Transportation",
            isActive: true, nsfCount: 1, lastNsf: now.AddMonths(-4)));

        deals.Add(MakeDeal(119060, AppStatus.Funded, "Alberta Earthworks Inc.", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Motor Grader (CAT 140)", equipYear: 2024, equipCost: 320000, grossContract: 405440,
            netInvest: 338800, monthly: 6756.67m, term: 60, made: 5, remaining: 55,
            booking: now.AddMonths(-5), finalPmt: now.AddMonths(55),
            creditRating: "CR3", province: "AB", industry: "Construction",
            isActive: true));

        // ===== PIPELINE DEALS (not yet funded — shows AppStatus variety) =====
        deals.Add(MakeDeal(119070, AppStatus.CreditValidation, "Québec Foresterie Ltée", "Équipements Nordiques Ltée",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Forestry (FOR)",
            equipType: "Forwarder (John Deere 1210G)", equipYear: 2025, equipCost: 420000, grossContract: 0,
            netInvest: 0, monthly: 0, term: 60, made: 0, remaining: 0,
            booking: null, finalPmt: null,
            creditRating: "CR3", province: "QC", industry: "Forestry",
            isActive: false));

        deals.Add(MakeDeal(119071, AppStatus.CreditReview, "Québec Foresterie Ltée", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Forestry (FOR)",
            equipType: "Harvester (Komatsu 951XC)", equipYear: 2024, equipCost: 385000, grossContract: 0,
            netInvest: 0, monthly: 0, term: 60, made: 0, remaining: 0,
            booking: null, finalPmt: null,
            creditRating: "CR3", province: "QC", industry: "Forestry",
            isActive: false));

        deals.Add(MakeDeal(119072, AppStatus.AutoscoringDeclined, "Smith & Sons Contracting Ltd.", "Finning International Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Track Loader (CAT 299D3)", equipYear: 2024, equipCost: 98000, grossContract: 0,
            netInvest: 0, monthly: 0, term: 48, made: 0, remaining: 0,
            booking: null, finalPmt: null,
            creditRating: "CR5", province: "BC", industry: "Construction",
            isActive: false));

        return deals;
    }

    private static Deal MakeDeal(
        int appNumber, string appStatus, string customer, string vendor,
        string format, string lessor, string accountManager, string equipCategory,
        string equipType, int equipYear, decimal equipCost, decimal grossContract,
        decimal netInvest, decimal monthly, int term, int made, int remaining,
        DateTimeOffset? booking, DateTimeOffset? finalPmt,
        string creditRating, string province, string industry,
        bool isActive, int nsfCount = 0, DateTimeOffset? lastNsf = null,
        int daysPastDue = 0, decimal past1 = 0, decimal past31 = 0,
        decimal past61 = 0, decimal past91 = 0)
    {
        return new Deal
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            AppNumber = appNumber,
            AppStatus = appStatus,
            CustomerLegalName = customer,
            PrimaryVendor = vendor,
            DealFormat = format,
            Lessor = lessor,
            AccountManager = accountManager,
            PrimaryEquipmentCategory = equipCategory,
            EquipmentType = equipType,
            EquipmentYear = equipYear,
            Amount = equipCost,
            TermMonths = term,
            Industry = industry,
            Province = province,
            CreditRating = creditRating,
            Status = appStatus is AppStatus.Funded or AppStatus.PaidOff
                ? DealStatus.Notified : DealStatus.Received,
            EquipmentCost = equipCost,
            GrossContract = grossContract,
            NetInvest = netInvest,
            MonthlyPayment = monthly,
            PaymentsMade = made,
            RemainingPayments = remaining,
            BookingDate = booking,
            FinalPaymentDate = finalPmt,
            IsActive = isActive,
            NsfCount = nsfCount,
            LastNsfDate = lastNsf,
            DaysPastDue = daysPastDue,
            Past1 = past1,
            Past31 = past31,
            Past61 = past61,
            Past91 = past91,
            CreatedAt = booking ?? DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
