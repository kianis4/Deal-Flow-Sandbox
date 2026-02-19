# DealFlow Sandbox

A microservices deal pipeline demonstrating async event-driven architecture with .NET 10.

**Deal flow:** Submit → Queue → Score → Notify → Report

## Architecture

```
deal-intake-api ──[DealSubmitted]──► deal-scoring-worker ──[DealScored]──► deal-notify

deal-reporting-api (read-only, queries Postgres directly)
```

**Stack:** .NET 10 · C# 14 · MassTransit · RabbitMQ · PostgreSQL · Docker · Azure Container Apps

## Quick Start (Local)

**Prerequisites:** Docker Desktop

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Intake API (Swagger) | http://localhost:5001/swagger |
| Reporting API (Swagger) | http://localhost:5002/swagger |
| RabbitMQ Dashboard | http://localhost:15672 (guest / guest) |

## Services

| Service | Role | Type |
|---|---|---|
| `deal-intake-api` | Accept deal submissions, publish `DealSubmitted` event | ASP.NET Core Minimal API |
| `deal-scoring-worker` | Consume event, score deal, publish `DealScored` | .NET Worker Service |
| `deal-notify` | Consume `DealScored`, log Teams-style notification | .NET Worker Service |
| `deal-reporting-api` | Query deals + audit timeline | ASP.NET Core Minimal API |

## Scoring Rules

| Factor | Impact |
|---|---|
| Amount > $500k | −20 pts |
| Amount > $1M | −35 pts |
| Term > 60 months | −10 pts |
| Equipment year < 2018 | −15 pts |
| Vendor Tier B | −10 pts |
| Vendor Tier C | −20 pts |

**Risk classification:** 75–100 = LOW · 50–74 = MEDIUM · 0–49 = HIGH

## Testing

```bash
export DOTNET_ROOT=$HOME/.dotnet && export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
dotnet test DealFlow.slnx
```

## Cloud Deployment

See [docs/azure-setup.md](docs/azure-setup.md) for Azure Container Apps deployment instructions.
