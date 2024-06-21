namespace DataLibrary;
public class WeekDayMuscleGroup
{
    public int Id { get; set; }
    public WeekDay WeekDay { get; set; }
    public int WeekDayId { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public int MuscleGroupId { get; set; }
}
