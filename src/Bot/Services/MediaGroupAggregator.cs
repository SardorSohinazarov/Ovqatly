using System.Collections.Concurrent;

namespace Bot.Services;

public interface IMediaGroupAggregator
{
    bool TryRegister(string mediaGroupId);
}

public sealed class MediaGroupAggregator : IMediaGroupAggregator
{
    private readonly ConcurrentDictionary<string, byte> _notifiedMediaGroups = new();

    public bool TryRegister(string mediaGroupId)
    {
        if (!_notifiedMediaGroups.TryAdd(mediaGroupId, 0))
        {
            return false;
        }

        _ = RemoveLaterAsync(mediaGroupId);
        return true;
    }

    private async Task RemoveLaterAsync(string mediaGroupId)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
        catch
        {
            return;
        }

        _notifiedMediaGroups.TryRemove(mediaGroupId, out _);
    }
}
