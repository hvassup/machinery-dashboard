namespace Contracts.Events;

public class BeginningOrderEvent : MachineEventBase
{
    public required string OrderId { get; set; }
}
