using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserProfileImage
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public bool? IsPrimary { get; set; }

    public string? Url { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
