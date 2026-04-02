namespace Domain.Entities;

public sealed class MealItem
{
    public long Id { get; set; }
    public long MealLogId { get; set; }
    public MealLog? MealLog { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Calories { get; set; }
    public int SortOrder { get; set; }
}
