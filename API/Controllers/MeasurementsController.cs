using API.Filters;
using DataLibrary.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("/measurements")]
[ServiceFilter(typeof(AuthenticatedUserFilter))]
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementsService _measurementsService;

    public MeasurementsController(IMeasurementsService measurementsService)
    {
        _measurementsService = measurementsService;
    }
    
    [HttpGet()]
    public async Task<IActionResult> GetMeasurementsAsync(int userId, CancellationToken cancellationToken)
    {
        var measurements = await _measurementsService.GetAll(userId, cancellationToken) ;

        return measurements.IsSuccess
            ? Ok(measurements.Value)
            : BadRequest();
    }

    [HttpPost()]
    public async Task<IActionResult> CreateMeasurementsAsync(int userId, [FromBody] MeasurementsWriteDto newMeasurementsWriteDto, CancellationToken cancellationToken)
    {
        var result = await _measurementsService.CreateAsync(userId, newMeasurementsWriteDto, cancellationToken);
        return result.IsSuccess ? CreatedAtRoute(nameof(CreateMeasurementsAsync),  new {result.Value}) : BadRequest();
    }
    
    [HttpPut("/{measurementId}")]
    public async Task<IActionResult> UpdateMeasurementsAsync(int userId, int measurementId, MeasurementsWriteDto updateMeasurementsDto, CancellationToken cancellationToken)
    {
        var result =
            await _measurementsService.UpdateAsync(userId, measurementId, updateMeasurementsDto, cancellationToken);
        
        return result.IsSuccess ?  NoContent() : BadRequest();
    }

    [HttpDelete("/{measurementId}")]
    public async Task<IActionResult> DeleteMeasurementsAsync(int userId, int measurementId, CancellationToken cancellationToken)
    {
        var result = await _measurementsService.DeleteAsync(userId, measurementId, cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest();
    }
}