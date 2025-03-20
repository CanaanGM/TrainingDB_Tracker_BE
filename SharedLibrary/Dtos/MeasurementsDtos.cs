namespace SharedLibrary.Dtos;

public class MeasurementsReadDto 
{
    public int MeasurementsId { get; set; }
    public double Hip { get; set; }
    public double Chest { get; set; }
    public double WaistOnBelly { get; set; }
    public double WaistUnderBelly { get; set; }
    public double LeftThigh { get; set; }
    public double RightThigh { get; set; }
    public double LeftCalf { get; set; }
    public double RightCalf { get; set; }
    public double LeftUpperArm { get; set; }
    public double RightUpperArm { get; set; }
    public double LeftForearm { get; set; }
    public double RightForearm { get; set; }
    public double Neck { get; set; }
    // for in body test ⬇️
    public double BasalMetabolicRate { get; set; }
    public double TotalBodyWater { get; set; }
    public double BodyFatMass { get; set; }
    public double Protein { get; set; }
    public double Minerals { get; set; }
    public double BodyWeight { get; set; } // must be the sum of Minerals + Protein + BodyFatMass + TotalBodyWater
    public double SkeletalMuscleMass { get; set; }
    public double BodyFatPercentage { get; set; }
    public double InBodyScore { get; set; }
    public double VisceralFatLevel { get; set; } // from 1 ~ 9
    public double BodyMassIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class MeasurementsWriteDto 
{
    public required double Hip { get; set; }
    public required double Chest { get; set; }
    public required double WaistOnBelly { get; set; }
    public required double WaistUnderBelly { get; set; }
    public required double LeftThigh { get; set; }
    public required double RightThigh { get; set; }
    public required double LeftCalf { get; set; }
    public required double RightCalf { get; set; }
    public required double LeftUpperArm { get; set; }
    public required double RightUpperArm { get; set; }
    public required double LeftForearm { get; set; }
    public required double RightForearm { get; set; }
    public required double Neck { get; set; }
    // for in body test ⬇️
    public double BodyWeight { get; set; }
    public double? TotalBodyWater { get; set; }
    public double? BodyFatMass { get; set; }
    public double? Protein { get; set; }
    public double? Minerals { get; set; }
    public double? SkeletalMuscleMass { get; set; }
    public double? BodyFatPercent { get; set; }
    public double? InBodyScore { get; set; }
    public double? BodyMassIndex { get; set; }
    public double? VisceralFatLevel { get; set; } // from 1 ~ 9
    public double? BasalMetabolicRate { get; set; }
}