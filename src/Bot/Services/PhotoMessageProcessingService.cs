using Google.GenAI;
using Google.GenAI.Types;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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
    private static readonly TimeSpan AlbumDebounceDelay = TimeSpan.FromMilliseconds(1800);
    private readonly ConcurrentDictionary<string, PendingAlbum> _pendingAlbums = new();

    public async Task HandlePhotoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var photo = message.Photo?.LastOrDefault();

        if (photo is null)
        {
            logger.LogWarning("Photo message {MessageId} does not contain photo sizes.", message.MessageId);
            return;
        }

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

        var album = _pendingAlbums.GetOrAdd(message.MediaGroupId, _ => new PendingAlbum());
        album.AddImage(imageBytes);
        var debounceToken = album.ResetDebounce();

        _ = FinalizeAlbumAsync(
            botClient,
            message.MediaGroupId,
            chatId,
            album,
            debounceToken,
            cancellationToken);
    }

    private async Task FinalizeAlbumAsync(
        ITelegramBotClient botClient,
        string mediaGroupId,
        long chatId,
        PendingAlbum album,
        CancellationToken debounceToken,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(AlbumDebounceDelay, debounceToken);
            
            if (!_pendingAlbums.TryGetValue(mediaGroupId, out var currentAlbum))
            {
                return;
            }

            if (!ReferenceEquals(album, currentAlbum) || !currentAlbum.IsCurrent(debounceToken))
            {
                return;
            }

            if (!_pendingAlbums.TryRemove(mediaGroupId, out var removedAlbum))
            {
                return;
            }

            var images = removedAlbum.Snapshot();
            if (images.Count == 0)
            {
                logger.LogWarning("Skipping empty media group {MediaGroupId}", mediaGroupId);
                return;
            }

            await ProcessImagesAsync(botClient, chatId, images, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to finalize media group {MediaGroupId}", mediaGroupId);
        }
    }

    private async Task ProcessImagesAsync(
        ITelegramBotClient botClient,
        long chatId,
        List<byte[]> images,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId,
            "Rasmlar tahlil qilinyapti...",
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
                text,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process images for chat {ChatId}", chatId);
            await botClient.SendMessage(
                chatId,
                "Kechirasiz, rasmlarni tahlil qilishda xatolik yuz berdi. Iltimos, birozdan keyin qayta urinib ko'ring.",
                cancellationToken: cancellationToken);
        }
    }

    private sealed class PendingAlbum
    {
        private readonly object _gate = new();
        private CancellationTokenSource? _debounceCts;
        private readonly ConcurrentQueue<byte[]> _images = new();

        public void AddImage(byte[] imageBytes)
        {
            _images.Enqueue(imageBytes);
        }

        public CancellationToken ResetDebounce()
        {
            lock (_gate)
            {
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _debounceCts = new CancellationTokenSource();
                return _debounceCts.Token;
            }
        }

        public bool IsCurrent(CancellationToken token)
        {
            lock (_gate)
            {
                return _debounceCts?.Token == token;
            }
        }

        public List<byte[]> Snapshot() => _images.ToList();
    }
}
