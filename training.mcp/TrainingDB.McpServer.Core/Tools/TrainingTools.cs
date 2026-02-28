using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using DataLibrary.Services;
using ModelContextProtocol.Server;
using SharedLibrary.Dtos;

namespace TrainingDB.McpServer.Core.Tools;
[McpServerToolType]
public class TrainingTools(ITrainingSessionService trainingSessionService, IExerciseService exerciseService)
{
    [McpServerTool(Name = "ping")]
    public Task<string> PingAsync() => Task.FromResult("pong");

    [McpServerTool(
        Name = "get_recent_training_sessions_24h",
        OpenWorld = false,
        ReadOnly = true
    )]
    [Description("Summarize the user's past 24 hours of training with totals and per-session metrics (no exercise descriptions).")]
    public async Task<string> GetRecentTrainingSessions24hAsync(
        [Description("The user id to get the sessions for; default is 1 (canaan)")]
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

    [McpServerTool(
        Name = "summarize_sessions_24h_with_exercise_descriptions",
        OpenWorld = false,
        ReadOnly = true
    )]
    [Description("For the past 24 hours, return a per-session list of exercises with short canonical descriptions fetched via ExerciseService. Use this when you need natural-language context about the exercises performed.")]
    public async Task<string> SummarizeSessions24hWithExerciseDescriptionsAsync(
        [Description("The user id to get the sessions for; default is 1 (canaan)")] int userId = 1,
        CancellationToken cancellationToken = default)
    {
        var sessionsResult = await trainingSessionService.GetTrainingSessionsPast24HoursAsync(userId, cancellationToken);
        if (!sessionsResult.IsSuccess)
            throw new InvalidOperationException(sessionsResult.ErrorMessage ?? "Could not retrieve recent sessions");

        var sessions = sessionsResult!.Value!;
        var now = DateTime.Now;
        var windowStart = now.AddHours(-24);
        if (sessions.Count == 0)
            return $"Exercises in past 24h for user {userId} ({windowStart:yyyy-MM-dd HH:mm} -> {now:yyyy-MM-dd HH:mm}): no sessions recorded.";

        // Collect unique exercise names
        var exerciseNames = sessions
            .SelectMany(s => s.ExerciseRecords.Select(r => r.ExerciseName))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Fetch descriptions via ExerciseService and cache
        var descByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in exerciseNames)
        {
            try
            {
                var ex = await exerciseService.GetByNameAsync(name, cancellationToken);
                if (ex.IsSuccess && ex.Value is not null)
                {
                    var d = string.IsNullOrWhiteSpace(ex.Value.Description) ? ex.Value.HowTo : ex.Value.Description;
                    descByName[name] = FirstSentenceOrTrim(d, 240);
                }
                else
                {
                    descByName[name] = "No description available.";
                }
            }
            catch
            {
                descByName[name] = "No description available.";
            }
        }

        // Build per-session output
        var header = $"Exercises in past 24h for user {userId} ({windowStart:yyyy-MM-dd HH:mm} -> {now:yyyy-MM-dd HH:mm}): {exerciseNames.Count} unique exercises across {sessions.Count} session(s).";
        var orderedSessions = sessions
            .OrderBy(s => s.CreatedAt ?? DateTime.MinValue)
            .ThenBy(s => s.Id)
            .ToList();

        string FormatExercise(string n) => $"{n} — {descByName.GetValueOrDefault(n, "No description available.")}";

        var lines = new List<string>();
        for (int i = 0; i < orderedSessions.Count; i++)
        {
            var s = orderedSessions[i];
            var created = s.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "n/a";
            var namesInSession = s.ExerciseRecords
                .Select(r => r.ExerciseName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList();
            var exercisesText = namesInSession.Count == 0 ? "None" : string.Join("; ", namesInSession.Select(FormatExercise));
            lines.Add($"Session {i + 1} (#{s.Id}, {created}): {exercisesText}");
        }

        return header + Environment.NewLine + string.Join(Environment.NewLine, lines);
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

        var lines = new List<string>();
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

    private static string FirstSentenceOrTrim(string? text, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(text)) return "No description available.";
        var t = text.Trim();
        int endIdx = t.IndexOfAny(new[] {'.', '!', '?' });
        string first = endIdx >= 0 ? t.Substring(0, endIdx + 1) : t;
        if (first.Length > maxLen) first = first.Substring(0, Math.Min(maxLen, first.Length)).TrimEnd() + "…";
        return first;
    }
}