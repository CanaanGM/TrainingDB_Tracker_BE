using DataLibrary.Dtos;
using Newtonsoft.Json;

namespace DateLibraryTests.helpers;

public static class PlanHelpers
{
    private static async Task<TrainingPlanWriteDto> ReadPlanFile(string planFile)
    {
        try
        {
            var jsonFile = await File.ReadAllTextAsync($@"E:\development\c#\TrainingDB_Integration\Docs\Plans\single\{planFile}.json");
            var plan = JsonConvert.DeserializeObject<TrainingPlanWriteDto>(jsonFile);
            return plan;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Deserialization error in file {planFile}: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the file {planFile}: {ex.Message}");
            return default;
        }
    }

    public static async Task<List<TrainingPlanWriteDto>> GeneratePlans()
    {
        var plans = new List<TrainingPlanWriteDto>();

        try
        {
            var planFiles = Directory.GetFiles(@"E:\development\c#\TrainingDB_Integration\Docs\Plans\single", "*.json");

            foreach (var planFile in planFiles)
            {
                var plan = await ReadPlanFile(Path.GetFileNameWithoutExtension(planFile));
                if (plan != null)
                {
                    plans.Add(plan);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while generating plans: {ex.Message}");
        }

        return plans;
    }

    public static async Task<List<TrainingPlanWriteDto>> GenerateBulkPlan()
    {
        try
        {
            var jsonFile = await File.ReadAllTextAsync($@"E:\development\c#\TrainingDB_Integration\Docs\Plans\bulk\plan_request.json");
            var plan = JsonConvert.DeserializeObject<List<TrainingPlanWriteDto>>(jsonFile);
            return plan;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while generating plans: {ex.Message}");
            return default;
        }
    }

}

