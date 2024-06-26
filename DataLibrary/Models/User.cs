using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<UserImage> UserImages { get; set; } = new List<UserImage>();

    public virtual ICollection<UserPassword> UserPasswords { get; set; } = new List<UserPassword>();
}
