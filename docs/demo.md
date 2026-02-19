# Demo Script — DealFlow Sandbox

End-to-end walkthrough using curl. Takes ~60 seconds.

## Setup

```bash
docker compose up --build
# Wait until all services are healthy (~60s first run, ~10s after)
```

## Step 1: Submit a Low-Risk Deal

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Forklift",
    "equipmentYear": 2022,
    "amount": 150000,
    "termMonths": 36,
    "industry": "Logistics",
    "province": "ON",
    "vendorTier": "A"
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
Deal <id> scored: 100 (LOW)
```

## Step 3: Check Deal Status

```bash
curl -s http://localhost:5001/api/v1/deals/<id> | python3 -m json.tool
```

**Expected:** status = "SCORED", score = 100, riskFlag = "LOW"

## Step 4: View Audit Timeline

```bash
curl -s http://localhost:5002/api/v1/deals/<id>/timeline | python3 -m json.tool
```

**Expected:** Array with DealSubmitted and DealScored events.

## Step 5: Submit a High-Risk Deal

```bash
curl -s -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Excavator",
    "equipmentYear": 2015,
    "amount": 1500000,
    "termMonths": 84,
    "industry": "Construction",
    "province": "AB",
    "vendorTier": "C"
  }' | python3 -m json.tool
```

**Expected:** score <= 20, riskFlag = "HIGH"

## Step 6: Filter Deals

```bash
# All scored deals
curl -s "http://localhost:5002/api/v1/deals?status=SCORED" | python3 -m json.tool

# High-value deals
curl -s "http://localhost:5002/api/v1/deals?minAmount=500000" | python3 -m json.tool

# Tier C deals
curl -s "http://localhost:5002/api/v1/deals?vendorTier=C" | python3 -m json.tool
```

## Step 7: Show RabbitMQ Dashboard

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
    "industry": "Logistics",
    "province": "ON",
    "vendorTier": "X"
  }' | python3 -m json.tool
```

**Expected:** 400 Bad Request with field-level validation errors.
