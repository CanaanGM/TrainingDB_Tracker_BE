using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary;
public class MuscleGroupMuscle
{
    public int Id { get; set; }
    public Muscle Muscle { get; set; }
    public int MuscleId { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public int MuscleGroupId { get; set; }

}
