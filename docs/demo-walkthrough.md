# DealFlow Sandbox: Demo Walkthrough

The DealFlow sandbox demonstrates a resilient, event-driven architecture using **.NET 10 Minimal APIs, RabbitMQ, PostgreSQL, and Worker Services**.

## What This Demo Shows

1. **Deal Pipeline** — Submit a deal via API, watch it flow through async scoring and notification workers
2. **Party Exposure Lookup** — Instantly check customer/vendor exposure, NSFs, delinquency, and document requirements
3. **Reporting API** — Filter and query deals with full audit timelines

## Architecture Highlights

- **Decoupled Intake:** The Intake API publishes a `DealSubmitted` message and returns immediately. Scoring happens asynchronously via RabbitMQ.
- **Pure Function Scoring:** Risk scoring is an isolated pure function (`ScoringEngine.Score()`) with no database dependencies — trivially testable.
- **Audit Trails:** Every state change writes to `DealEvents` with the full JSON payload, providing a complete history without full event-sourcing complexity.
- **Party Exposure Lookup:** Aggregates deals by customer or vendor name, calculates total net exposure, and automatically determines document requirements based on configurable thresholds.

## Recorded Demo

![DealFlow Final Demo](dealflow-demo.webp)

## Demo Scenarios

### Deal Pipeline
- **Low-Risk (CR1):** $185K Semi-Truck, 60-month term → Score 90, LOW risk
- **High-Risk (CR5):** $1.25M Excavator, 84-month term, 2014 equipment → Score 5, HIGH risk
- **Validation:** Empty fields, negative amounts, invalid credit ratings → 400 with field-level errors

### Exposure Lookup
- **TransCanada Hauling (customer):** 4 active deals, $535K exposure, 3 NSFs, Enhanced tier
- **Strongco Corporation (vendor):** 3 deals across 2 customers, $714K exposure
- **Coastal Demolition (customer):** 10 NSFs, 2 delinquent — the problem customer
- **Prairie Grain (customer):** $116K, Standard tier — clean payment history
- **Maritime Medical (customer):** Paid-off only, $0 active exposure

### Document Requirements
| Tier | Threshold | What's Required |
|---|---|---|
| Standard (green) | < $250K | No additional documents |
| Enhanced (amber) | $250K – $1M | Bank statements |
| FullReview (red) | > $1M | 3-year financials + interims for spreads |

## Running the Demo

See **[demo.md](demo.md)** for the full step-by-step script with curl commands.

```bash
# Quick start
docker compose up --build

# Web UI
open http://localhost:5002/index.html

# API docs
open http://localhost:5001/scalar/v1
```

## Seed Data

28 realistic deals are seeded automatically on first start, covering:
- 8 customers across transportation, construction, agriculture, medical, and tech
- 5+ vendors including National Truck Centre, Strongco, Fleet Equipment
- Both lessors (MHCCL and MHCCA)
- Mix of funded, paid-off, and pipeline-stage deals
- NSF histories and delinquency scenarios for demo variety
