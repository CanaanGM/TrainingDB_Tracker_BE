using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseHowTo
{
    public int Id { get; set; }

    public int? ExerciseId { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public virtual Exercise? Exercise { get; set; }
}
