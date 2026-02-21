# DealFlow Sandbox

A microservices deal-pipeline I built to demonstrate production-grade .NET architecture — async messaging, worker services, event-driven design, and a one-command Docker stack.

I'm Suleyman Kiani, currently a Sales Analyst at MHCC. I built this project from scratch to show how I think about software: decoupled services, clean separation of concerns, and systems that scale. It models a simplified version of the equipment-financing workflow I see every day — but engineered the way I believe it should be.

---

## What It Does

A broker submits a deal via API. The system **automatically scores it for risk**, **dispatches notifications**, and provides **instant party exposure lookups** — all without any service calling another directly. Everything communicates through RabbitMQ and PostgreSQL.

**The headline feature:** Party Exposure Lookup replaces a 15-20 minute manual process (SSRS reports + manual tally) with a sub-second API call that returns total exposure, NSF history, delinquency status, and document requirements in one response.

![DealFlow Demo](docs/dealflow-demo.webp)

---

## Quick Start

```bash
docker compose up --build
# All 4 services + PostgreSQL + RabbitMQ — ready in ~60s
```

| Service | URL |
|---|---|
| Intake API (submit deals) | http://localhost:5001/scalar/v1 |
| Reporting API + Exposure UI | http://localhost:5002/index.html |
| RabbitMQ Dashboard | http://localhost:15672 (guest/guest) |

25 realistic demo deals are seeded automatically — multiple customers, vendors, equipment types, NSF histories, and delinquency scenarios.

---

## Tech Stack

**.NET 10** · **ASP.NET Core Minimal APIs** · **RabbitMQ + MassTransit** · **PostgreSQL + EF Core** · **Docker Compose** · **Worker Services**

---

## Documentation

| Document | What's Inside |
|---|---|
| [Demo Walkthrough](docs/demo-walkthrough.md) | Recorded demo, scenario summaries, what to look for |
| [Demo Script](docs/demo.md) | Step-by-step curl commands to run the full demo yourself |
| [Architecture Deep-Dive](docs/architecture.md) | System diagrams, event flow, ERD, scoring logic, deployment topology |

---

## Project Structure

```
src/
  DealFlow.IntakeApi/        → Receives deals, publishes events (port 5001)
  DealFlow.ScoringWorker/    → Consumes events, scores risk (pure function)
  DealFlow.NotifyWorker/     → Consumes scored events, dispatches notifications
  DealFlow.ReportingApi/     → Query deals, timelines, exposure lookup (port 5002)
  DealFlow.Data/             → Shared EF Core DbContext, entities, migrations
  DealFlow.Contracts/        → MassTransit message contracts (shared DTOs)
```

No service calls another over HTTP. The only shared channels are PostgreSQL (state) and RabbitMQ (events).

---

## Author

**Suleyman Kiani**
[Resume (PDF)](docs/Suleyman_Resume_02_2026.pdf) · [GitHub](https://github.com/sulemaankiani) · [LinkedIn](https://linkedin.com/in/suleymankiani) · [suleymankiani.com](https://suleymankiani.com)
