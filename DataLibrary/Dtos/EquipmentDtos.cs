﻿namespace DataLibrary.Dtos;

public class EquipmentWriteDto
{
    /// <summary>
    /// the UNIQUE identifier for the equipment, instead of passing a number or a GUID, u pass a human readable name.
    /// </summary>
    public required string Name { get; init; } = null!;
    public string? NewName { get; set; }
    public string? Description { get; set; }
    public double? WeightKg { get; set; }
    public string? HowTo { get; set; }
    public string LanguageCode { get; set; } = "EN";
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