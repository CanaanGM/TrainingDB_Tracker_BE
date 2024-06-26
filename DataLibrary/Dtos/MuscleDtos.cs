namespace DataLibrary.Dtos;

public class MuscleReadDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    //public virtual ICollection<EquipmentMuscle> EquipmentMuscles { get; set; } = new List<EquipmentMuscle>();

    //public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<MuscleGroupReadDto> MuscleGroups { get; set; } = new List<MuscleGroupReadDto>();
}
