using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingWeekDay
{
    public int Id { get; set; }

    public int? TrainingWeekId { get; set; }

    public int? TrainingDayId { get; set; }

    public int? OrderNumber { get; set; }

    public virtual TrainingDay? TrainingDay { get; set; }

    public virtual TrainingWeek? TrainingWeek { get; set; }
}
