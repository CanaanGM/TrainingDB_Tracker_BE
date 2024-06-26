using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
