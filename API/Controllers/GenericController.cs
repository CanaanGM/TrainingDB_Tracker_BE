using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
public class GenericController : ControllerBase
{

    private readonly ILogger<GenericController> _logger;
    private readonly IMuscleService _muscleService;
    private readonly ITrainingTypesService _trainingTypesService;
    private readonly IExerciseService _exerciseService;
    private readonly ITrainingSessionService _trainingSessionService;

    public GenericController(
        ILogger<GenericController> logger
        , IMuscleService muscleService
        , ITrainingTypesService trainingTypesService
        , IExerciseService exerciseService
        , ITrainingSessionService trainingSessionService
        )
    {
        _logger = logger;
        _muscleService = muscleService;
        _trainingTypesService = trainingTypesService;
        _exerciseService = exerciseService;
        _trainingSessionService = trainingSessionService;
    }

    [HttpGet("/muscles")]
    public async Task<IActionResult> GetMuscles(CancellationToken cancellationToken)
    {
        return Ok(await _muscleService.GetAllAsync(cancellationToken));
    }

    [HttpGet("/muscles/{groupName}")]
    public async Task<IActionResult> GetMusclesByGroup(string groupName, CancellationToken cancellationToken)
    {
        return Ok(await _muscleService.GetAllByGroupAsync(groupName, cancellationToken));
    }

    [HttpPost("/muscles/bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] HashSet<MuscleWriteDto> newMuscles, CancellationToken cancellationToken)
    {
        DataLibrary.Core.Result<bool> res = await _muscleService.CreateBulkAsync(newMuscles, cancellationToken);

        return res.IsSuccess ? Ok() : BadRequest();
    }

    [HttpGet("/types")]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.GetAllAsync(cancellationToken));
    }

    [HttpPut("/types/{typeId}")]
    public async Task<IActionResult> GetTypes(int typeId, [FromBody] TrainingTypeWriteDto updatedType, CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.Update(typeId, updatedType, cancellationToken));
    }

    [HttpPost("/types/bulk")]
    public async Task<IActionResult> CreateTypesBulk([FromBody] HashSet<TrainingTypeWriteDto> newTypes, CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.CreateBulkAsync(newTypes, cancellationToken));
    }

    /// <summary>
    /// Retrieves a paginated list of exercises with optional filters and sorting.
    /// </summary>
    /// <param name="queryOptions">The options for filtering, sorting, and pagination.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A paginated list of exercises.</returns>
    [HttpGet("/exercise")]
    public async Task<IActionResult> GetExercises([FromQuery] ExerciseQueryOptions queryOptions, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving exercises with options: {@QueryOptions}", queryOptions);
            var result = await _exerciseService.GetAllAsync(queryOptions, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve exercises: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            _logger.LogInformation("Successfully retrieved exercises.");
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving exercises.");
            return StatusCode(500, "An error occurred while retrieving exercises.");
        }
    }

    [HttpGet("/exercise/{name}")]
    public async Task<IActionResult> GetExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.GetByNameAsync(name, cancellationToken));
    }
    [HttpPost("/exercise")]
    public async Task<IActionResult> CreateExerciseAsync([FromBody] ExerciseWriteDto newExercise, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.CreateAsync(newExercise, cancellationToken));
    }


    [HttpPost("/exercise/bulk")]
    public async Task<IActionResult> CreateExercisesBulkAsync([FromBody] List<ExerciseWriteDto> newExercises, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.CreateBulkAsync(newExercises, cancellationToken));
    }
    [HttpPut("/exercise/{id}")]
    public async Task<IActionResult> UpdateExerciseAsync(int id, [FromBody] ExerciseWriteDto updatedExercise, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.UpdateAsync(id, updatedExercise, cancellationToken));
    }
    [HttpDelete("/exercise/{id}")]
    public async Task<IActionResult> DeleteExerciseAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.DeleteExerciseAsync(id, cancellationToken));
    }

    [HttpGet("/training")]
    public async Task<IActionResult> GetTrainingSessionsAsync(CancellationToken cancellationToken, string? startDate, string? endDate)
    {
        Result<List<TrainingSessionReadDto>> sessions = await _trainingSessionService.GetTrainingSessionsAsync(startDate, endDate, cancellationToken);
        return Ok(sessions.Value);
    }

    [HttpPost("/training")]
    public async Task<IActionResult> CreateTrainingSessionAsync([FromBody] TrainingSessionWriteDto newTrainingSessionDto, CancellationToken cancellationToken)
    {
        return Ok(await _trainingSessionService.CreateSessionAsync(newTrainingSessionDto, cancellationToken));
    }

    [HttpPut("/training/{sessionId}")]
    public async Task<IActionResult> UpdateTrainingSessionAsync(int sessionId, TrainingSessionWriteDto updatedSessionDto, CancellationToken cancellationToken)
    {
        return Ok(await _trainingSessionService.UpdateSessionAsync(sessionId, updatedSessionDto, cancellationToken));
    }

    [HttpDelete("/training/{sessionId}")]
    public async Task<IActionResult> DeleteTrainingSessionAsyn(int sessionId, CancellationToken cancellationToken)
    {
        return Ok(await _trainingSessionService.DeleteSessionAsync(sessionId, cancellationToken));
    }


}
