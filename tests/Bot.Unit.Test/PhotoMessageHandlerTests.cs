using Bot.Services;
using Bot.Services.MessageHandlers;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot.Types;

namespace Bot.Unit.Test;

public class PhotoMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_SinglePhoto_AnalyzesAndSendsResult()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var mediaGroups = new Mock<IMediaGroupAggregator>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new PhotoMessageHandler(aiService.Object, mediaGroups.Object, NullLogger<PhotoMessageHandler>.Instance);
        var update = TestHelpers.CreatePhotoUpdate("file-1", chatId: 11);

        aiService
            .Setup(x => x.AnalyzeFoodImageAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("tahlil javobi");

        bot.Setup(x => x.GetFilePathAsync("file-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("path/file-1");

        bot.Setup(x => x.DownloadFileAsync("path/file-1", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((string _, Stream destination, CancellationToken _) =>
            {
                var bytes = new byte[] { 1, 2, 3 };
                return destination.WriteAsync(bytes, 0, bytes.Length);
            });

        bot.Setup(x => x.SendTextMessageAsync(11, "tahlil javobi", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, update, CancellationToken.None);

        aiService.VerifyAll();
        bot.Verify();
        mediaGroups.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_MediaGroup_SendsSingleGuidanceMessage()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var mediaGroups = new Mock<IMediaGroupAggregator>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new PhotoMessageHandler(aiService.Object, mediaGroups.Object, NullLogger<PhotoMessageHandler>.Instance);
        var update = TestHelpers.CreatePhotoUpdate("file-2", mediaGroupId: "album-1", chatId: 22);

        mediaGroups.Setup(x => x.TryRegister("album-1")).Returns(true);
        bot.Setup(x => x.SendTextMessageAsync(
                22,
                It.Is<string>(text => text.Contains("har bir ovqatni alohida rasm qilib yuboring")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, update, CancellationToken.None);

        bot.Verify();
        aiService.VerifyNoOtherCalls();
        mediaGroups.VerifyAll();
    }

    [Fact]
    public async Task HandleAsync_DuplicateMediaGroup_DoesNotRepeatWarning()
    {
        var aiService = new Mock<IAiResponseService>(MockBehavior.Strict);
        var mediaGroups = new Mock<IMediaGroupAggregator>(MockBehavior.Strict);
        var bot = new Mock<ITelegramBotFacade>(MockBehavior.Strict);
        var handler = new PhotoMessageHandler(aiService.Object, mediaGroups.Object, NullLogger<PhotoMessageHandler>.Instance);
        var first = TestHelpers.CreatePhotoUpdate("file-3", mediaGroupId: "album-dup", chatId: 33);
        var second = TestHelpers.CreatePhotoUpdate("file-4", mediaGroupId: "album-dup", chatId: 33);

        mediaGroups.SetupSequence(x => x.TryRegister("album-dup"))
            .Returns(true)
            .Returns(false);

        bot.Setup(x => x.SendTextMessageAsync(
                33,
                It.Is<string>(text => text.Contains("har bir ovqatni alohida rasm qilib yuboring")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await handler.HandleAsync(bot.Object, first, CancellationToken.None);
        await handler.HandleAsync(bot.Object, second, CancellationToken.None);

        bot.Verify(x => x.SendTextMessageAsync(
                33,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        aiService.VerifyNoOtherCalls();
        mediaGroups.VerifyAll();
    }
}
