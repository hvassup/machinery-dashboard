using System.Threading.Channels;
using Contracts.Commands;
using Contracts.Events;
using equipment_service.Options;
using MassTransit;
using Microsoft.Extensions.Options;

namespace equipment_service;

public class MachineWorker(
    Channel<BeginProcessOrderCommand> orderChannel,
    IBus bus,
    IOptions<MachineWorkerOptions> options,
    ILogger<MachineWorker> logger) : BackgroundService
{
    private enum State { Cold, WarmingUp, Processing, CoolingDown }

    private readonly string _machineId =
        Environment.GetEnvironmentVariable("EQUIPMENT_ID") ?? "unknown";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var processingSeconds = options.Value.ProcessingDelaySeconds;

        double warmUpStep   = 10;
        double coolDownStep = 10;

        var pending = new Queue<BeginProcessOrderCommand>();
        double pct = 0;
        var state = State.Cold;

        logger.LogInformation("Machine {MachineId} worker started", _machineId);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                switch (state)
                {
                    case State.Cold:
                        var first = await orderChannel.Reader.ReadAsync(ct);
                        pending.Enqueue(first);
                        pct = 0;
                        state = State.WarmingUp;
                        await Publish(new MachineWarmingUpEvent { MachineId = _machineId });
                        break;

                    case State.WarmingUp:
                        Drain(pending);
                        pct = Math.Min(100, pct + warmUpStep);
                        if (pct >= 100)
                            state = State.Processing;
                        break;

                    case State.Processing:
                        Drain(pending);
                        if (pending.Count == 0)
                        {
                            state = State.CoolingDown;
                            await Publish(new MachineCoolingDownEvent { MachineId = _machineId });
                            break;
                        }

                        var order = pending.Dequeue();
                        await Publish(new BeginningOrderEvent { MachineId = _machineId, OrderId = order.OrderId });
                        await Task.Delay(TimeSpan.FromSeconds(processingSeconds), ct);
                        await Publish(new FinishedOrderEvent  { MachineId = _machineId, OrderId = order.OrderId });
                        break;

                    case State.CoolingDown:
                        Drain(pending);
                        if (pending.Count > 0)
                        {
                            state = State.WarmingUp;
                            await Publish(new MachineWarmingUpEvent { MachineId = _machineId });
                            break;
                        }

                        pct = Math.Max(0, pct - coolDownStep);
                        if (pct <= 0)
                        {
                            state = State.Cold;
                            await Publish(new MachineIdleEvent { MachineId = _machineId });
                        }
                        break;
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), ct); // Just to simulate some sort of tick speed
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in machine worker {MachineId}", _machineId);
                await Task.Delay(1000, ct);
            }
        }
    }

    private void Drain(Queue<BeginProcessOrderCommand> queue)
    {
        while (orderChannel.Reader.TryRead(out var order))
            queue.Enqueue(order);
    }

    private Task Publish<T>(T message) where T : class => bus.Publish(message);
}
