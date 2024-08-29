using SharedLibrary.Dtos;

namespace TrainingTests.helpers;

public static class MeasurementsDtoFactory
{
    public static List<MeasurementsWriteDto> CreateDtosForUserIdOne() =>
    [
    ];
    
    public static MeasurementsWriteDto CreateOneNoBodyWeight() => new MeasurementsWriteDto()
    {
        Chest = 10,
        Hip = 10,
        Neck = 10,
        LeftThigh = 10,
        LeftCalf = 10,
        RightCalf = 10,
        RightForearm = 10,
        RightThigh = 10,
        LeftForearm = 10,
        LeftUpperArm = 10,
        RightUpperArm = 10,
        WaistOnBelly = 10,
        WaistUnderBelly = 10,
    };
    public static MeasurementsWriteDto CreateOneWithBodyWeight() => new MeasurementsWriteDto()
    {
        Chest = 10,
        Hip = 10,
        Neck = 10,
        LeftThigh = 10,
        LeftCalf = 10,
        RightCalf = 10,
        RightForearm = 10,
        RightThigh = 10,
        LeftForearm = 10,
        LeftUpperArm = 10,
        RightUpperArm = 10,
        WaistOnBelly = 10,
        WaistUnderBelly = 10,
        BodyWeight = 65.5
    };   
    public static MeasurementsWriteDto CreateOneWithBMI() => new MeasurementsWriteDto()
    {
        Chest = 10,
        Hip = 10,
        Neck = 10,
        LeftThigh = 10,
        LeftCalf = 10,
        RightCalf = 10,
        RightForearm = 10,
        RightThigh = 10,
        LeftForearm = 10,
        LeftUpperArm = 10,
        RightUpperArm = 10,
        WaistOnBelly = 10,
        WaistUnderBelly = 10,
        Protein = 12,
        Minerals = 12,
        BodyFatMass = 41,
        TotalBodyWater = 54
    };
    
public static MeasurementsWriteDto CreateOneWithBMIAndWeight() => new MeasurementsWriteDto()
    {
        Chest = 10,
        Hip = 10,
        Neck = 10,
        LeftThigh = 10,
        LeftCalf = 10,
        RightCalf = 10,
        RightForearm = 10,
        RightThigh = 10,
        LeftForearm = 10,
        LeftUpperArm = 10,
        RightUpperArm = 10,
        WaistOnBelly = 10,
        WaistUnderBelly = 10,
        Protein = 12,
        Minerals = 12,
        BodyFatMass = 41,
        TotalBodyWater = 54,
        BodyWeight = 99
    };
    
    
}