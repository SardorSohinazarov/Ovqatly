using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Services;

public partial class UpdateHandlerService
{
    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;

        if (message.Text == "/start")
        {
            await botClient.SendMessage(
                chatId,
                "Salom! Menga ovqat rasmini yuboring (kalloriya hisoblayman) yoki xohlagan savolingizni bering.");
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
