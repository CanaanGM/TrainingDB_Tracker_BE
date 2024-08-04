using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public double? WeightKg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? HowTo { get; set; }

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
