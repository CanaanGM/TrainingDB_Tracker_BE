
namespace DataLibrary.Models;

public partial class Block
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Sets { get; set; }

    public int? RestInSeconds { get; set; }

    public string? Instructions { get; set; }

    public int? OrderNumber { get; set; }

    public int? TrainingDayId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual TrainingDay? TrainingDay { get; set; }
}
