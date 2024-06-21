using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary;
public class ExerciseMuscle
{
    public int Id { get; set; }
    public Muscle Muscle { get; set; }
    public int MuscleId { get; set; }
    public Exercise Exercise { get; set; }
    public int ExerciseId { get; set; }
    // training Chest, chest is the primary worked muscle but the shoulders, abs, and traps get involved to some extent
    public bool IsPrimary { get; set; } = false;
}
