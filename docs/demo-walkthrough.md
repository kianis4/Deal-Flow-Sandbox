# DealFlow Sandbox: End-to-End Walkthrough

The DealFlow sandbox demonstrates a resilient, event-driven architecture using **.NET 10 Minimal APIs, RabbitMQ, PostgreSQL, and Worker Services**. 

This demo highlights the recent **CreditRating refactor**, validating that the system correctly parses `CR1`-`CR5` credit ratings, and factors them properly into the risk-flagging worker.

## Architecture Highlights
- **Decoupled Intake:** The Intake API does not wait maliciously for HTTP requests. It acts idempotently and offloads the scoring heavy-lifting via a `DealSubmitted` message.
- **Pure Function Logic:** Complex rules (e.g. `Amount > 1_000_000 => -35 score`, and `CR5 => -35 score`) are isolated in the pure `ScoringEngine.Score()` function without DB side-effects.
- **Audit Trails:** Event payloads are captured historically in `DealEvents`, serving as a source of truth for deal progression without tight event-sourcing complexity.

## Recorded Demo Walkthrough

Below is a recording showing the full lifecycle of multiple equipment financing deals.

![DealFlow Final Demo](dealflow-demo.webp)

### Demonstrated Scenarios:

**1. Low-Risk Deal Execution (CR1)**
- **Inputs:** Semi-Truck (Kenworth T680), $185,000, 60-month term, CR1 credit rating.
- **Workflow:** Submitted via `POST /api/v1/deals` in the Intake API. 
- **Result:** Deal proceeds through async messaging seamlessly, yielding a highly favorable score (~90) classifying it as **LOW** risk.

**2. High-Risk Deal Execution (CR5)**
- **Inputs:** Excavator (Caterpillar 336), 2014, $1,250,000, 84-month term, CR5 credit rating.
- **Workflow:** Submitted via `POST /api/v1/deals` in the Intake API. 
- **Result:** The system accurately applied all expected deductions (amount penalty, term penalty, age penalty, and maximum credit penalty) resolving as **HIGH** risk (Score ~5).

**3. Read-Side Reporting API Verification**
- **Filtering:** Tested the `GET /api/v1/deals?creditRating=CR5` boundary, proving the query dynamically tracks the updated schema changes.
- **Audit Timeline:** Confirmed that both `DealSubmitted` and `DealScored` asynchronous phases accurately logged payloads across boundaries via the Timeline API.

> [!NOTE] 
> The earlier 3 test failures on your local machine correctly reported a `DockerUnavailableException`, signifying a mismatch between the Testcontainers package (expecting max `1.41`) and the active Docker engine API version (`1.44`) locally—it was **not** a flaw in the `CreditRating` C# refactoring or PostgreSQL migrations. The underlying application code translates CR1-CR5 exactly as intended.
