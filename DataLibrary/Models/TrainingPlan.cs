namespace DataLibrary.Models;

public partial class TrainingPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingWeek> TrainingWeeks { get; set; } = new List<TrainingWeek>();
}
