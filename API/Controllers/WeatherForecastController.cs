using DataLibrary.Dtos;
using DataLibrary.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{


    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IPlanService _planService;
    private readonly IMuscleService _muscleService;
    private readonly IExerciseService _exerciseService;
    private readonly ITrainingTypesService _trainingTypesService;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger
        , IPlanService planService
        , IMuscleService muscleService
        , IExerciseService exerciseService
        , ITrainingTypesService trainingTypesService


        )
    {
        _logger = logger;
        _planService = planService;
        _muscleService = muscleService;
        _exerciseService = exerciseService;
        _trainingTypesService = trainingTypesService;
    }

    [HttpGet("/plans")]
    public IActionResult GetPlans()
    {
        return Ok(_planService.Get());
    }

    [HttpGet("/muscles")]
    public IActionResult GetMuscles()
    {
        return Ok(_muscleService.Get());
    }

    [HttpGet("/exercises")]
    public IActionResult GetExercises()
    {
        return Ok(_exerciseService.Get());
    }

    [HttpGet("/trainingTypes")]
    public IActionResult GetTypes()
    {
        return Ok(_trainingTypesService.Get());
    }

    [HttpPost("/trainingTypes/bulk")]
    public async Task<IActionResult> CreateTypeInBulkAsync(
        [FromBody] List<TypeWriteDto> trainingTypes, CancellationToken cancellationToken)
    {
        try
        {
            await _trainingTypesService.InsertBulkAsync(trainingTypes, cancellationToken);
        }
        catch (Exception)
        {
            return Ok("Not Ok");
        }
        // this should gimme a result of either Success<Value> or Failure<Excepion>
        return Ok("Ok");
    }


}
