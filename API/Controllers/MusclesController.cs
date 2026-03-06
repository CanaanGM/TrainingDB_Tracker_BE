using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("muscles")]
public class MusclesController : ControllerBase
{
    private readonly IMuscleService _muscleService;

    public MusclesController(IMuscleService muscleService)
    {
        _muscleService = muscleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetMuscles(CancellationToken cancellationToken)
    {
        var exercisesResult = await _muscleService.GetAllAsync(cancellationToken);
        return Ok(exercisesResult.Value);
    }

    [HttpGet("search/{searchTerm}")]
    public async Task<IActionResult> GetMuscles(string searchTerm, CancellationToken cancellationToken)
    {
        var exercisesResult = await _muscleService.SearchMuscleAsync(searchTerm, cancellationToken);
        return Ok(exercisesResult.Value);
    }

    [HttpGet("{groupName}")]
    public async Task<IActionResult> GetMusclesByGroup(string groupName, CancellationToken cancellationToken)
    {
        return Ok(await _muscleService.GetAllByGroupAsync(groupName, cancellationToken));
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] HashSet<MuscleWriteDto> newMuscles,
        CancellationToken cancellationToken)
    {
        Result<bool> res = await _muscleService.CreateBulkAsync(newMuscles, cancellationToken);
        return res.IsSuccess ? Ok() : BadRequest();
    }
}
