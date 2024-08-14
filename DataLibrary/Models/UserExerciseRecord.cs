using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserExerciseRecord
{
    public int ExerciseRecordId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ExerciseRecord ExerciseRecord { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
