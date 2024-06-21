namespace DataLibrary;
public class TrainingPlanType
{
    public int Id { get; set; }
    public TrainingPlan TrainingPlan { get; set; }
    public int TrainingPlanId { get; set; }
    public TrainingType TrainingType { get; set; }
    public int TrainingTypeId { get; set; }
}
