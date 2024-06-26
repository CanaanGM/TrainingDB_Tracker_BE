namespace DataLibrary.Dtos;
public class MuscleGroupReadDto
{
    public int Id { get; set; }

    public string ScientificName { get; set; } = null!;

    public string? CommonName { get; set; }

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }



    //public virtual ICollection<TrainingDayMuscleGroup> TrainingDayMuscleGroups { get; set; } = new List<TrainingDayMuscleGroup>();
}
