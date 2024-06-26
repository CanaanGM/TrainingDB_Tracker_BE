using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class AuthorLink
{
    public int Id { get; set; }

    public int? AuthorId { get; set; }

    public string? Url { get; set; }

    public DateTime? CreatedAt { get; set; }
}
