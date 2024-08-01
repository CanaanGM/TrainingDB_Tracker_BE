using DataLibrary.Helpers;

namespace DataLibrary.Dtos;

public class EquipmentWriteDto
{
    private string _name;
    private string _newName;
    private string _languageCode;

    /// <summary>
    /// the UNIQUE identifier for the equipment, instead of passing a number or a GUID, u pass a human readable name.
    /// </summary>
    public required string Name
    {
        get => _name;
        init => _name = Utils.NormalizeString(value);
    }

    public string? NewName
    {
        get => _newName;
        set => _newName = Utils.NormalizeString(value);
    }

    public string? Description { get; set; }
    public double? WeightKg { get; set; }
    public string? HowTo { get; set; }

    public string LanguageCode
    {
        get => _languageCode;
        set => _languageCode = Utils.NormalizeString(value);
    }
}

public class EquipmentReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? WeightKg { get; set; }
    public string? HowTo { get; set; }
    public string LanguageCode { get; set; }

    public DateTime? CreatedAt { get; set; }
}