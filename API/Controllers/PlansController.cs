using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("plans")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateTrainingPlanBulkAsync(
        [FromBody] TrainingPlanWriteDto newTrainingPlanWriteDto, CancellationToken cancellationToken)
    {
        return Ok(await _planService.CreateAsync(newTrainingPlanWriteDto, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlanByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        return Ok(await _planService.GetByIdAsync(id, cancellationToken));
    }
}
