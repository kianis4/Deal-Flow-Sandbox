namespace DealFlow.Data.Entities;

public class DealEvent
{
    public Guid Id { get; set; }
    public Guid DealId { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTimeOffset OccurredAt { get; set; }
    public Deal Deal { get; set; } = default!;
}
