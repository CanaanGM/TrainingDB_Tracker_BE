namespace DataLibrary.Models;

public class Measurements
{
    public int MeasurementsId { get; set; }
    public int Hip { get; set; }
    public int Chest { get; set; }
    public int WaistOnBelly { get; set; }
    public int WaistUnderBelly { get; set; }
    public int LeftThigh { get; set; }
    public int RightThigh { get; set; }
    public int LeftCalf { get; set; }
    public int RightCalf { get; set; }
    public int LeftUpperArm { get; set; }
    public int RightUpperArm { get; set; }
    public int LeftForearm { get; set; }
    public int RightForearm { get; set; }
    public int Neck { get; set; }
    // for in body test ⬇️
    public int BasalMetabolicRate { get; set; }
    public double TotalBodyWater { get; set; }
    public double BodyFatMass { get; set; }
    public double Protein { get; set; }
    public double Minerals { get; set; }
    public double BodyWeight { get; set; } // must be the sum of Minerals + Protein + BodyFatMass + TotalBodyWater
    public double SkeletalMuscleMass { get; set; }
    public double BodyFatPercent { get; set; }
    public double InBodyScore { get; set; }
    public int VisceralFatLevel { get; set; } // from 1 ~ 9
    public double BodyMassIndex { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}