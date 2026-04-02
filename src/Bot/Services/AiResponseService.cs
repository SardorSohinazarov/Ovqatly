using Google.GenAI;
using Google.GenAI.Types;
using Telegram.Bot.Types;

namespace Bot.Services;

public interface IAiResponseService
{
    Task<string?> GenerateChatReplyAsync(string prompt, CancellationToken cancellationToken);
    Task<string?> AnalyzeFoodImageAsync(byte[] imageBytes, CancellationToken cancellationToken);
}

public sealed class AiResponseService(Client geminiClient, ILogger<AiResponseService> logger) : IAiResponseService
{
    public async Task<string?> GenerateChatReplyAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var response = await geminiClient.Models.GenerateContentAsync(
                model: "gemini-3-flash-preview",
                contents: prompt);

            return AiResponseFormatter.Normalize(response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate chat reply.");
            return null;
        }
    }

    public async Task<string?> AnalyzeFoodImageAsync(byte[] imageBytes, CancellationToken cancellationToken)
    {
        try
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

            var response = await geminiClient.Models.GenerateContentAsync(
                model: "gemini-2.5-flash-lite",
                contents: new Content { Parts = parts });

            return AiResponseFormatter.Normalize(response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to analyze food image.");
            return null;
        }
    }
}
