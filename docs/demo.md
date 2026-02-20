# Demo Script — DealFlow Sandbox

End-to-end walkthrough using curl. Takes ~60 seconds.

## Setup

```bash
docker compose up --build
# Wait until all services are healthy (~60s first run, ~10s after)
```

## Step 1: Submit a Low-Risk Deal

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
    "creditRating": "CR1"
  }' | python3 -m json.tool
```

**Expected:** 201 Created, status = "RECEIVED"

Save the `id` from the response.

## Step 2: Watch the Scoring Worker (in a second terminal)

```bash
docker compose logs deal-scoring-worker -f
```

Within 2 seconds you should see:
```
Scoring deal <id> [correlation: <uuid>]
Deal <id> scored: 90 (LOW)
```

## Step 3: Check Deal Status

```bash
curl -s http://localhost:5001/api/v1/deals/<id> | python3 -m json.tool
```

**Expected:** status = "SCORED", score ≥ 85, riskFlag = "LOW"

## Step 4: View Audit Timeline

```bash
curl -s http://localhost:5002/api/v1/deals/<id>/timeline | python3 -m json.tool
```

**Expected:** Array with DealSubmitted and DealScored events with full payloads.

## Step 5: Submit a High-Risk Deal

A 2014 Cat 336 excavator — poor credit rating, aged equipment, large ticket, long term.

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

**Expected:** score ≤ 20, riskFlag = "HIGH"

Score breakdown:
- Base: 100
- Amount > $1M: −35
- Term > 60 months: −10
- Equipment pre-2018: −15
- CR5 credit rating: −35
- **Total: 5 → HIGH risk**

## Step 6: Submit an Average Deal

A 2020 Volvo FH16 dump truck — decent credit, recent, mid-range.

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Dump Truck (Volvo FH16)",
    "equipmentYear": 2020,
    "amount": 320000,
    "termMonths": 48,
    "industry": "Construction",
    "province": "BC",
    "creditRating": "CR3"
  }' | python3 -m json.tool
```

**Expected:** score ~75, riskFlag = "MEDIUM"

## Step 7: Filter Deals by Reporting API

```bash
# All scored deals
curl -s "http://localhost:5002/api/v1/deals?status=SCORED" | python3 -m json.tool

# High-value deals (over $500k)
curl -s "http://localhost:5002/api/v1/deals?minAmount=500000" | python3 -m json.tool

# Filter by credit rating (CR5 = highest risk)
curl -s "http://localhost:5002/api/v1/deals?creditRating=CR5" | python3 -m json.tool
```

## Step 8: Show RabbitMQ Dashboard

Open http://localhost:15672 (guest/guest)
- Navigate to **Queues** — see `deal-submitted` and `deal-scored-notify`
- Each queue shows message rates and delivery stats

## Validation Error Demo

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

**Expected:** 400 Bad Request with field-level validation errors — `equipmentType` empty, `amount` negative, `creditRating` invalid.
