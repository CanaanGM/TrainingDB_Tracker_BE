using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;
using SharedLibrary.Core;

namespace API.Controllers;

[ApiController]
[Route("exercise")]
public class ExercisesController : ControllerBase
{
    private readonly ILogger<ExercisesController> _logger;
    private readonly IExerciseService _exerciseService;

    public ExercisesController(ILogger<ExercisesController> logger, IExerciseService exerciseService)
    {
        _logger = logger;
        _exerciseService = exerciseService;
    }

    /// <summary>
    /// Retrieves a paginated list of exercises with optional filters and sorting.
    /// </summary>
    [HttpGet("")]
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

    [HttpGet("{name}")]
    public async Task<IActionResult> GetExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.GetByNameAsync(name, cancellationToken));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateExerciseAsync([FromBody] ExerciseWriteDto newExercise,
        CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.CreateAsync(newExercise, cancellationToken));
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateExercisesBulkAsync([FromBody] List<ExerciseWriteDto> newExercises,
        CancellationToken cancellationToken)
    {
        var result = await _exerciseService.CreateBulkAsync(newExercises, cancellationToken);
        if (result.IsSuccess)
            return Ok();
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("bulk")]
    public async Task<IActionResult> DeleteBulkAsync([FromBody] List<string> exercisesNamesToDelete,
        CancellationToken cancellationToken)
    {
        var result = await _exerciseService.DeleteBulkAsync(exercisesNamesToDelete, cancellationToken);
        if (result.IsSuccess)
            return Ok();
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExerciseAsync(int id, [FromBody] ExerciseWriteDto updatedExercise,
        CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.UpdateAsync(id, updatedExercise, cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExerciseAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.DeleteExerciseAsync(id, cancellationToken));
    }

    [HttpGet("search/{exercise}")]
    public async Task<IActionResult> SearchExercises(string exercise, CancellationToken cancellationToken)
    {
        var result = await _exerciseService.SearchExercisesAsync(exercise, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Value);
    }

    [HttpGet("csv")]
    public async Task<IActionResult> ExportExerciseCSVFile(CancellationToken cancellationToken)
    {
        var result = await _exerciseService.ExportCsvAsync(cancellationToken);
        return File(result, "application/octet-stream", "exercise.csv");
    }
}

