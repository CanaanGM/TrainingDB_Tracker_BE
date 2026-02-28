using System;
using System.Linq;
using System.ComponentModel;
using DataLibrary.Services;
using ModelContextProtocol.Server;
using SharedLibrary.Dtos;

namespace TrainingDB.McpServer.Core.Tools;
[McpServerToolType]
public class TrainingTools(ITrainingSessionService trainingSessionService)
{
    [McpServerTool(Name = "ping")]
    public Task<string> PingAsync() => Task.FromResult("pong");

    [McpServerTool(
        Name = "get_recent_training_sessions_24h",
        OpenWorld = false,
        ReadOnly = true
    )]
    [Description("Return a concise natural-language summary of the user's training in the past 24 hours, suitable for chat.")]
    public async Task<string> GetRecentTrainingSessions24hAsync(
        [Description("the user id to get the sessions for, default is 1 (canaan)")]
        int userId = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await trainingSessionService.GetTrainingSessionsPast24HoursAsync(userId, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Could not retrieve recent sessions");
        }
        return Build24hSummary(result!.Value!, userId);
    }

    private static string Build24hSummary(ICollection<TrainingSessionReadDto> sessions, int userId)
    {
        var now = DateTime.Now;
        var windowStart = now.AddHours(-24);
        if (sessions == null || sessions.Count == 0)
        {
            return $"Past 24h summary for user {userId} ({windowStart:yyyy-MM-dd HH:mm} -> {now:yyyy-MM-dd HH:mm}): no sessions recorded.";
        }

        // Aggregate header
        double totalMinutes = sessions.Sum(s => s.DurationInMinutes);
        int totalCalories = sessions.Sum(s => s.TotalCaloriesBurned ?? 0);
        double totalReps = sessions.Sum(s => s.TotalRepetitions);
        double totalKg = sessions.Sum(s => s.TotalKgMoved);
        double avgRpe = sessions.Any() ? sessions.Average(s => s.AverageRateOfPreceivedExertion) : 0;
        double avgMood = sessions.Where(s => s.Mood.HasValue).DefaultIfEmpty().Average(s => s?.Mood ?? 0);

        var header = $"Past 24h summary for user {userId} ({windowStart:yyyy-MM-dd HH:mm} -> {now:yyyy-MM-dd HH:mm}): " +
                     $"{sessions.Count} session(s), {Math.Round(totalMinutes)} min, {totalCalories} kcal, {Math.Round(totalReps)} reps, {Math.Round(totalKg)} kg moved. " +
                     $"Avg RPE {Math.Round(avgRpe, 1)}, Avg mood {Math.Round(avgMood, 1)}/10.";

        // Per-session breakdown (chronological by CreatedAt)
        var ordered = sessions
            .OrderBy(s => s.CreatedAt ?? DateTime.MinValue)
            .ThenBy(s => s.Id)
            .ToList();

        string FormatRange(double minW, double maxW)
        {
            if (maxW <= 0) return string.Empty;
            return minW == maxW ? $" @ {minW:0.#}kg" : $" @ {minW:0.#}-{maxW:0.#}kg";
        }

        var lines = new System.Collections.Generic.List<string>();
        for (int i = 0; i < ordered.Count; i++)
        {
            var s = ordered[i];
            var created = s.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "n/a";
            var types = s.TrainingTypes
                .Select(t => t.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            var typesText = types.Count == 0 ? "n/a" : string.Join(", ", types);

            var recs = s.ExerciseRecords;
            var exStats = recs
                .GroupBy(r => r.ExerciseName)
                .Select(g => new {
                    Name = g.Key,
                    Sets = g.Count(),
                    Reps = g.Sum(r => r.Repetitions),
                    MinW = g.Where(x => x.WeightUsedKg.HasValue).Select(x => x.WeightUsedKg!.Value).DefaultIfEmpty(0).Min(),
                    MaxW = g.Where(x => x.WeightUsedKg.HasValue).Select(x => x.WeightUsedKg!.Value).DefaultIfEmpty(0).Max()
                })
                .OrderByDescending(x => x.Reps)
                .ThenByDescending(x => x.Sets)
                .ToList();

            var exercisesText = exStats.Count == 0
                ? "None"
                : string.Join(", ", exStats.Select(x => $"{x.Name}: {x.Reps} reps in {x.Sets} sets{FormatRange(x.MinW, x.MaxW)}"));

            var notes = string.IsNullOrWhiteSpace(s.Notes) ? "None" : s.Notes;
            var kcal = s.TotalCaloriesBurned ?? 0;
            var mood = s.Mood?.ToString() ?? "n/a";
            var rpe = Math.Round(s.AverageRateOfPreceivedExertion, 1);

            lines.Add($"Session {i + 1} (#{s.Id}, {created}): {Math.Round(s.DurationInMinutes)} min, {kcal} kcal, mood {mood}/10, avg RPE {rpe}; Types: {typesText}; Exercises: [{exercisesText}]; Notes: {notes}");
        }

        return header + Environment.NewLine + string.Join(Environment.NewLine, lines);
    }
}