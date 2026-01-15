using Microsoft.AspNetCore.Mvc;

namespace Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get() => Ok("Bot is running");
    }
}
