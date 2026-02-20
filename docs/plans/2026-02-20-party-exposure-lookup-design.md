# Party Exposure Lookup — Feature Design

**Date:** 2026-02-20
**Status:** Implemented (2026-02-20)

## Problem

Before submitting a deal, the credit team must check the customer's total exposure across all existing deals. Today this requires:

1. Opening the SSRS reporting site (IFL_ALL_DEAL_ACTIVE)
2. Entering a client or dealer name
3. Waiting 2+ minutes for the database to return results
4. Manually scrolling through deals, writing down net invest amounts
5. Tallying up total net exposure by hand
6. Counting NSFs and noting which deals have them
7. Determining document requirements based on thresholds

This is slow, error-prone, and repeated for every deal submission.

## Solution

Add a **Party Exposure Lookup** feature to the DealFlow Sandbox that:

- Lets users type a party name (customer or vendor) and instantly get a complete exposure summary
- Shows all deals for that party with active/paid status
- Automatically calculates total net exposure, NSF summary, and delinquency info
- Tells the user exactly what documents are required based on exposure thresholds
- Provides both an API endpoint and a web UI for demos

## Data Source

Self-contained PostgreSQL database with seeded realistic demo data. No live connection to the production SSRS/SQL Server system.

---

## 1. Expanded Deal Model

### New Vision-Aligned Fields

| Field | Type | Example | Notes |
|---|---|---|---|
| `AppNumber` | int | 119948 | Vision application number |
| `AppStatus` | enum | Funded | See AppStatus enum below |
| `CustomerLegalName` | string | "Randhawa Freightways Ltd." | Customer legal name |
| `PrimaryVendor` | string | "Target Truck Sales & Leasing Inc." | Vendor/dealer name |
| `DealFormat` | enum | Vendor | Vendor or Broker |
| `Lessor` | enum | MHCCL | MHCCL or MHCCA |
| `AccountManager` | string | "Edwin Van Schepen" | Account manager/rep |
| `PrimaryEquipmentCategory` | string | "Transportation (TRAN)" | Equipment category |

### New Financial Fields

| Field | Type | Example | Notes |
|---|---|---|---|
| `EquipmentCost` | decimal(18,2) | 56400.00 | From SSRS EQUIPMENT_COST |
| `GrossContract` | decimal(18,2) | 71371.12 | From SSRS GROSS_CONTRACT |
| `NetInvest` | decimal(18,2) | 58142.95 | From SSRS LS_NET_INVEST — key exposure field |
| `MonthlyPayment` | decimal(18,2) | 1486.69 | From SSRS CONTRACT_PYMT |
| `PaymentsMade` | int | 1 | From SSRS PYMTS_MADE |
| `RemainingPayments` | int | 47 | From SSRS REMAINING_PMT |
| `BookingDate` | DateTimeOffset | 2026-02-10 | From SSRS BOOKING_DATE |
| `FinalPaymentDate` | DateTimeOffset | 2030-01-10 | From SSRS FINAL_PYMT_DATE |
| `IsActive` | bool | true | true = active, false = paid off |

### NSF & Delinquency Fields

| Field | Type | Example | Notes |
|---|---|---|---|
| `NsfCount` | int | 0 | From SSRS NB_NSF |
| `LastNsfDate` | DateTimeOffset? | null | From SSRS LAST_NSF |
| `DaysPastDue` | int | 0 | From SSRS DAYS_PAST_DUE |
| `Past1` | decimal(18,2) | 0.00 | 1-30 days past due amount |
| `Past31` | decimal(18,2) | 0.00 | 31-60 days past due |
| `Past61` | decimal(18,2) | 0.00 | 61-90 days past due |
| `Past91` | decimal(18,2) | 0.00 | 91+ days past due |

### Existing Fields (Unchanged)

All current fields remain: `Id`, `CorrelationId`, `EquipmentType`, `EquipmentYear`, `Amount`, `TermMonths`, `Industry`, `Province`, `CreditRating`, `Status`, `Score`, `RiskFlag`, `CreatedAt`, `UpdatedAt`, `Events`.

---

## 2. New Enums

### AppStatus
```
CreditValidation, CreditReview, AutoscoringApproved, AutoscoringDeclined,
MissingInfo, DealDeclined, Funded, PaidOff
```

### DealFormat
```
Vendor, Broker
```

### Lessor
```
MHCCL, MHCCA
```

---

## 3. Exposure Lookup API

### Endpoint

`GET /api/v1/exposure` on ReportingApi (port 5002)

### Query Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `searchType` | string | Yes | — | `customer` or `vendor` |
| `name` | string | Yes | — | Party name (partial/fuzzy match) |
| `includePastDeals` | bool | No | false | Include paid-off deals |

### Response Shape

```json
{
  "partyName": "Randhawa Freightways Ltd.",
  "searchType": "customer",
  "summary": {
    "totalDeals": 5,
    "activeDeals": 3,
    "paidOffDeals": 2,
    "totalNetExposure": 174428.85,
    "totalGrossContract": 214113.36,
    "totalNsfCount": 2,
    "lastNsfDate": "2025-08-15",
    "dealsWithNsfs": 1,
    "dealsDelinquent": 0,
    "totalPastDue": 0.00
  },
  "documentRequirements": {
    "tier": "Standard | Enhanced | FullReview",
    "totalNetExposure": 174428.85,
    "requirements": ["..."],
    "note": "..."
  },
  "deals": [
    { "...per-deal fields..." }
  ]
}
```

---

## 4. Document Requirements Engine

Business rules based on **total net exposure of active deals**:

| Total Net Exposure | Tier | Requirements |
|---|---|---|
| < $250,000 | **Standard** | No bank statements needed |
| $250,000 – $999,999 | **Enhanced** | Bank statements or financial statements required |
| >= $1,000,000 | **Full Review** | 3-year financial statements + interims required (for spreads) |

Thresholds configurable via `appsettings.json`:

```json
{
  "ExposureThresholds": {
    "EnhancedThreshold": 250000,
    "FullReviewThreshold": 1000000
  }
}
```

---

## 5. Web UI

Single HTML page served at `/exposure` by ReportingApi.

### Components
1. **Search bar** — dropdown (Customer/Vendor), text input, "Include past deals" checkbox, Search button
2. **Document Requirements banner** — color-coded: green (Standard), amber (Enhanced), red (Full Review)
3. **Summary cards** — Total Net Exposure, Active/Paid deal counts, NSF count, Last NSF date, Delinquent count
4. **Deals table** — sortable, with NSF and delinquency columns highlighted

### Tech
- Single HTML file with Tailwind CSS CDN
- Vanilla JavaScript, no build step
- Calls the exposure API endpoint

---

## 6. Seed Data

~25-30 deals across 6-8 customers and 4-5 vendors:

### Customer Scenarios
| Customer | Active Deals | Paid Deals | Exposure | NSFs | Delinquent | Doc Tier |
|---|---|---|---|---|---|---|
| High-exposure customer | 4 | 2 | ~$1.3M | 3 | 1 | Full Review |
| Mid-exposure customer | 3 | 1 | ~$450K | 1 | 0 | Enhanced |
| Low-exposure customer | 2 | 0 | ~$120K | 0 | 0 | Standard |
| Paid-off customer | 0 | 3 | $0 active | 0 | 0 | Standard |
| Problematic customer | 2 | 1 | ~$300K | 7 | 2 | Enhanced |
| New customer | 1 | 0 | ~$60K | 0 | 0 | Standard |

### Vendor coverage
- High-volume vendor (10+ deals across customers)
- Broker-format vendor
- Vendor with both MHCCL and MHCCA deals
- Small vendor (1-2 deals)

### Equipment categories
Transportation, Construction, Agriculture, Medical, Technology

### Account managers
3-4 reps: Edwin Van Schepen, Daniel De Luca, Sarah Mitchell, James Wong

---

## 7. Implementation Order

1. Add enums (`AppStatus`, `DealFormat`, `Lessor`) to DealFlow.Contracts
2. Expand `Deal` entity with all new fields
3. Update `DealFlowDbContext` with new column configurations
4. Create EF migration
5. Build seed data (migration or DbContext seed)
6. Add exposure DTOs to ReportingApi
7. Implement document requirements logic
8. Add `GET /api/v1/exposure` endpoint
9. Create web UI HTML page
10. Update existing IntakeApi endpoints for new fields
11. Write tests

## 8. Out of Scope (Future)

- Email drafting based on exposure results
- Live connection to SSRS/SQL Server
- Document upload and tracking
- Workflow automation
- Webhook/notification on threshold breach
