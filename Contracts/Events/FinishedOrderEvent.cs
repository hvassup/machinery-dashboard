namespace Contracts.Events;

public class FinishedOrderEvent : MachineEventBase
{
    public required string OrderId { get; set; }
}
