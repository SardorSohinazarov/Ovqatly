using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Google.GenAI;
using Google.GenAI.Types;
using Blob = Google.GenAI.Types.Blob;

namespace Bot.Services;

public partial class UpdateHandlerService(Client geminiClient)
{
    private async Task HandlePhotoAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;

        await botClient.SendMessage(chatId, "🧐 Rasmdagi ovqatni tahlil qilyapman...", cancellationToken: cancellationToken);
        var fileId = message.Photo[^1].FileId;
        var fileInfo = await botClient.GetFile(fileId, cancellationToken);
        using var ms = new MemoryStream();
        await botClient.DownloadFile(fileInfo.FilePath!, ms, cancellationToken);
        byte[] imageBytes = ms.ToArray();
        var imagePart = new Part()
        {
            InlineData = new Blob
            {
                MimeType = "image/jpeg",
                Data = imageBytes // Base64 ga o'girish shart
            }
        }; var textPart = new Part { Text = "Ushbu rasmdagi ovqatni aniqla. Har bir mahsulotning nomi va taxminiy kalloriyasini o'zbek tilida jadval ko'rinishida yozib ber. Oxirida umumiy kalloriyani hisobla." };
        
        var response = await geminiClient.Models.GenerateContentAsync(
            model: "gemini-3-flash-preview",
            contents: new Content { Parts = new List<Part> { textPart, imagePart } });

        await botClient.SendMessage(chatId, $"📊 **Tahlil natijasi:**\n\n{response.Candidates[0].Content.Parts[0].Text}",
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
    }
}
