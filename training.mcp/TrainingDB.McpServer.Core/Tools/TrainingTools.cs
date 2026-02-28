using System;
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
    [Description("Get the user's training session data for the last day (24 hours)")]
    public async Task<ICollection<TrainingSessionReadDto>> GetRecentTrainingSessions24hAsync(
        [Description("the user id to get the sessions for, default is 1 (canaan)")]
        int userId = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await trainingSessionService.GetTrainingSessionsPast24HoursAsync(userId, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Could not retrieve recent sessions");
        }
        return result!.Value!;
    }
}
