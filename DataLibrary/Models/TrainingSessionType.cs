using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingSessionType
{
    public int Id { get; set; }

    public int? TrainingSessionId { get; set; }

    public int? TrainingTypeId { get; set; }

    public virtual TrainingSession? TrainingSession { get; set; }

    public virtual TrainingType? TrainingType { get; set; }
}
