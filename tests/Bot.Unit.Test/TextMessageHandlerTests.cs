using Bot.Services;
using Bot.Services.MessageHandlers;
using Moq;

namespace Bot.Unit.Test;

public class TextMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_StartCommand_SendsGuidanceMessage()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new TextMessageHandler(aiService.Object);
        var update = TestHelpers.CreateTextUpdate("/start", chatId: 42);

        bot.Setup(x => x.SendTextMessageAsync(
                42,
                It.Is<string>(text => text.Contains("Bitta ovqat uchun bitta rasm yuboring")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, update, CancellationToken.None);

        bot.Verify();
        aiService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_TextCommand_UsesAiResponse()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new TextMessageHandler(aiService.Object);
        var update = TestHelpers.CreateTextUpdate("salom", chatId: 77);

        aiService
            .Setup(x => x.GenerateChatReplyAsync("salom", It.IsAny<CancellationToken>()))
            .ReturnsAsync("javob matni");

        bot.Setup(x => x.SendTextMessageAsync(77, "javob matni", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, update, CancellationToken.None);

        aiService.VerifyAll();
        bot.Verify();
    }

    [Fact]
    public async Task HandleAsync_EmptyAiResponse_SendsFallbackMessage()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new TextMessageHandler(aiService.Object);
        var update = TestHelpers.CreateTextUpdate("salom", chatId: 99);

        aiService
            .Setup(x => x.GenerateChatReplyAsync("salom", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        bot.Setup(x => x.SendTextMessageAsync(
                99,
                It.Is<string>(text => text.Contains("javob tayyorlanmadi")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, update, CancellationToken.None);

        bot.Verify();
        aiService.VerifyAll();
    }
}
