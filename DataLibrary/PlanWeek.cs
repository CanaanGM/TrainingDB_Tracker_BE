namespace DataLibrary;

public class PlanWeek
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int OrderNumber { get; set; } // 1st,2nd,3rd week of the plan

    public TrainingPlan TrainingPlan { get; set; }
    public int TrainingPlanId { get; set; }

    List<WeekDay> Days { get; set; }
}