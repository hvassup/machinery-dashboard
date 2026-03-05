using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class MachineWarmingUpEventConsumer(AppDbContext db, PushNotificationService push)
    : IConsumer<MachineWarmingUpEvent>
{
    public async Task Consume(ConsumeContext<MachineWarmingUpEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine == null) return;

        machine.Status = "WarmingUp";
        machine.LastSeen = evt.Timestamp;
        db.MachineEvents.Add(new MachineEvent
        {
            MachineId = evt.MachineId,
            EventType = "WarmingUp",
            Timestamp = evt.Timestamp
        });

        await db.SaveChangesAsync();
        await push.SendMachineStatusAsync(evt.MachineId, "WarmingUp");
    }
}
