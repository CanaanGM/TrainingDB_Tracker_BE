using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserPassword
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public bool? IsCurrent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
