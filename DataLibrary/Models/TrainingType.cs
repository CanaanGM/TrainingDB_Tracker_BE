using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
