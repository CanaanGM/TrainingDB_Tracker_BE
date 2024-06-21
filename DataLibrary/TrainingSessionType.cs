namespace DataLibrary;
public class TrainingSessionType
{
    public int Id { get; set; }
    public TrainingSession TrainingSession { get; set; }
    public int TrainingSessionId { get; set; }
    public TrainingType TrainingType { get; set; }
    public int TriningTypeId { get; set; }

}
