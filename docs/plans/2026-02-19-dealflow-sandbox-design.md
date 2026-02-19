# DealFlow Sandbox — Architecture Design
**Date:** 2026-02-19
**Status:** Approved

---

## Overview

A self-contained microservices system simulating a financing/ops deal pipeline:

> Deal submitted → validated → queued → scored → status updated → notification emitted → reporting available

---

## Tech Stack

| Layer | Technology |
|---|---|
| Services | .NET 10 LTS + C# 14 |
| Local orchestration | Docker Compose (ARM64 + AMD64) |
| Database | PostgreSQL 16 |
| Messaging (local) | RabbitMQ |
| Messaging (cloud) | Azure Service Bus |
| Observability | OpenTelemetry (traces, logs, metrics) |
| Cloud runtime | Azure Container Apps |
| Container registry | Azure Container Registry (ACR) |
| CI/CD | GitHub Actions |
| Notifications | Console log + optional SendGrid |

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    deal-intake-api                      │
│  POST /deals  →  Postgres  →  RabbitMQ: deal.submitted  │
│  GET  /deals/{id}                                       │
└─────────────────────────┬───────────────────────────────┘
                          │ async
┌─────────────────────────▼───────────────────────────────┐
│                 deal-scoring-worker                     │
│  Consumes deal.submitted                                │
│  Scores deal → updates Postgres                         │
│  Publishes RabbitMQ: deal.scored                        │
└─────────────────────────┬───────────────────────────────┘
                          │ async
┌─────────────────────────▼───────────────────────────────┐
│                    deal-notify                          │
│  Consumes deal.scored                                   │
│  Logs Teams-style payload + optional SendGrid email     │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                 deal-reporting-api                      │
│  GET /deals?status=&minAmount=&vendorTier=              │
│  GET /deals/{id}/timeline                               │
└─────────────────────────────────────────────────────────┘
```

---

## Data Model

### `deals` table
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| correlation_id | UUID | Traces request end-to-end |
| equipment_type | TEXT | |
| equipment_year | INT | |
| amount | DECIMAL(18,2) | |
| term_months | INT | |
| industry | TEXT | |
| province | TEXT | |
| vendor_tier | CHAR(1) | A / B / C |
| status | TEXT | RECEIVED → SCORING → SCORED → NOTIFIED |
| score | INT | null until scored |
| risk_flag | TEXT | LOW / MEDIUM / HIGH |
| created_at | TIMESTAMPTZ | |
| updated_at | TIMESTAMPTZ | |

### `deal_events` table (audit trail)
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| deal_id | UUID FK | → deals |
| event_type | TEXT | DealSubmitted / DealScored / NotificationSent |
| payload | JSONB | Full event snapshot |
| occurred_at | TIMESTAMPTZ | |

---

## Message Contracts (v1)

### `deal.submitted`
```json
{
  "correlationId": "uuid",
  "dealId": "uuid",
  "amount": 250000.00,
  "termMonths": 48,
  "equipmentYear": 2020,
  "vendorTier": "B",
  "industry": "Construction",
  "province": "ON"
}
```

### `deal.scored`
```json
{
  "correlationId": "uuid",
  "dealId": "uuid",
  "score": 75,
  "riskFlag": "LOW",
  "scoredAt": "2026-02-19T12:00:00Z"
}
```

---

## Scoring Rules

| Rule | Impact |
|---|---|
| Amount > $500k | −20 |
| Amount > $1M | −35 (replaces above) |
| Term > 60 months | −10 |
| Equipment year < 2018 | −15 |
| Vendor tier C | −20 |
| Vendor tier B | −10 |
| Vendor tier A | 0 |

**Risk classification:** Score < 50 = HIGH · 50–74 = MEDIUM · 75+ = LOW
**Base score:** 100

---

## Local Development

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| deal-intake-api | http://localhost:5001/swagger |
| deal-reporting-api | http://localhost:5002/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| PostgreSQL | localhost:5432 |

---

## Cloud Deployment (Azure Container Apps)

```
Azure Container Registry  ←  GitHub Actions (push on main)
Azure Container Apps Environment
  ├── deal-intake-api      (public HTTP ingress)
  ├── deal-scoring-worker  (internal, no ingress)
  ├── deal-notify          (internal, no ingress)
  └── deal-reporting-api   (public HTTP ingress)
Azure Database for PostgreSQL Flexible Server
Azure Service Bus (replaces RabbitMQ)
```

### Required Setup (one-time)
1. Free Azure account at portal.azure.com
2. `az login` + create resource group
3. Create ACR, Container Apps environment, Postgres, Service Bus
4. Add GitHub Actions secrets: `AZURE_CREDENTIALS`, `ACR_LOGIN_SERVER`, `ACR_USERNAME`, `ACR_PASSWORD`
5. Optional: `SENDGRID_API_KEY` for real email notifications

---

## CI/CD Pipeline

```
On push to main:
  1. dotnet test (all projects)
  2. docker buildx build --platform linux/amd64,linux/arm64
  3. docker push → ACR
  4. az containerapp update → deploy
```
