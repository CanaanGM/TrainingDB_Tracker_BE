using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary;
public class TrainingPlanEquipment
{
    public int Id { get; set; }
    public TrainingPlan Plan { get; set; }
    public int PlanId { get; set; }
    public Equipment Equipment { get; set; }
    public int EquipmentId { get; set; }
}
