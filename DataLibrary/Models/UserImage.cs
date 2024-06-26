using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserImage
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string Url { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
