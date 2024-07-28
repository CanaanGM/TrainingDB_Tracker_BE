namespace DataLibrary.Models;

public partial class LocalizedEquipment
{
    public int EquipmentId { get; set; }

    public int LanguageId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? HowTo { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Language Language { get; set; } = null!;
}
