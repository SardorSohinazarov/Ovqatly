using Google.GenAI;
using Google.GenAI.Types;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Blob = Google.GenAI.Types.Blob;

namespace Bot.Services;

public partial class UpdateHandlerService(Client geminiClient)
{
    private static readonly ConcurrentDictionary<string, List<byte[]>> _mediaGroups = new();
    private async Task HandlePhotoAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;

        await botClient.SendMessage(chatId, "🧐 Rasmdagi ovqatni tahlil qilyapman...", cancellationToken: cancellationToken);

        var photo = message.Photo!.Last();
        var file = await botClient.GetFile(photo.FileId, cancellationToken);

        using var ms = new MemoryStream();
        await botClient.DownloadFile(file.FilePath!, ms, cancellationToken);
        var imageBytes = ms.ToArray();

        if (message.MediaGroupId is null)
        {
            await ProcessImagesAsync(
                botClient,
                chatId,
                new List<byte[]> { imageBytes },
                cancellationToken);

            return;
        }

        var images = _mediaGroups.GetOrAdd(
            message.MediaGroupId,
            _ => new List<byte[]>());

        images.Add(imageBytes);

        // 🔹 Album tugashini kutib, keyin analiz qilamiz
        _ = FinalizeMediaGroupAsync(
            botClient,
            message.MediaGroupId,
            chatId,
            cancellationToken);
    }

    private async Task FinalizeMediaGroupAsync(
        ITelegramBotClient botClient,
        string mediaGroupId,
        long chatId,
        CancellationToken cancellationToken)
    {
        // ⏳ Oxirgi rasm kelishini kutamiz
        await Task.Delay(1500, cancellationToken);

        if (!_mediaGroups.TryRemove(mediaGroupId, out var images))
            return;

        await ProcessImagesAsync(
            botClient,
            chatId,
            images,
            cancellationToken);
    }

    private async Task ProcessImagesAsync(
        ITelegramBotClient botClient,
        long chatId,
        List<byte[]> images,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId,
            "🧐 Rasmlar tahlil qilinyapti...",
            cancellationToken: cancellationToken);

        var parts = new List<Part>
        {
            new Part
            {
                Text = """
                Quyidagi rasmlardagi ovqatlarni ALOHIDA tahlil qil.

                Har bir rasm uchun:
                - Ovqat nomi
                - Taxminiy kaloriya

                Oxirida:
                - Har bir rasm kaloriyasi
                - Umumiy kaloriya yig‘indisini chiqar.

                Javobni o‘zbek tilida JADVAL ko‘rinishida ber.
                """
            }
        };

        foreach (var img in images)
        {
            parts.Add(new Part
            {
                InlineData = new Blob
                {
                    MimeType = "image/jpeg",
                    Data = img
                }
            });
        }

        var response = await geminiClient.Models.GenerateContentAsync(
            model: "gemini-2.5-flash-lite",
            contents: new Content { Parts = parts });

        await botClient.SendMessage(
            chatId,
            response.Candidates[0].Content.Parts[0].Text,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }
}
