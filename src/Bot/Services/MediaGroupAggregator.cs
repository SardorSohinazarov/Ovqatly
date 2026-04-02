using System.Collections.Concurrent;

namespace Bot.Services;

public interface IMediaGroupAggregator
{
    bool TryRegister(string mediaGroupId);
}

public sealed class MediaGroupAggregator : IMediaGroupAggregator
{
    private static readonly ConcurrentDictionary<string, byte> NotifiedMediaGroups = new();

    public bool TryRegister(string mediaGroupId)
    {
        if (!NotifiedMediaGroups.TryAdd(mediaGroupId, 0))
        {
            return false;
        }

        _ = RemoveLaterAsync(mediaGroupId);
        return true;
    }

    private static async Task RemoveLaterAsync(string mediaGroupId)
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
