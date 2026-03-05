using Microsoft.Extensions.Options;
using status_api.Data;
using status_api.Options;
using WebPush;

namespace status_api.Services;

public class PushNotificationService(AppDbContext db, IOptions<VapidOptions> vapidOptions, ILogger<PushNotificationService> logger)
{
    public async Task SendMachineStatusAsync(string machineId, string status)
    {
        var vapid = vapidOptions.Value;

        if (string.IsNullOrEmpty(vapid.PublicKey) || string.IsNullOrEmpty(vapid.PrivateKey))
        {
            logger.LogWarning("VAPID keys not configured — skipping push notification");
            return;
        }

        var subscriptions = db.DeviceSubscriptions.ToList();
        if (subscriptions.Count == 0) return;

        var client = new WebPushClient();
        client.SetVapidDetails(vapid.Subject, vapid.PublicKey, vapid.PrivateKey);

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = $"{machineId}",
            body = $"Status changed to {status}"
        });

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(pushSub, payload);
            }
            catch (WebPushException ex) when ((int)ex.StatusCode is 404 or 410)
            {
                db.DeviceSubscriptions.Remove(sub);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send push to {Endpoint}", sub.Endpoint);
            }
        }
    }
}
