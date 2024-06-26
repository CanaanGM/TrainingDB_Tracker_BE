using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingDayBlock
{
    public int Id { get; set; }

    public int? TrainingDayId { get; set; }

    public int? BlockId { get; set; }

    public int? OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Block? Block { get; set; }

    public virtual TrainingDay? TrainingDay { get; set; }
}
