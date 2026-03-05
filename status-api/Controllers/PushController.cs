using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using status_api.Data;
using status_api.Models;
using status_api.Options;

namespace status_api.Controllers;

[ApiController]
[Route("push")]
public class PushController(AppDbContext db, IOptions<VapidOptions> vapidOptions) : ControllerBase
{
    [HttpGet("vapid-public-key")]
    public IActionResult GetPublicKey() =>
        Ok(new { publicKey = vapidOptions.Value.PublicKey });

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req)
    {
        if (!db.DeviceSubscriptions.Any(s => s.Endpoint == req.Endpoint))
        {
            db.DeviceSubscriptions.Add(new DeviceSubscription
            {
                Endpoint = req.Endpoint,
                P256dh = req.P256dh,
                Auth = req.Auth
            });
            await db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpDelete("subscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest req)
    {
        var sub = db.DeviceSubscriptions.FirstOrDefault(s => s.Endpoint == req.Endpoint);
        if (sub != null)
        {
            db.DeviceSubscriptions.Remove(sub);
            await db.SaveChangesAsync();
        }
        return Ok();
    }
}

public class SubscribeRequest
{
    public required string Endpoint { get; set; }
    public required string P256dh { get; set; }
    public required string Auth { get; set; }
}

public class UnsubscribeRequest
{
    public required string Endpoint { get; set; }
}
