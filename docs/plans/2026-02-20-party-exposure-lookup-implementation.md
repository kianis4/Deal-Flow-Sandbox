# Party Exposure Lookup — Implementation Plan

> **STATUS: Implemented** (2026-02-20) — All 12 tasks completed. See demo.md for walkthrough.

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a Party Exposure Lookup feature that lets users search by customer or vendor name and instantly see total exposure, NSFs, delinquency, and document requirements — replacing a manual 2-minute SSRS workflow.

**Architecture:** Expand the existing Deal entity with Vision-aligned fields and financial data. Add an exposure lookup endpoint to ReportingApi with summary aggregation and document-requirements logic. Serve a single-page web UI for demos. Seed 25-30 realistic deals.

**Tech Stack:** .NET 10, EF Core 10, PostgreSQL, Tailwind CSS CDN, vanilla JS

---

## Task 1: Add Domain Enums to DealFlow.Contracts

**Files:**
- Create: `src/DealFlow.Contracts/Domain/AppStatus.cs`
- Create: `src/DealFlow.Contracts/Domain/DealFormat.cs`
- Create: `src/DealFlow.Contracts/Domain/Lessor.cs`

**Step 1: Create AppStatus enum**

Create `src/DealFlow.Contracts/Domain/AppStatus.cs`:

```csharp
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
```

**Step 2: Create DealFormat enum**

Create `src/DealFlow.Contracts/Domain/DealFormat.cs`:

```csharp
namespace DealFlow.Contracts.Domain;

public static class DealFormat
{
    public const string Vendor = "VENDOR";
    public const string Broker = "BROKER";
}
```

**Step 3: Create Lessor enum**

Create `src/DealFlow.Contracts/Domain/Lessor.cs`:

```csharp
namespace DealFlow.Contracts.Domain;

public static class Lessor
{
    public const string MHCCL = "MHCCL";
    public const string MHCCA = "MHCCA";
}
```

**Step 4: Commit**

```bash
git add src/DealFlow.Contracts/Domain/AppStatus.cs src/DealFlow.Contracts/Domain/DealFormat.cs src/DealFlow.Contracts/Domain/Lessor.cs
git commit -m "feat: add AppStatus, DealFormat, Lessor domain enums"
```

---

## Task 2: Expand Deal Entity

**Files:**
- Modify: `src/DealFlow.Data/Entities/Deal.cs`

**Step 1: Add all new properties to the Deal class**

The existing Deal entity at `src/DealFlow.Data/Entities/Deal.cs` currently has 14 properties (Id through Events). Add the new Vision-aligned, financial, and delinquency fields AFTER the existing `UpdatedAt` property and BEFORE the `Events` collection:

```csharp
namespace DealFlow.Data.Entities;

public class Deal
{
    // === Existing fields (unchanged) ===
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public string EquipmentType { get; set; } = default!;
    public int EquipmentYear { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Industry { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string CreditRating { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int? Score { get; set; }
    public string? RiskFlag { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // === Vision-aligned fields ===
    public int? AppNumber { get; set; }
    public string? AppStatus { get; set; }
    public string? CustomerLegalName { get; set; }
    public string? PrimaryVendor { get; set; }
    public string? DealFormat { get; set; }
    public string? Lessor { get; set; }
    public string? AccountManager { get; set; }
    public string? PrimaryEquipmentCategory { get; set; }

    // === Financial fields ===
    public decimal EquipmentCost { get; set; }
    public decimal GrossContract { get; set; }
    public decimal NetInvest { get; set; }
    public decimal MonthlyPayment { get; set; }
    public int PaymentsMade { get; set; }
    public int RemainingPayments { get; set; }
    public DateTimeOffset? BookingDate { get; set; }
    public DateTimeOffset? FinalPaymentDate { get; set; }
    public bool IsActive { get; set; } = true;

    // === NSF & Delinquency ===
    public int NsfCount { get; set; }
    public DateTimeOffset? LastNsfDate { get; set; }
    public int DaysPastDue { get; set; }
    public decimal Past1 { get; set; }
    public decimal Past31 { get; set; }
    public decimal Past61 { get; set; }
    public decimal Past91 { get; set; }

    // === Navigation ===
    public ICollection<DealEvent> Events { get; set; } = new List<DealEvent>();
}
```

Note: All new Vision fields are nullable (`string?`, `int?`) so existing pipeline deals (submitted through IntakeApi without these fields) still work. Financial fields default to 0 or true for `IsActive`.

**Step 2: Commit**

```bash
git add src/DealFlow.Data/Entities/Deal.cs
git commit -m "feat: expand Deal entity with Vision-aligned and financial fields"
```

---

## Task 3: Update DbContext Configuration

**Files:**
- Modify: `src/DealFlow.Data/DealFlowDbContext.cs`

**Step 1: Add precision and length constraints for new columns**

Update the `OnModelCreating` method in `src/DealFlow.Data/DealFlowDbContext.cs`. The existing Deal configuration (lines 13-19) sets up `Id`, `Amount` precision, `CreditRating` max length, and `Events` relationship. Add new column configurations inside the same `modelBuilder.Entity<Deal>` block:

```csharp
modelBuilder.Entity<Deal>(e =>
{
    e.HasKey(x => x.Id);
    e.Property(x => x.Amount).HasPrecision(18, 2);
    e.Property(x => x.CreditRating).HasMaxLength(3);
    e.HasMany(x => x.Events).WithOne(x => x.Deal).HasForeignKey(x => x.DealId);

    // Vision-aligned
    e.Property(x => x.AppStatus).HasMaxLength(30);
    e.Property(x => x.CustomerLegalName).HasMaxLength(200);
    e.Property(x => x.PrimaryVendor).HasMaxLength(200);
    e.Property(x => x.DealFormat).HasMaxLength(10);
    e.Property(x => x.Lessor).HasMaxLength(10);
    e.Property(x => x.AccountManager).HasMaxLength(100);
    e.Property(x => x.PrimaryEquipmentCategory).HasMaxLength(100);

    // Financial
    e.Property(x => x.EquipmentCost).HasPrecision(18, 2);
    e.Property(x => x.GrossContract).HasPrecision(18, 2);
    e.Property(x => x.NetInvest).HasPrecision(18, 2);
    e.Property(x => x.MonthlyPayment).HasPrecision(18, 2);

    // Delinquency
    e.Property(x => x.Past1).HasPrecision(18, 2);
    e.Property(x => x.Past31).HasPrecision(18, 2);
    e.Property(x => x.Past61).HasPrecision(18, 2);
    e.Property(x => x.Past91).HasPrecision(18, 2);

    // Indexes for exposure lookups
    e.HasIndex(x => x.CustomerLegalName);
    e.HasIndex(x => x.PrimaryVendor);
    e.HasIndex(x => x.IsActive);
});
```

**Step 2: Commit**

```bash
git add src/DealFlow.Data/DealFlowDbContext.cs
git commit -m "feat: configure new Deal columns with precision and indexes"
```

---

## Task 4: Create EF Migration

**Step 1: Generate the migration**

Run from the repo root:

```bash
cd /Users/suley/ProductionApps/Deal-Flow-Sandbox
dotnet ef migrations add AddExposureFields --project src/DealFlow.Data --startup-project src/DealFlow.IntakeApi
```

Expected: Creates a new migration file in `src/DealFlow.Data/Migrations/` with `AddExposureFields` suffix.

**Step 2: Review the generated migration**

Open the generated migration and verify it has:
- `AddColumn` calls for all ~23 new columns
- `CreateIndex` calls for `CustomerLegalName`, `PrimaryVendor`, `IsActive`
- No destructive changes to existing columns

**Step 3: Commit**

```bash
git add src/DealFlow.Data/Migrations/
git commit -m "feat: add migration for exposure fields"
```

---

## Task 5: Seed Realistic Demo Data

**Files:**
- Create: `src/DealFlow.Data/SeedData.cs`
- Modify: `src/DealFlow.IntakeApi/Program.cs` (add seed call after migration)

**Step 1: Create SeedData class**

Create `src/DealFlow.Data/SeedData.cs` with a static method that seeds ~28 deals across multiple customers and vendors. The method should be idempotent (skip if data exists).

```csharp
using DealFlow.Contracts.Domain;
using DealFlow.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealFlow.Data;

public static class SeedData
{
    public static async Task SeedAsync(DealFlowDbContext db)
    {
        if (await db.Deals.AnyAsync(d => d.AppNumber != null))
            return; // Already seeded

        var deals = BuildDeals();
        db.Deals.AddRange(deals);
        await db.SaveChangesAsync();
    }

    private static List<Deal> BuildDeals()
    {
        var deals = new List<Deal>();
        var now = DateTimeOffset.UtcNow;

        // ===== ACCOUNT MANAGERS =====
        const string repEdwin = "Edwin Van Schepen";
        const string repDaniel = "Daniel De Luca";
        const string repSarah = "Sarah Mitchell";
        const string repJames = "James Wong";

        // ===== HIGH-EXPOSURE CUSTOMER: TransCanada Hauling Ltd. ($1.3M+, Full Review) =====
        // 4 active + 2 paid-off, 3 NSFs total, 1 delinquent deal
        deals.Add(MakeDeal(119001, AppStatus.Funded, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Semi-Truck (Kenworth T680)", equipYear: 2024, equipCost: 185000, grossContract: 234500,
            netInvest: 195000, monthly: 4885.42m, term: 48, made: 12, remaining: 36,
            booking: now.AddMonths(-12), finalPmt: now.AddMonths(36),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true, nsfCount: 2, lastNsf: now.AddMonths(-3), daysPastDue: 0));

        deals.Add(MakeDeal(119002, AppStatus.Funded, "TransCanada Hauling Ltd.", "National Truck Centre Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repEdwin, "Transportation (TRAN)",
            equipType: "Semi-Truck (Peterbilt 579)", equipYear: 2023, equipCost: 175000, grossContract: 221800,
            netInvest: 185000, monthly: 4620.83m, term: 48, made: 18, remaining: 30,
            booking: now.AddMonths(-18), finalPmt: now.AddMonths(30),
            creditRating: "CR2", province: "ON", industry: "Transportation",
            isActive: true, nsfCount: 1, lastNsf: now.AddMonths(-6), daysPastDue: 0));

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
            isActive: true, daysPastDue: 45, past1: 0, past31: 2427.50m));

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

        // ===== MID-EXPOSURE CUSTOMER: Excavation Pro Québec Inc. ($450K, Enhanced) =====
        // 3 active + 1 paid-off, 1 NSF
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

        // ===== LOW-EXPOSURE CUSTOMER: Prairie Grain Services Ltd. ($120K, Standard) =====
        // 2 active, no NSFs, clean
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

        // ===== PAID-OFF ONLY CUSTOMER: Maritime Medical Supplies Inc. =====
        // 0 active, 3 paid-off
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
            equipType: "Patient Monitor System (Philips IntelliVue)", equipYear: 2020, equipCost: 45000, grossContract: 57020,
            netInvest: 0, monthly: 1583.89m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-40), finalPmt: now.AddMonths(-4),
            creditRating: "CR1", province: "NS", industry: "Medical",
            isActive: false));

        // ===== PROBLEMATIC CUSTOMER: Coastal Demolition Corp. ($300K, Enhanced, lots of NSFs) =====
        // 2 active + 1 paid, 7 NSFs total, 2 delinquent
        deals.Add(MakeDeal(119030, AppStatus.Funded, "Coastal Demolition Corp.", "Finning International Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Demolition Excavator (Volvo EC380E)", equipYear: 2023, equipCost: 178000, grossContract: 225540,
            netInvest: 188500, monthly: 3760.00m, term: 60, made: 14, remaining: 46,
            booking: now.AddMonths(-14), finalPmt: now.AddMonths(46),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: true, nsfCount: 5, lastNsf: now.AddDays(-15),
            daysPastDue: 62, past1: 0, past31: 3760.00m, past61: 3760.00m));

        deals.Add(MakeDeal(119031, AppStatus.Funded, "Coastal Demolition Corp.", "Finning International Inc.",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Skid Steer (Bobcat T770)", equipYear: 2024, equipCost: 72000, grossContract: 91220,
            netInvest: 76200, monthly: 1270.28m, term: 72, made: 6, remaining: 66,
            booking: now.AddMonths(-6), finalPmt: now.AddMonths(66),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: true, nsfCount: 2, lastNsf: now.AddDays(-30),
            daysPastDue: 35, past1: 0, past31: 1270.28m));

        deals.Add(MakeDeal(118700, AppStatus.PaidOff, "Coastal Demolition Corp.", "Pacific Equipment Brokers",
            DealFormat.Broker, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Concrete Crusher (MB BF90.3)", equipYear: 2020, equipCost: 55000, grossContract: 69700,
            netInvest: 0, monthly: 1937.50m, term: 36, made: 36, remaining: 0,
            booking: now.AddMonths(-42), finalPmt: now.AddMonths(-6),
            creditRating: "CR4", province: "BC", industry: "Construction",
            isActive: false, nsfCount: 3, lastNsf: now.AddMonths(-8)));

        // ===== NEW CUSTOMER: TechVault Solutions Inc. ($60K, Standard, brand new) =====
        // 1 active, 0 NSF
        deals.Add(MakeDeal(119040, AppStatus.Funded, "TechVault Solutions Inc.", "CDW Canada Corp.",
            DealFormat.Vendor, Lessor.MHCCL, repJames, "Technology (TECH)",
            equipType: "Server Rack (Dell PowerEdge R760)", equipYear: 2025, equipCost: 58000, grossContract: 73480,
            netInvest: 61400, monthly: 2041.11m, term: 36, made: 1, remaining: 35,
            booking: now.AddMonths(-1), finalPmt: now.AddMonths(35),
            creditRating: "CR2", province: "ON", industry: "Technology",
            isActive: true));

        // ===== ADDITIONAL VENDOR DIVERSITY DEALS =====

        // Broker-format deal for Fleet Equipment Corp (vendor lookup)
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

        // Multi-lessor vendor: Strongco sells to both MHCCL and MHCCA
        deals.Add(MakeDeal(119060, AppStatus.Funded, "Alberta Earthworks Inc.", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCL, repSarah, "Construction (CONS)",
            equipType: "Motor Grader (CAT 140)", equipYear: 2024, equipCost: 320000, grossContract: 405440,
            netInvest: 338800, monthly: 6756.67m, term: 60, made: 5, remaining: 55,
            booking: now.AddMonths(-5), finalPmt: now.AddMonths(55),
            creditRating: "CR3", province: "AB", industry: "Construction",
            isActive: true));

        // Deals in Credit Review pipeline (not yet funded - tests AppStatus variety)
        deals.Add(MakeDeal(119070, AppStatus.CreditValidation, "Québec Foresterie Ltée", "Équipements Nordiques Ltée",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Agriculture (AGRI)",
            equipType: "Forwarder (John Deere 1210G)", equipYear: 2025, equipCost: 420000, grossContract: 0,
            netInvest: 0, monthly: 0, term: 60, made: 0, remaining: 0,
            booking: null, finalPmt: null,
            creditRating: "CR3", province: "QC", industry: "Forestry",
            isActive: false));

        deals.Add(MakeDeal(119071, AppStatus.CreditReview, "Québec Foresterie Ltée", "Strongco Corporation",
            DealFormat.Vendor, Lessor.MHCCA, repDaniel, "Agriculture (AGRI)",
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

            // Vision-aligned
            AppNumber = appNumber,
            AppStatus = appStatus,
            CustomerLegalName = customer,
            PrimaryVendor = vendor,
            DealFormat = format,
            Lessor = lessor,
            AccountManager = accountManager,
            PrimaryEquipmentCategory = equipCategory,

            // Original fields
            EquipmentType = equipType,
            EquipmentYear = equipYear,
            Amount = equipCost, // Amount maps to equipment cost for pipeline compat
            TermMonths = term,
            Industry = industry,
            Province = province,
            CreditRating = creditRating,
            Status = appStatus is AppStatus.Funded or AppStatus.PaidOff
                ? DealStatus.Notified : DealStatus.Received,

            // Financial
            EquipmentCost = equipCost,
            GrossContract = grossContract,
            NetInvest = netInvest,
            MonthlyPayment = monthly,
            PaymentsMade = made,
            RemainingPayments = remaining,
            BookingDate = booking,
            FinalPaymentDate = finalPmt,
            IsActive = isActive,

            // NSF & Delinquency
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
```

**Step 2: Wire seed into IntakeApi startup**

In `src/DealFlow.IntakeApi/Program.cs`, right after the existing migration block (lines 48-52), add the seed call:

```csharp
// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DealFlowDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}
```

Requires adding `using DealFlow.Data;` at the top (already present).

**Step 3: Verify by building**

```bash
dotnet build src/DealFlow.IntakeApi/
```

Expected: Build succeeds.

**Step 4: Commit**

```bash
git add src/DealFlow.Data/SeedData.cs src/DealFlow.IntakeApi/Program.cs
git commit -m "feat: seed 28 realistic deals for exposure demo"
```

---

## Task 6: Add Exposure DTOs to ReportingApi

**Files:**
- Create: `src/DealFlow.ReportingApi/Models/ExposureModels.cs`

**Step 1: Create the response model records**

Create `src/DealFlow.ReportingApi/Models/ExposureModels.cs`:

```csharp
namespace DealFlow.ReportingApi.Models;

public record ExposureResponse(
    string PartyName,
    string SearchType,
    ExposureSummary Summary,
    DocumentRequirements DocumentRequirements,
    List<ExposureDeal> Deals
);

public record ExposureSummary(
    int TotalDeals,
    int ActiveDeals,
    int PaidOffDeals,
    decimal TotalNetExposure,
    decimal TotalGrossContract,
    int TotalNsfCount,
    DateTimeOffset? LastNsfDate,
    int DealsWithNsfs,
    int DealsDelinquent,
    decimal TotalPastDue
);

public record DocumentRequirements(
    string Tier,
    decimal TotalNetExposure,
    List<string> Requirements,
    string Note
);

public record ExposureDeal(
    Guid Id,
    int? AppNumber,
    string? AppStatus,
    string? CustomerLegalName,
    string? PrimaryVendor,
    string? DealFormat,
    string? Lessor,
    string? AccountManager,
    string? PrimaryEquipmentCategory,
    string CreditRating,
    decimal EquipmentCost,
    decimal GrossContract,
    decimal NetInvest,
    decimal MonthlyPayment,
    int TermMonths,
    int PaymentsMade,
    int RemainingPayments,
    DateTimeOffset? BookingDate,
    bool IsActive,
    int NsfCount,
    DateTimeOffset? LastNsfDate,
    int DaysPastDue,
    decimal Past1,
    decimal Past31,
    decimal Past61,
    decimal Past91
);
```

**Step 2: Commit**

```bash
git add src/DealFlow.ReportingApi/Models/ExposureModels.cs
git commit -m "feat: add exposure response DTOs"
```

---

## Task 7: Document Requirements Logic

**Files:**
- Create: `src/DealFlow.ReportingApi/Services/DocumentRequirementsService.cs`
- Modify: `src/DealFlow.ReportingApi/appsettings.json` (add thresholds)

**Step 1: Write the failing test**

Create `tests/DealFlow.ReportingApi.Tests/DocumentRequirementsTests.cs`:

```csharp
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
```

**Step 2: Run test to verify it fails**

```bash
dotnet test tests/DealFlow.ReportingApi.Tests/ --filter "DocumentRequirements"
```

Expected: Build failure — `DocumentRequirementsService` and `ExposureThresholdOptions` do not exist yet.

**Step 3: Implement DocumentRequirementsService**

Create `src/DealFlow.ReportingApi/Services/DocumentRequirementsService.cs`:

```csharp
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
```

**Step 4: Add thresholds to appsettings.json**

Add to `src/DealFlow.ReportingApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dealflow;Username=dealflow;Password=dealflow"
  },
  "ExposureThresholds": {
    "EnhancedThreshold": 250000,
    "FullReviewThreshold": 1000000
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Step 5: Run tests**

```bash
dotnet test tests/DealFlow.ReportingApi.Tests/ --filter "DocumentRequirements"
```

Expected: All 6 tests pass.

**Step 6: Commit**

```bash
git add src/DealFlow.ReportingApi/Services/DocumentRequirementsService.cs src/DealFlow.ReportingApi/appsettings.json tests/DealFlow.ReportingApi.Tests/DocumentRequirementsTests.cs
git commit -m "feat: add document requirements engine with threshold logic"
```

---

## Task 8: Exposure API Endpoint

**Files:**
- Modify: `src/DealFlow.ReportingApi/Program.cs`

**Step 1: Register DocumentRequirementsService and add the endpoint**

In `src/DealFlow.ReportingApi/Program.cs`:

1. Add service registration after `builder.Services.AddOpenApi();`:

```csharp
using DealFlow.ReportingApi.Services;

// ... existing code ...

builder.Services.Configure<ExposureThresholdOptions>(
    builder.Configuration.GetSection("ExposureThresholds"));
builder.Services.AddSingleton<DocumentRequirementsService>();
```

2. Add the exposure endpoint after the existing `/api/v1/deals/{id}/timeline` endpoint:

```csharp
// GET /api/v1/exposure
app.MapGet("/api/v1/exposure", async (
    string searchType,
    string name,
    bool? includePastDeals,
    DealFlowDbContext db,
    DocumentRequirementsService docReqs) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { error = "name parameter is required" });

    if (searchType is not ("customer" or "vendor"))
        return Results.BadRequest(new { error = "searchType must be 'customer' or 'vendor'" });

    var include = includePastDeals ?? false;
    var nameLower = name.ToLower();

    var query = searchType == "customer"
        ? db.Deals.Where(d => d.CustomerLegalName != null && d.CustomerLegalName.ToLower().Contains(nameLower))
        : db.Deals.Where(d => d.PrimaryVendor != null && d.PrimaryVendor.ToLower().Contains(nameLower));

    if (!include)
        query = query.Where(d => d.IsActive);

    var deals = await query
        .OrderByDescending(d => d.IsActive)
        .ThenByDescending(d => d.NetInvest)
        .Select(d => new ExposureDeal(
            d.Id, d.AppNumber, d.AppStatus, d.CustomerLegalName, d.PrimaryVendor,
            d.DealFormat, d.Lessor, d.AccountManager, d.PrimaryEquipmentCategory,
            d.CreditRating, d.EquipmentCost, d.GrossContract, d.NetInvest,
            d.MonthlyPayment, d.TermMonths, d.PaymentsMade, d.RemainingPayments,
            d.BookingDate, d.IsActive, d.NsfCount, d.LastNsfDate,
            d.DaysPastDue, d.Past1, d.Past31, d.Past61, d.Past91))
        .ToListAsync();

    if (deals.Count == 0)
        return Results.Ok(new { message = "No deals found", searchType, name });

    var activeDeals = deals.Where(d => d.IsActive).ToList();
    var totalNetExposure = activeDeals.Sum(d => d.NetInvest);

    var summary = new ExposureSummary(
        TotalDeals: deals.Count,
        ActiveDeals: activeDeals.Count,
        PaidOffDeals: deals.Count(d => !d.IsActive),
        TotalNetExposure: totalNetExposure,
        TotalGrossContract: activeDeals.Sum(d => d.GrossContract),
        TotalNsfCount: deals.Sum(d => d.NsfCount),
        LastNsfDate: deals.Where(d => d.LastNsfDate.HasValue)
            .Select(d => d.LastNsfDate!.Value)
            .OrderDescending().FirstOrDefault(),
        DealsWithNsfs: deals.Count(d => d.NsfCount > 0),
        DealsDelinquent: deals.Count(d => d.DaysPastDue > 0),
        TotalPastDue: deals.Sum(d => d.Past1 + d.Past31 + d.Past61 + d.Past91)
    );

    var partyName = searchType == "customer"
        ? deals.First().CustomerLegalName ?? name
        : deals.First().PrimaryVendor ?? name;

    var response = new ExposureResponse(
        PartyName: partyName,
        SearchType: searchType,
        Summary: summary,
        DocumentRequirements: docReqs.Evaluate(totalNetExposure),
        Deals: deals
    );

    return Results.Ok(response);
})
.WithName("GetExposure");
```

**Step 2: Add static files middleware for the web UI (prep for Task 9)**

Before `app.Run();`, add:

```csharp
app.UseStaticFiles();
```

**Step 3: Build and verify**

```bash
dotnet build src/DealFlow.ReportingApi/
```

Expected: Build succeeds.

**Step 4: Commit**

```bash
git add src/DealFlow.ReportingApi/Program.cs
git commit -m "feat: add GET /api/v1/exposure endpoint with aggregation"
```

---

## Task 9: Web UI

**Files:**
- Create: `src/DealFlow.ReportingApi/wwwroot/index.html`

**Step 1: Create the exposure lookup web page**

Create `src/DealFlow.ReportingApi/wwwroot/index.html`:

A single-page HTML with:
- Tailwind CSS CDN for styling
- Search form: dropdown (Customer/Vendor), text input, "Include past deals" checkbox
- Document Requirements banner (green/amber/red)
- Summary cards row (Total Net Exposure, Active Deals, NSFs, Delinquent)
- Deals data table with all fields
- Vanilla JS fetch to `/api/v1/exposure`

The HTML should be a complete, functional page (~300 lines). Key features:
- Responsive layout
- Color-coded document requirements banner
- NSF and delinquency values highlighted in amber/red when > 0
- Active deals in normal rows, paid-off deals in slightly muted rows
- Currency formatting for all dollar amounts
- Date formatting for all dates
- Loading state while fetching
- Empty state when no results

**Step 2: Verify static file serving**

```bash
dotnet build src/DealFlow.ReportingApi/
```

**Step 3: Commit**

```bash
git add src/DealFlow.ReportingApi/wwwroot/
git commit -m "feat: add exposure lookup web UI"
```

---

## Task 10: Update IntakeApi for New Fields

**Files:**
- Modify: `src/DealFlow.IntakeApi/Models/SubmitDealRequest.cs`
- Modify: `src/DealFlow.IntakeApi/Models/DealResponse.cs`
- Modify: `src/DealFlow.IntakeApi/Program.cs` (deal creation mapping)

**Step 1: Add optional fields to SubmitDealRequest**

All new fields are optional in the submit request so the existing scoring pipeline still works:

```csharp
namespace DealFlow.IntakeApi.Models;

public record SubmitDealRequest(
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string CreditRating,
    // Optional Vision-aligned fields
    int? AppNumber = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? DealFormat = null,
    string? Lessor = null,
    string? AccountManager = null,
    string? PrimaryEquipmentCategory = null,
    // Optional financial fields
    decimal? EquipmentCost = null,
    decimal? GrossContract = null,
    decimal? NetInvest = null,
    decimal? MonthlyPayment = null
);
```

**Step 2: Expand DealResponse**

```csharp
namespace DealFlow.IntakeApi.Models;

public record DealResponse(
    Guid Id,
    Guid CorrelationId,
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string CreditRating,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int? AppNumber = null,
    string? AppStatus = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? DealFormat = null,
    string? Lessor = null,
    string? AccountManager = null,
    string? PrimaryEquipmentCategory = null,
    decimal? NetInvest = null,
    bool IsActive = true
);
```

**Step 3: Update deal creation mapping in Program.cs**

In the POST `/api/v1/deals` handler (around line 73), add mappings for the new fields when creating the Deal:

```csharp
var deal = new Deal
{
    // ... existing fields ...
    AppNumber = request.AppNumber,
    CustomerLegalName = request.CustomerLegalName,
    PrimaryVendor = request.PrimaryVendor,
    DealFormat = request.DealFormat,
    Lessor = request.Lessor,
    AccountManager = request.AccountManager,
    PrimaryEquipmentCategory = request.PrimaryEquipmentCategory,
    EquipmentCost = request.EquipmentCost ?? request.Amount,
    GrossContract = request.GrossContract ?? 0,
    NetInvest = request.NetInvest ?? 0,
    MonthlyPayment = request.MonthlyPayment ?? 0,
};
```

Update the `ToResponse` helper to include the new fields:

```csharp
static DealResponse ToResponse(Deal d) => new(
    d.Id, d.CorrelationId, d.EquipmentType, d.EquipmentYear,
    d.Amount, d.TermMonths, d.Industry, d.Province,
    d.CreditRating, d.Status, d.Score, d.RiskFlag,
    d.CreatedAt, d.UpdatedAt,
    d.AppNumber, d.AppStatus, d.CustomerLegalName, d.PrimaryVendor,
    d.DealFormat, d.Lessor, d.AccountManager, d.PrimaryEquipmentCategory,
    d.NetInvest, d.IsActive);
```

**Step 4: Build**

```bash
dotnet build src/DealFlow.IntakeApi/
```

**Step 5: Commit**

```bash
git add src/DealFlow.IntakeApi/
git commit -m "feat: expand IntakeApi request/response for Vision fields"
```

---

## Task 11: Update ReportingApi DealSummary

**Files:**
- Modify: `src/DealFlow.ReportingApi/Models/DealSummary.cs`
- Modify: `src/DealFlow.ReportingApi/Program.cs` (ListDeals query)

**Step 1: Add key fields to DealSummary**

```csharp
namespace DealFlow.ReportingApi.Models;

public record DealSummary(
    Guid Id,
    string EquipmentType,
    decimal Amount,
    string CreditRating,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt,
    int? AppNumber = null,
    string? CustomerLegalName = null,
    string? PrimaryVendor = null,
    string? Lessor = null,
    bool IsActive = true
);

public record TimelineEvent(
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt
);
```

**Step 2: Update the ListDeals query projection**

In `src/DealFlow.ReportingApi/Program.cs`, update the `.Select()` in the ListDeals endpoint:

```csharp
.Select(d => new DealSummary(
    d.Id, d.EquipmentType, d.Amount, d.CreditRating,
    d.Status, d.Score, d.RiskFlag, d.CreatedAt,
    d.AppNumber, d.CustomerLegalName, d.PrimaryVendor,
    d.Lessor, d.IsActive))
```

**Step 3: Fix existing test**

Update `tests/DealFlow.ReportingApi.Tests/ReportingApiTests.cs` to use the new DealSummary signature:

```csharp
var a = new DealSummary(id, "Semi-Truck (Kenworth T680)", 185_000, "CR1", "RECEIVED", null, null, now);
```

This still works because the new fields are optional with defaults. Verify:

```bash
dotnet test tests/DealFlow.ReportingApi.Tests/
```

**Step 4: Commit**

```bash
git add src/DealFlow.ReportingApi/ tests/DealFlow.ReportingApi.Tests/
git commit -m "feat: expand ReportingApi DealSummary with Vision fields"
```

---

## Task 12: Integration Smoke Test

**Step 1: Start Docker infrastructure**

```bash
docker compose up -d postgres rabbitmq
```

**Step 2: Run IntakeApi to migrate + seed**

```bash
dotnet run --project src/DealFlow.IntakeApi/ &
sleep 5
```

Wait for migration and seed to complete. Check logs for "seed" output.

**Step 3: Test the exposure endpoint**

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=TransCanada" | jq .
```

Expected: JSON response with TransCanada Hauling deals, $535K+ active net exposure, FullReview tier.

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=vendor&name=Strongco" | jq .
```

Expected: JSON response with Strongco deals across multiple customers.

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=Coastal&includePastDeals=true" | jq .
```

Expected: Coastal Demolition with active + paid deals, 7+ NSFs total.

**Step 4: Visit the web UI**

Open `http://localhost:5002/index.html` in a browser. Test searching.

**Step 5: Run all tests**

```bash
dotnet test --verbosity normal
```

Expected: All tests pass.

**Step 6: Final commit**

```bash
git add -A
git commit -m "feat: party exposure lookup — complete feature"
```

---

## Summary of All Files Changed/Created

| Action | File |
|--------|------|
| Create | `src/DealFlow.Contracts/Domain/AppStatus.cs` |
| Create | `src/DealFlow.Contracts/Domain/DealFormat.cs` |
| Create | `src/DealFlow.Contracts/Domain/Lessor.cs` |
| Modify | `src/DealFlow.Data/Entities/Deal.cs` |
| Modify | `src/DealFlow.Data/DealFlowDbContext.cs` |
| Create | `src/DealFlow.Data/Migrations/*_AddExposureFields.cs` (generated) |
| Create | `src/DealFlow.Data/SeedData.cs` |
| Modify | `src/DealFlow.IntakeApi/Program.cs` |
| Modify | `src/DealFlow.IntakeApi/Models/SubmitDealRequest.cs` |
| Modify | `src/DealFlow.IntakeApi/Models/DealResponse.cs` |
| Create | `src/DealFlow.ReportingApi/Models/ExposureModels.cs` |
| Create | `src/DealFlow.ReportingApi/Services/DocumentRequirementsService.cs` |
| Modify | `src/DealFlow.ReportingApi/Program.cs` |
| Modify | `src/DealFlow.ReportingApi/appsettings.json` |
| Modify | `src/DealFlow.ReportingApi/Models/DealSummary.cs` |
| Create | `src/DealFlow.ReportingApi/wwwroot/index.html` |
| Create | `tests/DealFlow.ReportingApi.Tests/DocumentRequirementsTests.cs` |
