# DealFlow Sandbox — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a complete 4-service microservices deal pipeline with local Docker Compose demo and Azure Container Apps cloud deployment.

**Architecture:** Four .NET 10 services communicate via RabbitMQ (local) / Azure Service Bus (cloud) using MassTransit as the messaging abstraction. PostgreSQL stores deals and an audit event log. All services are containerized and orchestrated via Docker Compose locally and Azure Container Apps in the cloud.

**Tech Stack:** .NET 10 · C# 14 · MassTransit 8 · PostgreSQL 16 · RabbitMQ 3 · Entity Framework Core 10 · xUnit · FluentAssertions · Testcontainers · GitHub Actions · Azure Container Apps · Azure Container Registry

---

## Prerequisites (User must do this — Claude cannot)

### Step P1: Install .NET 10 SDK (Mac M1)
```bash
# Download from https://dotnet.microsoft.com/download/dotnet/10.0
# Choose: macOS Arm64 .pkg installer
# After install, verify:
dotnet --version   # should print 10.x.x
```

### Step P2: Install Azure CLI
```bash
brew install azure-cli
az --version   # should print 2.x.x
```

### Step P3: Install GitHub CLI
```bash
brew install gh
gh --version
gh auth login   # authenticate with GitHub
```

### Step P4: Create Free Azure Account
1. Go to https://azure.microsoft.com/free
2. Sign up (requires credit card for verification, not charged)
3. After account is created, run: `az login`

### Step P5: Update Docker Desktop
Docker 20.10.14 is from 2022. Update to latest via Docker Desktop → Check for Updates.
This ensures multi-platform build support works correctly.

---

## Phase 1: Repository Scaffold

### Task 1: Solution + Project Structure

**Files to create:**
```
DealFlow.sln
src/
  DealFlow.Contracts/          ← shared message types
  DealFlow.IntakeApi/          ← ASP.NET Core Minimal API
  DealFlow.ScoringWorker/      ← .NET Worker Service
  DealFlow.NotifyWorker/       ← .NET Worker Service
  DealFlow.ReportingApi/       ← ASP.NET Core Minimal API
tests/
  DealFlow.IntakeApi.Tests/
  DealFlow.ScoringWorker.Tests/
  DealFlow.ReportingApi.Tests/
  DealFlow.Integration.Tests/
```

**Step 1: Create solution and projects**
```bash
cd /path/to/Deal-Flow-Sandbox

# Solution file
dotnet new sln -n DealFlow

# Shared contracts (class library)
dotnet new classlib -n DealFlow.Contracts -o src/DealFlow.Contracts --framework net10.0

# Services
dotnet new webapi -n DealFlow.IntakeApi -o src/DealFlow.IntakeApi --framework net10.0 --use-minimal-apis
dotnet new worker -n DealFlow.ScoringWorker -o src/DealFlow.ScoringWorker --framework net10.0
dotnet new worker -n DealFlow.NotifyWorker -o src/DealFlow.NotifyWorker --framework net10.0
dotnet new webapi -n DealFlow.ReportingApi -o src/DealFlow.ReportingApi --framework net10.0 --use-minimal-apis

# Test projects
dotnet new xunit -n DealFlow.IntakeApi.Tests -o tests/DealFlow.IntakeApi.Tests --framework net10.0
dotnet new xunit -n DealFlow.ScoringWorker.Tests -o tests/DealFlow.ScoringWorker.Tests --framework net10.0
dotnet new xunit -n DealFlow.ReportingApi.Tests -o tests/DealFlow.ReportingApi.Tests --framework net10.0
dotnet new xunit -n DealFlow.Integration.Tests -o tests/DealFlow.Integration.Tests --framework net10.0

# Add all to solution
dotnet sln DealFlow.sln add src/DealFlow.Contracts/DealFlow.Contracts.csproj
dotnet sln DealFlow.sln add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj
dotnet sln DealFlow.sln add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj
dotnet sln DealFlow.sln add src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj
dotnet sln DealFlow.sln add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj
dotnet sln DealFlow.sln add tests/DealFlow.IntakeApi.Tests/DealFlow.IntakeApi.Tests.csproj
dotnet sln DealFlow.sln add tests/DealFlow.ScoringWorker.Tests/DealFlow.ScoringWorker.Tests.csproj
dotnet sln DealFlow.sln add tests/DealFlow.ReportingApi.Tests/DealFlow.ReportingApi.Tests.csproj
dotnet sln DealFlow.sln add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj
```

**Step 2: Add project references**
```bash
# All services reference Contracts
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj
dotnet add src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj

# Test projects reference their service + Contracts
dotnet add tests/DealFlow.IntakeApi.Tests/DealFlow.IntakeApi.Tests.csproj reference src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj
dotnet add tests/DealFlow.ScoringWorker.Tests/DealFlow.ScoringWorker.Tests.csproj reference src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj
dotnet add tests/DealFlow.ReportingApi.Tests/DealFlow.ReportingApi.Tests.csproj reference src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj reference src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj
```

**Step 3: Add NuGet packages**
```bash
# --- Contracts (no dependencies needed) ---

# --- IntakeApi ---
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package MassTransit.RabbitMQ
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package FluentValidation.AspNetCore
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package Serilog.AspNetCore
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package OpenTelemetry.Extensions.Hosting
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package OpenTelemetry.Exporter.Console
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package Swashbuckle.AspNetCore

# --- ScoringWorker ---
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj package MassTransit.RabbitMQ
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj package Serilog.Extensions.Hosting
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj package OpenTelemetry.Extensions.Hosting

# --- NotifyWorker ---
dotnet add src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj package MassTransit.RabbitMQ
dotnet add src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj package Serilog.Extensions.Hosting
dotnet add src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj package SendGrid

# --- ReportingApi ---
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj package Serilog.AspNetCore
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj package OpenTelemetry.Extensions.Hosting
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj package Swashbuckle.AspNetCore

# --- Test projects ---
dotnet add tests/DealFlow.IntakeApi.Tests/DealFlow.IntakeApi.Tests.csproj package FluentAssertions
dotnet add tests/DealFlow.IntakeApi.Tests/DealFlow.IntakeApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/DealFlow.ScoringWorker.Tests/DealFlow.ScoringWorker.Tests.csproj package FluentAssertions
dotnet add tests/DealFlow.ReportingApi.Tests/DealFlow.ReportingApi.Tests.csproj package FluentAssertions
dotnet add tests/DealFlow.ReportingApi.Tests/DealFlow.ReportingApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj package FluentAssertions
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj package Testcontainers.PostgreSql
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj package Testcontainers.RabbitMq
dotnet add tests/DealFlow.Integration.Tests/DealFlow.Integration.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

**Step 4: Create .gitignore**
```bash
dotnet new gitignore
```

**Step 5: Verify build**
```bash
dotnet build DealFlow.sln
# Expected: Build succeeded with 0 errors
```

**Step 6: Commit**
```bash
git add -A
git commit -m "chore: scaffold solution with 5 projects and 4 test projects"
```

---

## Phase 2: Shared Contracts

### Task 2: Message Contracts + Domain Enums

**File:** `src/DealFlow.Contracts/Messages/DealSubmitted.cs`
```csharp
namespace DealFlow.Contracts.Messages;

public record DealSubmitted
{
    public Guid CorrelationId { get; init; }
    public Guid DealId { get; init; }
    public decimal Amount { get; init; }
    public int TermMonths { get; init; }
    public int EquipmentYear { get; init; }
    public string VendorTier { get; init; } = default!;
    public string Industry { get; init; } = default!;
    public string Province { get; init; } = default!;
}
```

**File:** `src/DealFlow.Contracts/Messages/DealScored.cs`
```csharp
namespace DealFlow.Contracts.Messages;

public record DealScored
{
    public Guid CorrelationId { get; init; }
    public Guid DealId { get; init; }
    public int Score { get; init; }
    public string RiskFlag { get; init; } = default!;
    public DateTimeOffset ScoredAt { get; init; }
}
```

**File:** `src/DealFlow.Contracts/Domain/DealStatus.cs`
```csharp
namespace DealFlow.Contracts.Domain;

public static class DealStatus
{
    public const string Received = "RECEIVED";
    public const string Scoring = "SCORING";
    public const string Scored = "SCORED";
    public const string Notified = "NOTIFIED";
}
```

**File:** `src/DealFlow.Contracts/Domain/RiskFlag.cs`
```csharp
namespace DealFlow.Contracts.Domain;

public static class RiskFlag
{
    public const string Low = "LOW";
    public const string Medium = "MEDIUM";
    public const string High = "HIGH";
}
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: add shared message contracts and domain constants"
```

---

## Phase 3: Database + EF Core

### Task 3: Shared Database Context

> Both IntakeApi and ReportingApi use this. ScoringWorker uses it too. Create it in Contracts or a new `DealFlow.Data` project.
> Decision: Add to `src/DealFlow.Data/` (new class library) to avoid coupling.

```bash
dotnet new classlib -n DealFlow.Data -o src/DealFlow.Data --framework net10.0
dotnet sln DealFlow.sln add src/DealFlow.Data/DealFlow.Data.csproj
dotnet add src/DealFlow.Data/DealFlow.Data.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/DealFlow.Data/DealFlow.Data.csproj reference src/DealFlow.Contracts/DealFlow.Contracts.csproj

# Add reference to services that need DB
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj reference src/DealFlow.Data/DealFlow.Data.csproj
dotnet add src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj reference src/DealFlow.Data/DealFlow.Data.csproj
dotnet add src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj reference src/DealFlow.Data/DealFlow.Data.csproj
```

**File:** `src/DealFlow.Data/Entities/Deal.cs`
```csharp
namespace DealFlow.Data.Entities;

public class Deal
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public string EquipmentType { get; set; } = default!;
    public int EquipmentYear { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Industry { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string VendorTier { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int? Score { get; set; }
    public string? RiskFlag { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<DealEvent> Events { get; set; } = new List<DealEvent>();
}
```

**File:** `src/DealFlow.Data/Entities/DealEvent.cs`
```csharp
namespace DealFlow.Data.Entities;

public class DealEvent
{
    public Guid Id { get; set; }
    public Guid DealId { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;   // JSON
    public DateTimeOffset OccurredAt { get; set; }
    public Deal Deal { get; set; } = default!;
}
```

**File:** `src/DealFlow.Data/DealFlowDbContext.cs`
```csharp
using DealFlow.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealFlow.Data;

public class DealFlowDbContext(DbContextOptions<DealFlowDbContext> options) : DbContext(options)
{
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealEvent> DealEvents => Set<DealEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Deal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.VendorTier).HasMaxLength(1);
            e.HasMany(x => x.Events).WithOne(x => x.Deal).HasForeignKey(x => x.DealId);
        });

        modelBuilder.Entity<DealEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Payload).HasColumnType("jsonb");
        });
    }
}
```

**File:** `src/DealFlow.Data/Migrations/` — generate via EF tools
```bash
dotnet add src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj package Microsoft.EntityFrameworkCore.Design

# Generate initial migration (run from repo root)
dotnet ef migrations add InitialCreate \
  --project src/DealFlow.Data \
  --startup-project src/DealFlow.IntakeApi \
  --output-dir Migrations
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: add EF Core data layer with Deal and DealEvent entities"
```

---

## Phase 4: deal-intake-api

### Task 4: IntakeApi — Models, Validation, Endpoints

**File:** `src/DealFlow.IntakeApi/Models/SubmitDealRequest.cs`
```csharp
namespace DealFlow.IntakeApi.Models;

public record SubmitDealRequest(
    string EquipmentType,
    int EquipmentYear,
    decimal Amount,
    int TermMonths,
    string Industry,
    string Province,
    string VendorTier   // "A", "B", or "C"
);
```

**File:** `src/DealFlow.IntakeApi/Models/DealResponse.cs`
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
    string VendorTier,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
```

**File:** `src/DealFlow.IntakeApi/Validators/SubmitDealValidator.cs`
```csharp
using DealFlow.IntakeApi.Models;
using FluentValidation;

namespace DealFlow.IntakeApi.Validators;

public class SubmitDealValidator : AbstractValidator<SubmitDealRequest>
{
    private static readonly string[] ValidTiers = ["A", "B", "C"];

    public SubmitDealValidator()
    {
        RuleFor(x => x.EquipmentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EquipmentYear).InclusiveBetween(1990, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.TermMonths).InclusiveBetween(6, 120);
        RuleFor(x => x.Industry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VendorTier).Must(t => ValidTiers.Contains(t))
            .WithMessage("VendorTier must be A, B, or C");
    }
}
```

**File:** `src/DealFlow.IntakeApi/Program.cs`
```csharp
using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.Data;
using DealFlow.Data.Entities;
using DealFlow.IntakeApi.Models;
using DealFlow.IntakeApi.Validators;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

// Database
builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<SubmitDealValidator>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DealFlowDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "deal-intake-api" }));

// POST /deals
app.MapPost("/api/v1/deals", async (
    SubmitDealRequest request,
    IValidator<SubmitDealRequest> validator,
    DealFlowDbContext db,
    IPublishEndpoint publisher,
    ILogger<Program> logger) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var correlationId = Guid.NewGuid();
    var deal = new Deal
    {
        Id = Guid.NewGuid(),
        CorrelationId = correlationId,
        EquipmentType = request.EquipmentType,
        EquipmentYear = request.EquipmentYear,
        Amount = request.Amount,
        TermMonths = request.TermMonths,
        Industry = request.Industry,
        Province = request.Province,
        VendorTier = request.VendorTier,
        Status = DealStatus.Received,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    deal.Events.Add(new DealEvent
    {
        Id = Guid.NewGuid(),
        DealId = deal.Id,
        EventType = "DealSubmitted",
        Payload = JsonSerializer.Serialize(request),
        OccurredAt = DateTimeOffset.UtcNow
    });

    db.Deals.Add(deal);
    await db.SaveChangesAsync();

    await publisher.Publish(new DealSubmitted
    {
        CorrelationId = correlationId,
        DealId = deal.Id,
        Amount = deal.Amount,
        TermMonths = deal.TermMonths,
        EquipmentYear = deal.EquipmentYear,
        VendorTier = deal.VendorTier,
        Industry = deal.Industry,
        Province = deal.Province
    });

    logger.LogInformation("Deal {DealId} submitted with correlation {CorrelationId}", deal.Id, correlationId);

    return Results.Created($"/api/v1/deals/{deal.Id}", ToResponse(deal));
})
.WithName("SubmitDeal")
.WithOpenApi();

// GET /deals/{id}
app.MapGet("/api/v1/deals/{id:guid}", async (Guid id, DealFlowDbContext db) =>
{
    var deal = await db.Deals.FindAsync(id);
    return deal is null ? Results.NotFound() : Results.Ok(ToResponse(deal));
})
.WithName("GetDeal")
.WithOpenApi();

app.Run();

static DealResponse ToResponse(Deal d) => new(
    d.Id, d.CorrelationId, d.EquipmentType, d.EquipmentYear,
    d.Amount, d.TermMonths, d.Industry, d.Province,
    d.VendorTier, d.Status, d.Score, d.RiskFlag,
    d.CreatedAt, d.UpdatedAt);

// Required for WebApplicationFactory in tests
public partial class Program { }
```

**File:** `src/DealFlow.IntakeApi/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dealflow;Username=dealflow;Password=dealflow"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" }
  }
}
```

### Task 5: IntakeApi Tests

**File:** `tests/DealFlow.IntakeApi.Tests/SubmitDealValidatorTests.cs`
```csharp
using DealFlow.IntakeApi.Models;
using DealFlow.IntakeApi.Validators;
using FluentAssertions;

namespace DealFlow.IntakeApi.Tests;

public class SubmitDealValidatorTests
{
    private readonly SubmitDealValidator _validator = new();

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var request = new SubmitDealRequest(
            "Excavator", 2021, 250_000, 48, "Construction", "ON", "A");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", 2021, 250_000, 48, "Construction", "ON", "A")]   // empty equipment type
    [InlineData("Excavator", 2021, -1, 48, "Construction", "ON", "A")] // negative amount
    [InlineData("Excavator", 2021, 250_000, 48, "Construction", "ON", "X")] // invalid tier
    public async Task Invalid_request_fails_validation(
        string type, int year, decimal amount, int term,
        string industry, string province, string tier)
    {
        var request = new SubmitDealRequest(type, year, amount, term, industry, province, tier);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}
```

**Step: Run unit tests**
```bash
dotnet test tests/DealFlow.IntakeApi.Tests/ -v
# Expected: 4 tests pass
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: implement deal-intake-api with validation and event publishing"
```

---

## Phase 5: deal-scoring-worker

### Task 6: Scoring Engine (pure logic, fully testable)

**File:** `src/DealFlow.ScoringWorker/Scoring/ScoringEngine.cs`
```csharp
using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;

namespace DealFlow.ScoringWorker.Scoring;

public static class ScoringEngine
{
    public static (int Score, string RiskFlag) Score(DealSubmitted deal)
    {
        var score = 100;

        // Amount risk
        score += deal.Amount switch
        {
            > 1_000_000 => -35,
            > 500_000   => -20,
            _           => 0
        };

        // Term risk
        if (deal.TermMonths > 60) score -= 10;

        // Equipment age risk
        if (deal.EquipmentYear < 2018) score -= 15;

        // Vendor tier risk
        score += deal.VendorTier switch
        {
            "C" => -20,
            "B" => -10,
            _   => 0
        };

        score = Math.Clamp(score, 0, 100);

        var flag = score switch
        {
            < 50 => RiskFlag.High,
            < 75 => RiskFlag.Medium,
            _    => RiskFlag.Low
        };

        return (score, flag);
    }
}
```

**File:** `tests/DealFlow.ScoringWorker.Tests/ScoringEngineTests.cs`
```csharp
using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.ScoringWorker.Scoring;
using FluentAssertions;

namespace DealFlow.ScoringWorker.Tests;

public class ScoringEngineTests
{
    private static DealSubmitted MakeDeal(
        decimal amount = 200_000,
        int termMonths = 36,
        int equipYear = 2022,
        string vendorTier = "A") => new()
    {
        CorrelationId = Guid.NewGuid(),
        DealId = Guid.NewGuid(),
        Amount = amount,
        TermMonths = termMonths,
        EquipmentYear = equipYear,
        VendorTier = vendorTier,
        Industry = "Construction",
        Province = "ON"
    };

    [Fact]
    public void Perfect_deal_scores_100_LOW()
    {
        var (score, flag) = ScoringEngine.Score(MakeDeal());
        score.Should().Be(100);
        flag.Should().Be(RiskFlag.Low);
    }

    [Fact]
    public void High_amount_reduces_score()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(amount: 750_000));
        score.Should().Be(80); // 100 - 20
    }

    [Fact]
    public void Million_plus_reduces_score_by_35()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(amount: 1_500_000));
        score.Should().Be(65); // 100 - 35
    }

    [Fact]
    public void Long_term_reduces_score()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(termMonths: 72));
        score.Should().Be(90); // 100 - 10
    }

    [Fact]
    public void Old_equipment_reduces_score()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(equipYear: 2015));
        score.Should().Be(85); // 100 - 15
    }

    [Fact]
    public void Tier_C_vendor_reduces_score_20()
    {
        var (score, _) = ScoringEngine.Score(MakeDeal(vendorTier: "C"));
        score.Should().Be(80);
    }

    [Fact]
    public void Worst_case_deal_is_HIGH_risk()
    {
        var worst = MakeDeal(amount: 2_000_000, termMonths: 84, equipYear: 2010, vendorTier: "C");
        var (score, flag) = ScoringEngine.Score(worst);
        score.Should().BeLessThan(50);
        flag.Should().Be(RiskFlag.High);
    }

    [Fact]
    public void Score_is_clamped_to_0_minimum()
    {
        var worst = MakeDeal(amount: 10_000_000, termMonths: 120, equipYear: 1990, vendorTier: "C");
        var (score, _) = ScoringEngine.Score(worst);
        score.Should().BeGreaterThanOrEqualTo(0);
    }
}
```

**Step: Run scoring tests**
```bash
dotnet test tests/DealFlow.ScoringWorker.Tests/ -v
# Expected: 8 tests pass
```

### Task 7: ScoringWorker Consumer

**File:** `src/DealFlow.ScoringWorker/Consumers/DealSubmittedConsumer.cs`
```csharp
using DealFlow.Contracts.Domain;
using DealFlow.Contracts.Messages;
using DealFlow.Data;
using DealFlow.Data.Entities;
using DealFlow.ScoringWorker.Scoring;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DealFlow.ScoringWorker.Consumers;

public class DealSubmittedConsumer(
    DealFlowDbContext db,
    IPublishEndpoint publisher,
    ILogger<DealSubmittedConsumer> logger) : IConsumer<DealSubmitted>
{
    public async Task Consume(ConsumeContext<DealSubmitted> context)
    {
        var msg = context.Message;
        logger.LogInformation("Scoring deal {DealId} [correlation: {CorrelationId}]",
            msg.DealId, msg.CorrelationId);

        // Idempotency: skip if already scored
        var deal = await db.Deals
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.Id == msg.DealId);

        if (deal is null)
        {
            logger.LogWarning("Deal {DealId} not found — skipping", msg.DealId);
            return;
        }

        if (deal.Status != DealStatus.Received)
        {
            logger.LogWarning("Deal {DealId} already in status {Status} — skipping (idempotency)",
                msg.DealId, deal.Status);
            return;
        }

        deal.Status = DealStatus.Scoring;
        deal.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var (score, riskFlag) = ScoringEngine.Score(msg);

        deal.Score = score;
        deal.RiskFlag = riskFlag;
        deal.Status = DealStatus.Scored;
        deal.UpdatedAt = DateTimeOffset.UtcNow;

        deal.Events.Add(new DealEvent
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            EventType = "DealScored",
            Payload = JsonSerializer.Serialize(new { score, riskFlag }),
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        await publisher.Publish(new DealScored
        {
            CorrelationId = msg.CorrelationId,
            DealId = msg.DealId,
            Score = score,
            RiskFlag = riskFlag,
            ScoredAt = DateTimeOffset.UtcNow
        });

        logger.LogInformation("Deal {DealId} scored: {Score} ({RiskFlag})", msg.DealId, score, riskFlag);
    }
}
```

**File:** `src/DealFlow.ScoringWorker/Program.cs`
```csharp
using DealFlow.Data;
using DealFlow.ScoringWorker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(cfg =>
    cfg.ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console());

builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DealSubmittedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("deal.submitted", e =>
        {
            e.ConfigureDeadLetterQueueDeadLetterTransport();
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            e.ConfigureConsumer<DealSubmittedConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: implement scoring worker with idempotency and dead-letter support"
```

---

## Phase 6: deal-notify

### Task 8: NotifyWorker Consumer

**File:** `src/DealFlow.NotifyWorker/Consumers/DealScoredConsumer.cs`
```csharp
using DealFlow.Contracts.Messages;
using MassTransit;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DealFlow.NotifyWorker.Consumers;

public class DealScoredConsumer(
    IConfiguration config,
    ILogger<DealScoredConsumer> logger) : IConsumer<DealScored>
{
    public async Task Consume(ConsumeContext<DealScored> context)
    {
        var msg = context.Message;

        var payload = new
        {
            type = "AdaptiveCard",
            title = $"Deal Scored — {msg.RiskFlag} Risk",
            body = new[]
            {
                $"Deal ID: {msg.DealId}",
                $"Score:   {msg.Score}/100",
                $"Risk:    {msg.RiskFlag}",
                $"Scored:  {msg.ScoredAt:u}"
            }
        };

        logger.LogInformation("[NOTIFY] Teams payload: {Payload}",
            System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var sendgridKey = config["SendGrid:ApiKey"];
        if (!string.IsNullOrWhiteSpace(sendgridKey))
        {
            await SendEmailAsync(sendgridKey, msg, logger);
        }
    }

    private static async Task SendEmailAsync(string apiKey, DealScored msg, ILogger logger)
    {
        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@dealflow.demo", "DealFlow");
            var to = new EmailAddress("demo@example.com"); // replace with your email
            var subject = $"[DealFlow] Deal {msg.DealId} scored — {msg.RiskFlag}";
            var body = $"Score: {msg.Score}/100 | Risk: {msg.RiskFlag} | Scored at: {msg.ScoredAt:u}";
            var mail = MailHelper.CreateSingleEmail(from, to, subject, body, body);
            var response = await client.SendEmailAsync(mail);
            logger.LogInformation("SendGrid response: {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SendGrid send failed — notification still logged");
        }
    }
}
```

**File:** `src/DealFlow.NotifyWorker/Program.cs`
```csharp
using DealFlow.Data;
using DealFlow.NotifyWorker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(cfg =>
    cfg.ReadFrom.Configuration(builder.Configuration).WriteTo.Console());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DealScoredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("deal.scored.notify", e =>
        {
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            e.ConfigureConsumer<DealScoredConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: implement notify worker with Teams-style logging and optional SendGrid"
```

---

## Phase 7: deal-reporting-api

### Task 9: ReportingApi Endpoints

**File:** `src/DealFlow.ReportingApi/Models/DealSummary.cs`
```csharp
namespace DealFlow.ReportingApi.Models;

public record DealSummary(
    Guid Id,
    string EquipmentType,
    decimal Amount,
    string VendorTier,
    string Status,
    int? Score,
    string? RiskFlag,
    DateTimeOffset CreatedAt
);

public record TimelineEvent(
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt
);
```

**File:** `src/DealFlow.ReportingApi/Program.cs`
```csharp
using DealFlow.Data;
using Microsoft.EntityFrameworkCore;
using DealFlow.ReportingApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

builder.Services.AddDbContext<DealFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "deal-reporting-api" }));

// GET /api/v1/deals?status=&minAmount=&vendorTier=
app.MapGet("/api/v1/deals", async (
    DealFlowDbContext db,
    string? status,
    decimal? minAmount,
    string? vendorTier) =>
{
    var query = db.Deals.AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(d => d.Status == status.ToUpper());

    if (minAmount.HasValue)
        query = query.Where(d => d.Amount >= minAmount.Value);

    if (!string.IsNullOrWhiteSpace(vendorTier))
        query = query.Where(d => d.VendorTier == vendorTier.ToUpper());

    var deals = await query
        .OrderByDescending(d => d.CreatedAt)
        .Select(d => new DealSummary(
            d.Id, d.EquipmentType, d.Amount, d.VendorTier,
            d.Status, d.Score, d.RiskFlag, d.CreatedAt))
        .ToListAsync();

    return Results.Ok(deals);
})
.WithName("ListDeals")
.WithOpenApi();

// GET /api/v1/deals/{id}/timeline
app.MapGet("/api/v1/deals/{id:guid}/timeline", async (Guid id, DealFlowDbContext db) =>
{
    var exists = await db.Deals.AnyAsync(d => d.Id == id);
    if (!exists) return Results.NotFound();

    var events = await db.DealEvents
        .Where(e => e.DealId == id)
        .OrderBy(e => e.OccurredAt)
        .Select(e => new TimelineEvent(e.EventType, e.Payload, e.OccurredAt))
        .ToListAsync();

    return Results.Ok(events);
})
.WithName("GetTimeline")
.WithOpenApi();

app.Run();

public partial class Program { }
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: implement reporting api with filtering and audit timeline"
```

---

## Phase 8: Docker Compose

### Task 10: Dockerfiles

**File:** `src/DealFlow.IntakeApi/Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY DealFlow.sln ./
COPY src/DealFlow.Contracts/DealFlow.Contracts.csproj src/DealFlow.Contracts/
COPY src/DealFlow.Data/DealFlow.Data.csproj src/DealFlow.Data/
COPY src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj src/DealFlow.IntakeApi/
RUN dotnet restore src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj
COPY src/DealFlow.Contracts/ src/DealFlow.Contracts/
COPY src/DealFlow.Data/ src/DealFlow.Data/
COPY src/DealFlow.IntakeApi/ src/DealFlow.IntakeApi/
RUN dotnet publish src/DealFlow.IntakeApi -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DealFlow.IntakeApi.dll"]
```

Repeat pattern for `ScoringWorker`, `NotifyWorker`, `ReportingApi` (same structure, different project path).

**File:** `docker-compose.yml`
```yaml
services:

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: dealflow
      POSTGRES_USER: dealflow
      POSTGRES_PASSWORD: dealflow
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dealflow"]
      interval: 5s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    healthcheck:
      test: rabbitmq-diagnostics ping
      interval: 10s
      timeout: 10s
      retries: 5

  deal-intake-api:
    build:
      context: .
      dockerfile: src/DealFlow.IntakeApi/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=dealflow;Username=dealflow;Password=dealflow
      - RabbitMQ__Host=rabbitmq
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  deal-scoring-worker:
    build:
      context: .
      dockerfile: src/DealFlow.ScoringWorker/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=dealflow;Username=dealflow;Password=dealflow
      - RabbitMQ__Host=rabbitmq
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  deal-notify:
    build:
      context: .
      dockerfile: src/DealFlow.NotifyWorker/Dockerfile
    environment:
      - RabbitMQ__Host=rabbitmq
      - SendGrid__ApiKey=${SENDGRID_API_KEY:-}
    depends_on:
      rabbitmq:
        condition: service_healthy

  deal-reporting-api:
    build:
      context: .
      dockerfile: src/DealFlow.ReportingApi/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=dealflow;Username=dealflow;Password=dealflow
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  postgres_data:
```

**Step: Verify local stack starts**
```bash
docker compose up --build
# Wait for all services to be healthy
# Then test:
curl http://localhost:5001/health
curl http://localhost:5002/health
# Both should return: {"status":"healthy","service":"..."}
```

**Step: Commit**
```bash
git add -A
git commit -m "feat: add Dockerfiles and docker-compose for full local stack"
```

---

## Phase 9: Integration Tests

### Task 11: End-to-End Integration Test

**File:** `tests/DealFlow.Integration.Tests/DealPipelineTests.cs`
```csharp
using DealFlow.IntakeApi.Models;
using FluentAssertions;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace DealFlow.Integration.Tests;

public class DealPipelineTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("dealflow").WithUsername("dealflow").WithPassword("dealflow")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbit.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _rabbit.DisposeAsync();
    }

    [Fact]
    public async Task Submit_deal_returns_201_with_RECEIVED_status()
    {
        await using var factory = new DealFlowWebAppFactory(
            _postgres.GetConnectionString(),
            _rabbit.GetConnectionString());

        var client = factory.CreateClient();

        var request = new SubmitDealRequest(
            "Forklift", 2022, 150_000, 36, "Logistics", "BC", "B");

        var response = await client.PostAsJsonAsync("/api/v1/deals", request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        body!.status.ToString().Should().Be("RECEIVED");
    }
}
```

**File:** `tests/DealFlow.Integration.Tests/DealFlowWebAppFactory.cs`
```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DealFlow.Data;

namespace DealFlow.Integration.Tests;

public class DealFlowWebAppFactory(string pgConnStr, string rabbitConnStr)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", pgConnStr);
        builder.UseSetting("RabbitMQ:Host", new Uri(rabbitConnStr).Host);
        builder.UseSetting("RabbitMQ:Username", "guest");
        builder.UseSetting("RabbitMQ:Password", "guest");
    }
}
```

**Step: Run integration tests**
```bash
dotnet test tests/DealFlow.Integration.Tests/ -v
```

**Step: Commit**
```bash
git add -A
git commit -m "test: add integration tests with Testcontainers"
```

---

## Phase 10: CI/CD (GitHub Actions)

### Task 12: CI Workflow

**File:** `.github/workflows/ci.yml`
```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore DealFlow.sln
      - run: dotnet build DealFlow.sln --no-restore
      - run: dotnet test DealFlow.sln --no-build --verbosity normal

  build-images:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - uses: docker/login-action@v3
        with:
          registry: ${{ secrets.ACR_LOGIN_SERVER }}
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}
      - name: Build and push intake-api
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/DealFlow.IntakeApi/Dockerfile
          platforms: linux/amd64,linux/arm64
          push: true
          tags: ${{ secrets.ACR_LOGIN_SERVER }}/deal-intake-api:${{ github.sha }}
      - name: Build and push scoring-worker
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/DealFlow.ScoringWorker/Dockerfile
          platforms: linux/amd64,linux/arm64
          push: true
          tags: ${{ secrets.ACR_LOGIN_SERVER }}/deal-scoring-worker:${{ github.sha }}
      - name: Build and push notify-worker
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/DealFlow.NotifyWorker/Dockerfile
          platforms: linux/amd64,linux/arm64
          push: true
          tags: ${{ secrets.ACR_LOGIN_SERVER }}/deal-notify-worker:${{ github.sha }}
      - name: Build and push reporting-api
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/DealFlow.ReportingApi/Dockerfile
          platforms: linux/amd64,linux/arm64
          push: true
          tags: ${{ secrets.ACR_LOGIN_SERVER }}/deal-reporting-api:${{ github.sha }}
```

**Step: Commit**
```bash
git add -A
git commit -m "ci: add GitHub Actions build and test workflow"
```

---

## Phase 11: Azure Infrastructure

### Task 13: Azure Setup (User-executed, one-time)

```bash
# Login
az login

# Variables (customize these)
RESOURCE_GROUP="rg-dealflow"
LOCATION="canadacentral"
ACR_NAME="dealflowacr"            # must be globally unique
ACA_ENV="dealflow-env"
PG_SERVER="dealflow-pg"
PG_PASSWORD="Deal@Flow2026!"

# Resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Azure Container Registry
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic --admin-enabled true

# Get ACR credentials → add as GitHub Secrets
az acr show --name $ACR_NAME --query loginServer --output tsv
az acr credential show --name $ACR_NAME --query "{username:username, password:passwords[0].value}"

# PostgreSQL Flexible Server
az postgres flexible-server create \
  --resource-group $RESOURCE_GROUP \
  --name $PG_SERVER \
  --location $LOCATION \
  --admin-user dealflow \
  --admin-password "$PG_PASSWORD" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16 \
  --yes

az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $PG_SERVER \
  --database-name dealflow

# Allow Azure services to access Postgres
az postgres flexible-server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --name $PG_SERVER \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Azure Service Bus (replaces RabbitMQ in cloud)
az servicebus namespace create \
  --resource-group $RESOURCE_GROUP \
  --name dealflow-sb \
  --sku Basic \
  --location $LOCATION

# Container Apps Environment
az containerapp env create \
  --name $ACA_ENV \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Deploy intake-api
az containerapp create \
  --name deal-intake-api \
  --resource-group $RESOURCE_GROUP \
  --environment $ACA_ENV \
  --image "$ACR_NAME.azurecr.io/deal-intake-api:latest" \
  --registry-server "$ACR_NAME.azurecr.io" \
  --registry-username $(az acr credential show --name $ACR_NAME --query username -o tsv) \
  --registry-password $(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv) \
  --target-port 8080 \
  --ingress external \
  --env-vars \
    "ConnectionStrings__DefaultConnection=Host=$PG_SERVER.postgres.database.azure.com;Database=dealflow;Username=dealflow;Password=$PG_PASSWORD;SslMode=Require" \
    "RabbitMQ__Host=dealflow-sb.servicebus.windows.net" \
  --min-replicas 1 --max-replicas 3

# (Repeat for scoring-worker, notify-worker, reporting-api with --ingress internal for workers)
```

### Task 14: GitHub Secrets Setup

In your GitHub repo → Settings → Secrets → Actions, add:
- `ACR_LOGIN_SERVER` = `dealflowacr.azurecr.io`
- `ACR_USERNAME` = (from `az acr credential show`)
- `ACR_PASSWORD` = (from `az acr credential show`)
- `SENDGRID_API_KEY` = (optional, from sendgrid.com free account)

---

## Phase 12: Documentation

### Task 15: README + Demo Script

**File:** `README.md`
```markdown
# DealFlow Sandbox

Microservices deal pipeline built with .NET 10, MassTransit, PostgreSQL, and RabbitMQ.

## Quick Start (Local)

**Prerequisites:** Docker Desktop

\```bash
docker compose up --build
\```

| Endpoint | URL |
|---|---|
| Submit deals (Swagger) | http://localhost:5001/swagger |
| View reports (Swagger) | http://localhost:5002/swagger |
| RabbitMQ Dashboard | http://localhost:15672 (guest/guest) |

## Architecture

deal-intake-api → RabbitMQ → deal-scoring-worker → RabbitMQ → deal-notify
                                                              ↓
                                                  deal-reporting-api (read-only)

## Services

| Service | Role |
|---|---|
| deal-intake-api | Accept deal submissions, publish events |
| deal-scoring-worker | Score deals asynchronously |
| deal-notify | Send notifications on scoring |
| deal-reporting-api | Query deals and audit timeline |
```

**File:** `docs/demo.md`
```markdown
# Demo Script

## Step 1: Start the stack
\```bash
docker compose up --build
\```

## Step 2: Submit a deal
\```bash
curl -X POST http://localhost:5001/api/v1/deals \
  -H "Content-Type: application/json" \
  -d '{
    "equipmentType": "Excavator",
    "equipmentYear": 2019,
    "amount": 750000,
    "termMonths": 60,
    "industry": "Construction",
    "province": "ON",
    "vendorTier": "B"
  }'
\```

## Step 3: Watch worker process (scoring logs)
\```bash
docker compose logs deal-scoring-worker -f
\```

## Step 4: Check deal status
\```bash
curl http://localhost:5001/api/v1/deals/{id}
\```

## Step 5: View audit timeline
\```bash
curl http://localhost:5002/api/v1/deals/{id}/timeline
\```

## Step 6: Filter deals
\```bash
curl "http://localhost:5002/api/v1/deals?status=SCORED&minAmount=500000"
\```

## Step 7: RabbitMQ Dashboard
Open http://localhost:15672 → Queues → see deal.submitted and deal.scored queues
```

**Step: Final commit**
```bash
git add -A
git commit -m "docs: add README, demo script, and architecture docs"
git push origin main
```

---

## Verification Checklist

- [ ] `docker compose up --build` starts all 5 containers cleanly
- [ ] `POST /api/v1/deals` returns 201 with RECEIVED status
- [ ] Deal appears in scoring worker logs within 2 seconds
- [ ] `GET /api/v1/deals/{id}` shows SCORED status
- [ ] `GET /api/v1/deals/{id}/timeline` shows DealSubmitted + DealScored events
- [ ] `GET /api/v1/deals?status=SCORED` returns filtered list
- [ ] RabbitMQ dashboard shows queues at http://localhost:15672
- [ ] GitHub Actions CI passes on push to main
- [ ] Azure Container Apps URLs respond to health checks
