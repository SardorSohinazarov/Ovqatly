using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Bot.Services;

public class BotBackgroundService(
    ILogger<BotBackgroundService> logger,
    TelegramBotClient botClient,
    IUpdateHandler updateHandler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = await botClient.GetMe(stoppingToken);

        logger.LogInformation("Telegram bot :{bot.Username} is started listening", bot.Username);

        botClient.StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            errorHandler: updateHandler.HandleErrorAsync,
            receiverOptions: new ReceiverOptions(){ DropPendingUpdates = true },
            cancellationToken: stoppingToken);
    }
}

