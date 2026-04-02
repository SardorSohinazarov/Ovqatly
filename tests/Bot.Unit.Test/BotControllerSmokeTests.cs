using Bot.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Bot.Unit.Test;

public class BotControllerSmokeTests
{
    [Fact]
    public void Health_ReturnsOkStatus()
    {
        var controller = new BotController();

        var result = controller.Health();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var statusProperty = okResult.Value!.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("ok", statusProperty!.GetValue(okResult.Value));
    }
}
