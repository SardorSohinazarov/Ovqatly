namespace Domain.Entities;

public sealed class MealLog
{
    public long Id { get; set; }
    public long AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public MealSourceType SourceType { get; set; }
    public string? RawInput { get; set; }
    public string? AiRawResponse { get; set; }
    public int? TotalCalories { get; set; }
    public MealLogStatus Status { get; set; } = MealLogStatus.Pending;
    public string? ModelName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public ICollection<MealItem> Items { get; set; } = new List<MealItem>();
}
