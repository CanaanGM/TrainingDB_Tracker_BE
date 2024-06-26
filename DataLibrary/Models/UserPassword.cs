using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserPassword
{
    public string Id { get; set; } = null!;

    public string? UserId { get; set; }

    public string Password { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
