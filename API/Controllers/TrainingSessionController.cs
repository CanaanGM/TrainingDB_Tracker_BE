using API.Common.Filters;
using API.Security;
using DataLibrary.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("/training")]
[ServiceFilter(typeof(AuthenticatedUserFilter))]
public class TrainingSessionController : ControllerBase
{
    private readonly ITrainingSessionService _trainingSessionService;
    
    public TrainingSessionController(
        ITrainingSessionService trainingSessionService)
    {
        _trainingSessionService = trainingSessionService;
    }
    
    [HttpGet()]
    public async Task<IActionResult> GetTrainingSessionsAsync(int userId, CancellationToken cancellationToken, string? startDate, string? endDate)
    {
        
        var sessions = await _trainingSessionService.GetPaginatedTrainingSessionsAsync(
            userId, 1, 10, cancellationToken);
        return Ok(sessions.Value);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateTrainingSessionAsync(int userId,[FromBody] TrainingSessionWriteDto newTrainingSessionDto, CancellationToken cancellationToken)
    {
        var res = await _trainingSessionService.CreateSessionAsync(userId, newTrainingSessionDto, cancellationToken);
        return res.IsSuccess ?  Created() : BadRequest();
    }

    [HttpPost("/bulk")]
    public async Task<IActionResult> CreateTrainingSessionBulkAsync(int userId,[FromBody] List<TrainingSessionWriteDto> newTrainingSessionDtos, CancellationToken cancellationToken)
    {
        var res = await _trainingSessionService.CreateSessionsBulkAsync(userId, newTrainingSessionDtos,
            cancellationToken);
        return res.IsSuccess ? Created() : BadRequest();
    }
    
    [HttpPut("/{sessionId}")]
    public async Task<IActionResult> UpdateTrainingSessionAsync(int userId,int sessionId, TrainingSessionWriteDto updatedSessionDto, CancellationToken cancellationToken)
    {
        var res = await _trainingSessionService.UpdateTrainingSession(userId, sessionId, updatedSessionDto,
            cancellationToken);
        return res.IsSuccess ? NoContent() : BadRequest();
    }

    [HttpDelete("/{sessionId}")]
    public async Task<IActionResult> DeleteTrainingSessionAsync(int userId,int sessionId, CancellationToken cancellationToken)
    {
        var res = await _trainingSessionService.DeleteTrainingSessionAsync(userId, sessionId, cancellationToken);
        return res.IsSuccess ? NoContent() : BadRequest();
    }

}