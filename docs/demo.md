# Demo Script — DealFlow Sandbox

End-to-end walkthrough covering the deal pipeline and party exposure lookup. Takes ~5 minutes.

## Setup

```bash
docker compose up --build
# Wait until all services are healthy (~60s first run, ~10s after)
```

28 realistic demo deals are seeded automatically on first start — covering multiple customers, vendors, equipment types, NSF histories, and delinquency scenarios.

---

## Part 1: Deal Pipeline (Submit → Score → Notify)

### Step 1: Submit a Low-Risk Deal

A 2023 Kenworth T680 semi-truck — strong credit, recent equipment, mid-range ticket.

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Semi-Truck (Kenworth T680)",
    "equipmentYear": 2023,
    "amount": 185000,
    "termMonths": 60,
    "industry": "Transportation",
    "province": "ON",
    "creditRating": "CR1",
    "customerLegalName": "Northern Logistics Inc.",
    "primaryVendor": "National Truck Centre Inc.",
    "dealFormat": "VENDOR",
    "lessor": "MHCCL",
    "accountManager": "Edwin Van Schepen",
    "primaryEquipmentCategory": "Transportation (TRAN)",
    "netInvest": 195000,
    "monthlyPayment": 4885.42
  }' | python3 -m json.tool
```

**Expected:** 201 Created, status = "RECEIVED". Save the `id` from the response.

### Step 2: Watch the Scoring Worker

```bash
docker compose logs deal-scoring-worker -f
```

Within 2 seconds:
```
Scoring deal <id> [correlation: <uuid>]
Deal <id> scored: 90 (LOW)
```

### Step 3: Check Deal Status

```bash
curl -s http://localhost:5001/api/v1/deals/<id> | python3 -m json.tool
```

**Expected:** status = "SCORED", score = 90, riskFlag = "LOW"

### Step 4: View Audit Timeline

```bash
curl -s http://localhost:5002/api/v1/deals/<id>/timeline | python3 -m json.tool
```

**Expected:** Array with DealSubmitted and DealScored events with full payloads.

### Step 5: Submit a High-Risk Deal

A 2014 Cat 336 excavator — worst credit rating, aged equipment, large ticket, long term.

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Excavator (Caterpillar 336)",
    "equipmentYear": 2014,
    "amount": 1250000,
    "termMonths": 84,
    "industry": "Construction",
    "province": "AB",
    "creditRating": "CR5"
  }' | python3 -m json.tool
```

**Expected:** score = 5, riskFlag = "HIGH"

Score breakdown:
- Base: 100
- Amount > $1M: −35
- Term > 60 months: −10
- Equipment pre-2018: −15
- CR5 credit rating: −35
- **Total: 5 → HIGH risk**

### Step 6: Validation Error Demo

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "",
    "equipmentYear": 2022,
    "amount": -100,
    "termMonths": 36,
    "industry": "Construction",
    "province": "ON",
    "creditRating": "CR9"
  }' | python3 -m json.tool
```

**Expected:** 400 Bad Request with field-level validation errors — `equipmentType` empty, `amount` negative, `creditRating` invalid (must be CR1–CR5).

---

## Part 2: Party Exposure Lookup

This is the feature that replaces a 2+ minute manual process on the SSRS reporting site. Open the web UI at **http://localhost:5002/index.html** or use the API directly.

### Step 7: Customer Lookup — TransCanada Hauling (Enhanced Tier)

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=TransCanada" \
  | python3 -m json.tool
```

**Expected:**
- 4 active deals, total net exposure ~$535K
- 3 NSFs across 2 deals, 1 deal delinquent (45 days past due)
- Document tier: **Enhanced** — bank statements required (exposure > $250K)

### Step 8: Vendor Lookup — Strongco Corporation

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=vendor&name=Strongco" \
  | python3 -m json.tool
```

**Expected:**
- 3 deals from 2 different customers (Excavation Pro Québec + Alberta Earthworks)
- $714K vendor exposure
- Shows how one vendor can be linked to multiple customers

### Step 9: Problem Customer — Coastal Demolition (High NSFs)

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=Coastal&includePastDeals=true" \
  | python3 -m json.tool
```

**Expected:**
- 3 deals (2 active, 1 paid-off — visible because `includePastDeals=true`)
- 10 total NSFs, 2 delinquent deals
- This is the customer you'd flag immediately in a credit review

### Step 10: Small Customer — Prairie Grain (Standard Tier)

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=Prairie" \
  | python3 -m json.tool
```

**Expected:**
- $116K exposure → **Standard** tier (no additional documents required)
- Clean payment history — 0 NSFs, no delinquency

### Step 11: Paid-Off Only — Maritime Medical

```bash
curl -s "http://localhost:5002/api/v1/exposure?searchType=customer&name=Maritime&includePastDeals=true" \
  | python3 -m json.tool
```

**Expected:**
- 0 active deals, 3 paid-off deals
- $0 active exposure (paid-off deals don't count toward exposure)
- Only visible with `includePastDeals=true`

### Step 12: Web UI Demo

Open **http://localhost:5002/index.html** in a browser.

1. Select **Customer**, type "TransCanada", click Search
   - See the amber **Enhanced** document requirements banner
   - Summary cards show exposure, NSFs, delinquency at a glance
   - Deal table shows all 4 deals with status badges and highlighted risk indicators

2. Switch to **Vendor**, type "Strongco", click Search
   - See deals from multiple customers aggregated under one vendor

3. Type "Coastal", check **Include past deals**, click Search
   - Red flags everywhere — NSF counts, delinquent deals highlighted

---

## Part 3: Reporting API

### Step 13: Filter Deals

```bash
# All scored deals
curl -s "http://localhost:5002/api/v1/deals?status=SCORED" | python3 -m json.tool

# High-value deals
curl -s "http://localhost:5002/api/v1/deals?minAmount=500000" | python3 -m json.tool

# Filter by credit rating
curl -s "http://localhost:5002/api/v1/deals?creditRating=CR5" | python3 -m json.tool
```

### Step 14: RabbitMQ Dashboard

Open **http://localhost:15672** (guest/guest)
- Navigate to **Queues** — see `deal-submitted` and `deal-scored-notify`
- Each queue shows message rates and delivery stats

---

## Document Requirement Thresholds

These are configurable in `src/DealFlow.ReportingApi/appsettings.json`:

```json
{
  "ExposureThresholds": {
    "EnhancedThreshold": 250000,
    "FullReviewThreshold": 1000000
  }
}
```

| Tier | Net Exposure | What's Required |
|---|---|---|
| Standard | < $250K | No additional documents |
| Enhanced | $250K – $1M | Bank statements or financial statements |
| FullReview | > $1M | 3-year financial statements + interims for spreads |

## Scoring Logic Reference

All deals start at **100 points**. Deductions:

| Factor | Condition | Deduction |
|---|---|---|
| Deal size | Amount > $1M | −35 |
| Deal size | Amount > $500K | −20 |
| Term length | > 60 months | −10 |
| Equipment age | Year < 2018 | −15 |
| Credit rating | CR2 | −5 |
| Credit rating | CR3 | −15 |
| Credit rating | CR4 | −25 |
| Credit rating | CR5 | −35 |

Risk classification: 75–100 = LOW, 50–74 = MEDIUM, 0–49 = HIGH
