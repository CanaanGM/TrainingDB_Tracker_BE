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
    /// Gets a paginated list of all exercises in the database with their related muscles.
    /// </summary>
    /// <param name="asc">Ascending or decending option</param>
    /// <param name="d">Difficulty, from 1 ~ 10</param>
    /// <param name="g">Muscle Group to filter the results by</param>
    /// <param name="m">Muscle to filter the results by</param>
    /// <param name="pageNumber">PageNumber</param>
    /// <param name="pageSize">Page Size</param>
    /// <param name="sort">SortBy: NAME, DIFFICULTY, MUSCLE_GROUP, TRAINING_TYPE</param>
    /// <param name="cancellationToken"></param>
    /// <returns>a paginated list of all exercises in the database with their related muscles with applied sorting and filtering.</returns>
    [HttpGet("/exercise")]
    public async Task<IActionResult> GetExercisesAsync(
       [FromQuery] bool asc = true,
       [FromQuery] int d = 1,
       [FromQuery] string g = "",
       [FromQuery] string m = "",
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] string sort = "NAME",
       CancellationToken cancellationToken = default)
    {
        ExerciseQueryOptions options = new ExerciseQueryOptions
        {
            Ascending = asc,
            MinimumDifficulty = d,
            MuscleGroupName = g,
            MuscleName = m,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = Enum.TryParse(sort, true, out SortBy sortByResult) ? sortByResult : SortBy.NAME,
            TrainingTypeName = "" // Add a query parameter if needed to handle this
        };

        Result<PaginatedList<ExerciseReadDto>> result = await _exerciseService.GetAsync(options, cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.ErrorMessage);
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
        return Ok(await _trainingSessionService.GetTrainingSessionsAsync(startDate, endDate, cancellationToken));
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
