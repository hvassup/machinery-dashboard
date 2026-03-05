using System.Threading.Channels;
using Contracts.Commands;
using MassTransit;

namespace equipment_service.CommandHandlers;

public class BeginProcessOrderCommandConsumer(
    Channel<BeginProcessOrderCommand> orderChannel,
    ILogger<BeginProcessOrderCommandConsumer> logger) : IConsumer<BeginProcessOrderCommand>
{
    public async Task Consume(ConsumeContext<BeginProcessOrderCommand> context)
    {
        logger.LogInformation("Queuing order {OrderId} for machine {MachineId}",
            context.Message.OrderId, context.Message.MachineId);
        await orderChannel.Writer.WriteAsync(context.Message, context.CancellationToken);
    }
}
