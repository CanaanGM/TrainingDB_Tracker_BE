using API.Filters;
using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
public class GenericController : ControllerBase
{
    private readonly ILogger<GenericController> _logger;
    private readonly IMuscleService _muscleService;
    private readonly ITrainingTypesService _trainingTypesService;
    private readonly IExerciseService _exerciseService;
    private readonly IEquipmentService _equipmentService;
    private readonly IPlanService _planService;

    public GenericController(
        ILogger<GenericController> logger
        , IMuscleService muscleService
        , ITrainingTypesService trainingTypesService
        , IExerciseService exerciseService
        , IEquipmentService equipmentService
        , IPlanService planService
    )
    {
        _logger = logger;
        _muscleService = muscleService;
        _trainingTypesService = trainingTypesService;
        _exerciseService = exerciseService;
        _equipmentService = equipmentService;
        _planService = planService;
    }

    [HttpGet("/muscles")]
    public async Task<IActionResult> GetMuscles(CancellationToken cancellationToken)
    {
        var exercisesResult = await _muscleService.GetAllAsync(cancellationToken);
        return Ok(exercisesResult.Value);
    }

    [HttpGet("/muscles/search/{searchTerm}")]
    public async Task<IActionResult> GetMuscles(string searchTerm, CancellationToken cancellationToken)
    {
        var exercisesResult = await _muscleService.SearchMuscleAsync(searchTerm, cancellationToken);
        return Ok(exercisesResult.Value);
    }

    [HttpGet("/muscles/{groupName}")]
    public async Task<IActionResult> GetMusclesByGroup(string groupName, CancellationToken cancellationToken)
    {
        return Ok(await _muscleService.GetAllByGroupAsync(groupName, cancellationToken));
    }

    [HttpPost("/muscles/bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] HashSet<MuscleWriteDto> newMuscles,
        CancellationToken cancellationToken)
    {
        Result<bool> res = await _muscleService.CreateBulkAsync(newMuscles, cancellationToken);

        return res.IsSuccess ? Ok() : BadRequest();
    }

    [HttpGet("/types")]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.GetAllAsync(cancellationToken));
    }

    [HttpPut("/types/{typeId}")]
    public async Task<IActionResult> GetTypes(int typeId, [FromBody] TrainingTypeWriteDto updatedType,
        CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.Update(typeId, updatedType, cancellationToken));
    }

    [HttpPost("/types/bulk")]
    public async Task<IActionResult> CreateTypesBulk([FromBody] HashSet<TrainingTypeWriteDto> newTypes,
        CancellationToken cancellationToken)
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
    public async Task<IActionResult> GetExercises([FromQuery] ExerciseQueryOptions queryOptions,
        CancellationToken cancellationToken)
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
    public async Task<IActionResult> CreateExerciseAsync([FromBody] ExerciseWriteDto newExercise,
        CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.CreateAsync(newExercise, cancellationToken));
    }

    [HttpPost("/exercise/bulk")]
    public async Task<IActionResult> CreateExercisesBulkAsync([FromBody] List<ExerciseWriteDto> newExercises,
        CancellationToken cancellationToken)
    {
        var result = await _exerciseService.CreateBulkAsync(newExercises, cancellationToken);
        if (result.IsSuccess)
            return Ok();
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("/exercise/bulk")]
    public async Task<IActionResult> DeleteBulkAsync([FromBody] List<string> exercisesNamesToDelete,
        CancellationToken cancellationToken)
    {
        var result = await _exerciseService.DeleteBulkAsync(exercisesNamesToDelete, cancellationToken);
        if (result.IsSuccess)
            return Ok();
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("/exercise/{id}")]
    public async Task<IActionResult> UpdateExerciseAsync(int id, [FromBody] ExerciseWriteDto updatedExercise,
        CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.UpdateAsync(id, updatedExercise, cancellationToken));
    }

    [HttpDelete("/exercise/{id}")]
    public async Task<IActionResult> DeleteExerciseAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.DeleteExerciseAsync(id, cancellationToken));
    }

    [HttpGet("/exercise/search/{exercise}")]
    public async Task<IActionResult> SearchExercises(string exercise, CancellationToken cancellationToken)
    {
        var result = await _exerciseService.SearchExercisesAsync(exercise, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Value);
    }


    [HttpGet("/equipment")]
    public async Task<IActionResult> GetEquipmentsAync(CancellationToken cancellationToken)
    {
        var equipments = await _equipmentService.GetAsync(cancellationToken);
        return Ok(equipments.Value);
    }

    [HttpPost("/equipment")]
    // [ServiceFilter(typeof(AuthenticatedUserFilter))]
    public async Task<IActionResult> UpsertEquipmentAsync([FromBody] EquipmentWriteDto newEquipmentWriteDto,
        CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.UpsertAsync(newEquipmentWriteDto, cancellationToken));
    }

    [HttpPost("/equipment/bulk")]
    public async Task<IActionResult> CreateEquipmentBulkAsync([FromBody] List<EquipmentWriteDto> newEquipmentWriteDtos,
        CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.CreateBulkAsync(newEquipmentWriteDtos, cancellationToken));
    }


    [HttpDelete("/equipment/{equipmentName}")]
    public async Task<IActionResult> DeleteEquipmentAsync(string equipmentName, CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.DeleteAsync(equipmentName, cancellationToken));
    }

    [HttpPost("/plans")]
    public async Task<IActionResult> CreateTrainingPlanBulkAsync(
        [FromBody] TrainingPlanWriteDto newTrainingPlanWriteDto, CancellationToken cancellationToken)
    {
        return Ok(await _planService.CreateAsync(newTrainingPlanWriteDto, cancellationToken));
    }
}