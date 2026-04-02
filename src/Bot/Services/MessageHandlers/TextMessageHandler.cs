using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services.MessageHandlers;

public interface ITextMessageHandler
{
    Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}

public sealed class TextMessageHandler(IAiResponseService aiResponseService) : ITextMessageHandler
{
    private const string StartMessage = """
    Salom! Menga ovqat rasmini yuboring, men taxminiy kaloriya hisoblab beraman.

    Qulay foydalanish uchun:
    - Bitta ovqat uchun bitta rasm yuboring
    - Agar album (media group) yuborsangiz, bot har bir ovqatni alohida rasm qilib yuborishni so'raydi
    """;

    public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message?.Text is null)
        {
            return;
        }

        var chatId = message.Chat.Id;

        if (message.Text == "/start")
        {
            await botClient.SendMessage(chatId, StartMessage, cancellationToken: cancellationToken);
            return;
        }

        var response = await aiResponseService.GenerateChatReplyAsync(message.Text, cancellationToken);
        if (string.IsNullOrWhiteSpace(response))
        {
            await botClient.SendMessage(
                chatId,
                "Kechirasiz, javob tayyorlanmadi. Iltimos, qayta urinib ko'ring.",
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
    }
}
