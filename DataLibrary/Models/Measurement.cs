namespace DataLibrary.Models;

public partial class Measurement
{
    public int Id { get; set; }

    public double? Hip { get; set; }

    public double? Chest { get; set; }

    public double? WaistUnderBelly { get; set; }

    public double? WaistOnBelly { get; set; }

    public double? LeftThigh { get; set; }

    public double? RightThigh { get; set; }

    public double? LeftCalf { get; set; }

    public double? RightCalf { get; set; }

    public double? LeftUpperArm { get; set; }

    public double? LeftForearm { get; set; }

    public double? RightUpperArm { get; set; }

    public double? RightForearm { get; set; }

    public double? Neck { get; set; }

    public double? Minerals { get; set; }

    public double? Protein { get; set; }

    public double? TotalBodyWater { get; set; }

    public double? BodyFatMass { get; set; }

    public double? BodyWeight { get; set; }

    public double? BodyFatPercentage { get; set; }

    public double? SkeletalMuscleMass { get; set; }

    public double? InBodyScore { get; set; }

    public double? BodyMassIndex { get; set; }

    public int? BasalMetabolicRate { get; set; }

    public int? VisceralFatLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }
}
