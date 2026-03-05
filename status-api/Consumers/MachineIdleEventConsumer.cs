using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class MachineIdleEventConsumer(AppDbContext db, PushNotificationService push)
    : IConsumer<MachineIdleEvent>
{
    public async Task Consume(ConsumeContext<MachineIdleEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine == null) return;

        machine.Status = "Idle";
        machine.CurrentOrderId = null;
        machine.LastSeen = evt.Timestamp;
        db.MachineEvents.Add(new MachineEvent
        {
            MachineId = evt.MachineId,
            EventType = "Idle",
            Timestamp = evt.Timestamp
        });

        await db.SaveChangesAsync();
        await push.SendMachineStatusAsync(evt.MachineId, "Idle");
    }
}
