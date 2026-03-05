namespace Contracts.Events;

public class BeginningOrderEvent
{
    public required string MachineId { get; set; }
    public required string OrderId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
