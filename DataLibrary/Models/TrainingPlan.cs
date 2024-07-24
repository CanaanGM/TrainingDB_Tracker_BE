using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingPlan
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public virtual ICollection<TrainingWeek> Weeks { get; set; } = new List<TrainingWeek>();
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}