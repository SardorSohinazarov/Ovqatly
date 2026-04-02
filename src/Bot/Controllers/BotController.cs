using Microsoft.AspNetCore.Mvc;

namespace Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "ok", service = "Bot" });
    }
}
