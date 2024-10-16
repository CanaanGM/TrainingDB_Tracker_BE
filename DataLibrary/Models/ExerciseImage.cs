using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseImage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int ExerciseId { get; set; }

    public bool? IsPrimary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;
}
