namespace Contracts.Commands;

public class BeginProcessOrderCommand
{
    public required string MachineId { get; set; }
    public required string OrderId { get; set; }
    public required string ProductId { get; set; }
    public int Quantity { get; set; }
}
