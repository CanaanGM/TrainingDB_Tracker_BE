using Newtonsoft.Json;
using SharedLibrary.Dtos;

namespace TrainingTests.helpers;

public static class PlanHelpers
{
	public static List<TrainingDaysWriteDto> GenerateTrainingDays(int numberOfDays)
	{
		var days = new List<TrainingDaysWriteDto>();

		for (int i = 1; i <= numberOfDays; i++)
		{
			days.Add(new TrainingDaysWriteDto
			{
				Name = $"Day {i}",
				OrderNumber = i,
				Notes = $"Notes for Day {i}",
				Blocks = GenerateBlocks(2) // Assuming each day has 2 blocks
			});
		}

		return days;
	}

	private static List<BlockWriteDto> GenerateBlocks(int numberOfBlocks)
	{
		var blocks = new List<BlockWriteDto>();

		for (int i = 1; i <= numberOfBlocks; i++)
		{
			blocks.Add(new BlockWriteDto
			{
				Name = $"Block {i}",
				OrderNumber = i,
				Sets = 3,
				RestInSeconds = 60,
				Instructions = $"Instructions for Block {i}",
				BlockExercises = GenerateExercises(3) // Assuming each block has 3 exercises
			});
		}

		return blocks;
	}

	private static List<BlockExerciseWriteDto> GenerateExercises(int numberOfExercises)
	{
		var exercises = new List<BlockExerciseWriteDto>();

		for (int i = 1; i <= numberOfExercises; i++)
		{
			exercises.Add(new BlockExerciseWriteDto
			{
				ExerciseName = $"Exercise {i}",
				OrderNumber = i,
				Repetitions = 10,
				TimerInSeconds = 60,
				DistanceInMeters = 100,
				Instructions = "some instructions"
			});
		}

		return exercises;
	}

	public static async Task<TrainingPlanWriteDto> ReadPlanFile(string planFile)
	{
		try
		{
			var planPath = Path.Combine(
				PathHelpers.GetSolutionRoot() ?? throw new DirectoryNotFoundException("Solution root not found."),
				"Docs", "Plans", "single", $"{planFile}.json"
			);

			if (!File.Exists(planPath))
				throw new FileNotFoundException($"File not found: {planPath}");

			var jsonFile = await File.ReadAllTextAsync(planPath);
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

	public static async Task<List<TrainingPlanWriteDto>> GeneratePlansDtos()
	{
		var plans = new List<TrainingPlanWriteDto>();

		try
		{
			var planDirectory = Path.Combine(
					PathHelpers.GetSolutionRoot() ?? throw new DirectoryNotFoundException("Solution root not found."),
					"Docs", "Plans", "single"
				);

			if (!Directory.Exists(planDirectory))
				throw new DirectoryNotFoundException($"Directory not found: {planDirectory}");

			var jsonFiles = Directory.GetFiles(planDirectory, "*.json");

			if (jsonFiles.Length == 0)
				throw new FileNotFoundException($"No JSON files found in: {planDirectory}");

			foreach (var planFile in jsonFiles)
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
			var planPath = Path.Combine(
				PathHelpers.GetSolutionRoot() ?? throw new DirectoryNotFoundException("Solution root not found."),
				"Docs", "Plans", "bulk", "plan_request.json"
			);
			if (!File.Exists(planPath))
				throw new FileNotFoundException($"File not found: {planPath}");

			var jsonFile = await File.ReadAllTextAsync(planPath);
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

