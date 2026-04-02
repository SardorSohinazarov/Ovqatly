using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Unit.Test;

internal static class TestHelpers
{
    internal static Update CreateTextUpdate(string text, long chatId = 1)
        => new()
        {
            Message = new Message
            {
                Text = text,
                Chat = new Chat
                {
                    Id = chatId,
                    Type = ChatType.Private
                }
            }
        };

    internal static Update CreatePhotoUpdate(
        string fileId,
        string? mediaGroupId = null,
        long chatId = 1)
        => new()
        {
            Message = new Message
            {
                Chat = new Chat
                {
                    Id = chatId,
                    Type = ChatType.Private
                },
                MediaGroupId = mediaGroupId,
                Photo = new[]
                {
                    new PhotoSize
                    {
                        FileId = fileId,
                        FileUniqueId = $"unique-{fileId}",
                        Width = 100,
                        Height = 100
                    }
                }
            }
        };
}
