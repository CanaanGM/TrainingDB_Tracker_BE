using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary;
public class EquipmentMuscle
{
    public int Id { get; set; }
    public Equipment Equipment { get; set; }
    public int EquipmentId { get; set; }
    public Muscle Muscle { get; set; }
    public int MuscleId { get; set; }
}
