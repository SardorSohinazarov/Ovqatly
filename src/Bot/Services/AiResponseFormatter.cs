namespace Bot.Services;

public static class AiResponseFormatter
{
    public static string? Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

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
}
