using Google.GenAI;
using Google.GenAI.Types;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Blob = Google.GenAI.Types.Blob;

namespace Bot.Services;

public interface IPhotoMessageProcessor
{
    Task HandlePhotoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}

public sealed class PhotoMessageProcessingService(
    Client geminiClient,
    ILogger<PhotoMessageProcessingService> logger) : IPhotoMessageProcessor
{
    private static readonly ConcurrentDictionary<string, byte> NotifiedMediaGroups = new();

    public async Task HandlePhotoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var photo = message.Photo?.LastOrDefault();

        if (photo is null)
        {
            logger.LogWarning("Photo message {MessageId} does not contain photo sizes.", message.MessageId);
            return;
        }

        if (message.MediaGroupId is not null)
        {
            if (NotifiedMediaGroups.TryAdd(message.MediaGroupId, 0))
            {
                await botClient.SendMessage(
                    chatId,
                    "Media group yubordingiz. Iltimos, har bir ovqatni alohida rasm qilib yuboring. Shunda men har birini alohida kaloriya bilan tahlil qilib beraman.",
                    cancellationToken: cancellationToken);

                _ = RemoveMediaGroupNotificationLaterAsync(message.MediaGroupId);
            }

            return;
        }

        var file = await botClient.GetFile(photo.FileId, cancellationToken);

        using var ms = new MemoryStream();
        await botClient.DownloadFile(file.FilePath!, ms, cancellationToken);
        var imageBytes = ms.ToArray();

        await ProcessImageAsync(
            botClient,
            chatId,
            imageBytes,
            cancellationToken);
    }

    private async Task ProcessImageAsync(
        ITelegramBotClient botClient,
        long chatId,
        byte[] imageBytes,
        CancellationToken cancellationToken)
    {
        var parts = new List<Part>
        {
            new Part
            {
                Text = """
                Quyidagi rasmda ko'rsatilgan ovqatni taxminiy kaloriya bo'yicha tahlil qil.

                Qattiq format qoidalari:
                - Hech qanday kirish so'zi yozma
                - Hech qanday izoh, eslatma yoki qo'shimcha matn yozma
                - Faqat jadval va yakuniy umumiy kaloriya qatori bo'lsin
                - Bitta rasm tahlil qilinyapti

                Format:
                | Ovqat nomi | Taxminiy kaloriya |
                |---|---|
                | ... | ... |

                Umumiy kaloriya yig'indisi: ... kkal

                Javob faqat shu formatda bo'lsin.
                """
            },
            new Part
            {
                InlineData = new Blob
                {
                    MimeType = "image/jpeg",
                    Data = imageBytes
                }
            }
        };

        try
        {
            var response = await geminiClient.Models.GenerateContentAsync(
                model: "gemini-2.5-flash-lite",
                contents: new Content { Parts = parts });

            var text = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogWarning("Gemini returned an empty response for chat {ChatId}", chatId);
                await botClient.SendMessage(
                    chatId,
                    "Kechirasiz, rasmni tahlil qilishda javob olinmadi. Iltimos, qayta urinib ko'ring.",
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.SendMessage(
                chatId,
                NormalizeAnalysisResponse(text),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process image for chat {ChatId}", chatId);
            await botClient.SendMessage(
                chatId,
                "Kechirasiz, rasmni tahlil qilishda xatolik yuz berdi. Iltimos, birozdan keyin qayta urinib ko'ring.",
                cancellationToken: cancellationToken);
        }
    }

    private static string NormalizeAnalysisResponse(string text)
    {
        var lines = text
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.None)
            .Select(line => line.Trim())
            .ToList();

        var firstRelevantLineIndex = lines.FindIndex(line =>
            line.StartsWith('|') ||
            line.StartsWith("Umumiy kaloriya yig'indisi", StringComparison.OrdinalIgnoreCase));

        if (firstRelevantLineIndex < 0)
        {
            return text.Trim();
        }

        var normalizedLines = lines
            .Skip(firstRelevantLineIndex)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return string.Join(System.Environment.NewLine, normalizedLines).Trim();
    }

    private static async Task RemoveMediaGroupNotificationLaterAsync(string mediaGroupId)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
        catch
        {
            return;
        }

        NotifiedMediaGroups.TryRemove(mediaGroupId, out _);
    }
}
