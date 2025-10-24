using Duotify.Membership.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Duotify.Membership.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly MembershipDbContext _dbContext;

    public HealthController(MembershipDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy" });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return Ok(new { status = "Ready", database = "Connected" });
        }
        catch
        {
            return StatusCode(503, new { status = "Not Ready", database = "Disconnected" });
        }
    }
}
