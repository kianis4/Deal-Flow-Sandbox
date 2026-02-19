using DealFlow.ReportingApi.Models;
using FluentAssertions;

namespace DealFlow.ReportingApi.Tests;

public class ModelTests
{
    [Fact]
    public void DealSummary_record_equality_works()
    {
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var a = new DealSummary(id, "Forklift", 100_000, "A", "RECEIVED", null, null, now);
        var b = new DealSummary(id, "Forklift", 100_000, "A", "RECEIVED", null, null, now);
        a.Should().Be(b);
    }

    [Fact]
    public void TimelineEvent_record_holds_data()
    {
        var now = DateTimeOffset.UtcNow;
        var e = new TimelineEvent("DealSubmitted", "{}", now);
        e.EventType.Should().Be("DealSubmitted");
        e.Payload.Should().Be("{}");
        e.OccurredAt.Should().Be(now);
    }
}
