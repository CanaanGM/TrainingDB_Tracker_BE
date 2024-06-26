using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class EquipmentMuscle
{
    public int Id { get; set; }

    public int? EquipmentId { get; set; }

    public int? MuscleId { get; set; }

    public bool? IsPrimary { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual Muscle? Muscle { get; set; }
}
