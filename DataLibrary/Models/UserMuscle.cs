namespace DataLibrary.Models;

public partial class UserMuscle
{
    public int? UserId { get; set; }

    public int? MuscleId { get; set; }

    public int? MuscleCooldown { get; set; }

    public int? Frequency { get; set; }

    public int? TrainingVolume { get; set; }

    public virtual Muscle? Muscle { get; set; }

    public virtual User? User { get; set; }
}
