using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double? Weight { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<EquipmentMuscle> EquipmentMuscles { get; set; } = new List<EquipmentMuscle>();

    public virtual ICollection<TrainingPlanEquipment> TrainingPlanEquipments { get; set; } = new List<TrainingPlanEquipment>();
}
