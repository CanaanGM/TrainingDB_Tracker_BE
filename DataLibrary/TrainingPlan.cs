namespace DataLibrary;
public class TrainingPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Notes { get; set; }
    public int TrainingDayCount { get; set; }
    public int TrainingWeekCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PlanWeek> Weeks { get; set; }
    public List<TrainingPlanEquipment> RequiredEquipments { get; set; }
    public List<TrainingPlanType> TrainingTypes { get; set; }



}
