namespace DataLibrary;

public class WeekDay
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsRest { get; set; } = false;
    public int OrderNumber { get; set; }
    public string? Notes { get; set; }

    public PlanWeek Week { get; set; }
    public int WeekId { get; set; }
    List<WeekDayMuscleGroup> WeekDayMuscleGroups { get; set; }
    List<Block> Blocks { get; set; }
}