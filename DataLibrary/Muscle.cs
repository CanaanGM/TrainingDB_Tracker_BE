using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary;
public class Muscle
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ScientificName { get; set; }
    public string Description { get; set; } = null!;
}
