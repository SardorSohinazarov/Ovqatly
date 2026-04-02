using Bot.Services;

namespace Bot.Unit.Test;

public class AiResponseFormatterTests
{
    [Fact]
    public void Normalize_EmptyText_ReturnsNull()
    {
        Assert.Null(AiResponseFormatter.Normalize(null));
        Assert.Null(AiResponseFormatter.Normalize(string.Empty));
        Assert.Null(AiResponseFormatter.Normalize("   "));
    }

    [Fact]
    public void Normalize_StripsIntroAndKeepsTable()
    {
        var input = """
        Albatta, quyida jadval:

        | Ovqat nomi | Taxminiy kaloriya |
        |---|---|
        | Shashka | 400 kkal |

        Umumiy kaloriya yig'indisi: 400 kkal
        """;

        var normalized = AiResponseFormatter.Normalize(input);

        Assert.NotNull(normalized);
        Assert.StartsWith("| Ovqat nomi | Taxminiy kaloriya |", normalized!);
        Assert.Contains("Umumiy kaloriya yig'indisi: 400 kkal", normalized!);
        Assert.DoesNotContain("Albatta", normalized!);
    }
}
