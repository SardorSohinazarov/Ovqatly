using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services;

public partial class UpdateHandlerService
{
    private const string StartMessage = """
    Salom! Menga ovqat rasmini yuboring, men taxminiy kaloriya hisoblab beraman.

    Qulay foydalanish uchun:
    - Bitta ovqat uchun bitta rasm yuboring
    - Agar album (media group) yuborsangiz, bot har bir ovqatni alohida rasm qilib yuborishni so'raydi
    """;

    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;

        if (message.Text == "/start")
        {
            await botClient.SendMessage(
                chatId,
                StartMessage);
            return;
        }
        var response = await geminiClient.Models.GenerateContentAsync(
            model: "gemini-3-flash-preview",
            contents: message.Text);

        await botClient.SendMessage(chatId,
            response.Candidates[0].Content.Parts[0].Text,
            cancellationToken: cancellationToken);
    }
}
