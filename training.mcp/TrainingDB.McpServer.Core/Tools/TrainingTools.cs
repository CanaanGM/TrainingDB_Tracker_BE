using System;
using ModelContextProtocol.Server;

namespace TrainingDB.McpServer.Core.Tools;
[McpServerToolType]
public class TrainingTools
{
    [McpServerTool(Name = "ping")]
    public Task<string> PingAsync() => Task.FromResult("pong");
}
