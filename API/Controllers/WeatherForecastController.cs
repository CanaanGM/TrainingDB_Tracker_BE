using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IMuscleService _muscleService;
    private readonly ITrainingTypesService _trainingTypesService;
    private readonly IExerciseService _exerciseService;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger
        , IMuscleService muscleService
        , ITrainingTypesService trainingTypesService
        , IExerciseService exerciseService
        )
    {
        _logger = logger;
        _muscleService = muscleService;
        _trainingTypesService = trainingTypesService;
        _exerciseService = exerciseService;
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


    [HttpGet("/exercise")]
    public async Task<IActionResult> GetExercisesAsync(
        [FromQuery] bool s = true
        , [FromQuery] int d = 1
        , [FromQuery] string g = ""
        , [FromQuery] string m = ""
        , [FromQuery] int pageNumber=1
        , [FromQuery] int pageSize=2
        , [FromQuery] string sort = ""
        , CancellationToken cancellationToken= default)
    {
        var options = new ExerciseQueryOptions
        {
            Ascending = true,
            MinimumDifficulty = 1,
            MuscleGroupName = "",
            MuscleName = "",
            PageNumber = 1,
            PageSize = 10,
            SortBy = "",
            TrainingTypeName = ""
        };
        return Ok(await _exerciseService.GetAsync(options, cancellationToken));
    }
    [HttpGet("/exercise/{name}")]
    public async Task<IActionResult> GetExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        return Ok(await _exerciseService.GetByNameAsync(name, cancellationToken));
    }
    [HttpPost("/exercise")]
    public async Task<IActionResult> CreateExerciseAsync([FromBody] ExerciseWriteDto newExercise, CancellationToken cancellationToken)
    {
        return Ok( await _exerciseService.CreateAsync(newExercise, cancellationToken));
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








}
