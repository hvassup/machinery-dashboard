using System.Threading.Channels;
using Contracts.Commands;
using Contracts.Events;
using MassTransit;

namespace equipment_service;

public class MachineWorker(
    Channel<BeginProcessOrderCommand> orderChannel,
    IBus bus,
    IConfiguration config,
    ILogger<MachineWorker> logger) : BackgroundService
{
    private enum State { Cold, WarmingUp, Processing, CoolingDown }

    private readonly string _machineId =
        Environment.GetEnvironmentVariable("EQUIPMENT_ID") ?? "unknown";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var tickMs         = config.GetValue<int>("TickIntervalMs", 500);
        var warmUpSeconds  = config.GetValue<int>("WarmUpDurationSeconds", 10);
        var coolDownSeconds= config.GetValue<int>("CoolDownDurationSeconds", 5);
        var processingSeconds = config.GetValue<int>("ProcessingDelaySeconds", 5);

        double warmUpStep   = 100.0 / (warmUpSeconds  * 1000.0 / tickMs);
        double coolDownStep = 100.0 / (coolDownSeconds * 1000.0 / tickMs);

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
                        await Publish(new WarmUpProgressEvent { MachineId = _machineId, Percentage = 0 });
                        break;

                    case State.WarmingUp:
                        await Task.Delay(tickMs, ct);
                        Drain(pending);
                        pct = Math.Min(100, pct + warmUpStep);
                        await Publish(new WarmUpProgressEvent { MachineId = _machineId, Percentage = (int)Math.Round(pct) });
                        if (pct >= 100) state = State.Processing;
                        break;

                    case State.Processing:
                        Drain(pending);
                        if (pending.Count == 0) { state = State.CoolingDown; break; }

                        var order = pending.Dequeue();
                        await Publish(new BeginningOrderEvent { MachineId = _machineId, OrderId = order.OrderId });
                        await Task.Delay(TimeSpan.FromSeconds(processingSeconds), ct);
                        await Publish(new FinishedOrderEvent  { MachineId = _machineId, OrderId = order.OrderId });
                        break;

                    case State.CoolingDown:
                        Drain(pending);
                        if (pending.Count > 0) { state = State.WarmingUp; break; }

                        await Task.Delay(tickMs, ct);
                        pct = Math.Max(0, pct - coolDownStep);
                        await Publish(new CoolDownProgressEvent { MachineId = _machineId, Percentage = (int)Math.Round(pct) });
                        if (pct <= 0) state = State.Cold;
                        break;
                }
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
