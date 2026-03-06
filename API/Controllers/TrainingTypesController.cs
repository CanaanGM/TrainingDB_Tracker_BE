using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("types")]
public class TrainingTypesController : ControllerBase
{
    private readonly ITrainingTypesService _trainingTypesService;

    public TrainingTypesController(ITrainingTypesService trainingTypesService)
    {
        _trainingTypesService = trainingTypesService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.GetAllAsync(cancellationToken));
    }

    [HttpPut("{typeId}")]
    public async Task<IActionResult> UpdateType(int typeId, [FromBody] TrainingTypeWriteDto updatedType,
        CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.Update(typeId, updatedType, cancellationToken));
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateTypesBulk([FromBody] HashSet<TrainingTypeWriteDto> newTypes,
        CancellationToken cancellationToken)
    {
        return Ok(await _trainingTypesService.CreateBulkAsync(newTypes, cancellationToken));
    }
}
