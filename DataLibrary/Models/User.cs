namespace DataLibrary.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public double? Height { get; set; }

    public string? Gender { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();

    public virtual ICollection<UserExercise> UserExercises { get; set; } = new List<UserExercise>();

    public virtual ICollection<UserPassword> UserPasswords { get; set; } = new List<UserPassword>();

    public virtual ICollection<UserProfileImage> UserProfileImages { get; set; } = new List<UserProfileImage>();
}
