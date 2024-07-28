namespace DataLibrary.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public double? WeightKg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
