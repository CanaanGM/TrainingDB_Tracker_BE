using API.Common.Filters;
using API.Security;
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
    private readonly IUserAccessor _userAccessor;

    public MeasurementsController(IMeasurementsService measurementsService, IUserAccessor userAccessor)
    {
	    _measurementsService = measurementsService;
	    _userAccessor = userAccessor;
    }
    
    [HttpGet()]
    public async Task<IActionResult> GetMeasurementsAsync( CancellationToken cancellationToken)
    {
	    var userId = GetUserId();
        var measurements = await _measurementsService.GetAll(userId, cancellationToken) ;

        return measurements.IsSuccess
            ? Ok(measurements.Value)
            : BadRequest();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeasurementByIdAsync(int id, CancellationToken cancellationToken)
    {
	    var measurement = await _measurementsService.GetByIdAsync(GetUserId(), id, cancellationToken);
	    return measurement.IsSuccess
		    ? Ok(measurement.Value)
		    : BadRequest();
    }


    [HttpPost()]
    public async Task<IActionResult> CreateMeasurementsAsync( [FromBody] MeasurementsWriteDto newMeasurementsWriteDto, CancellationToken cancellationToken)
    {
	    var userId = GetUserId();
        var result = await _measurementsService.CreateAsync(userId, newMeasurementsWriteDto, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(CreateMeasurementsAsync), new { id = result.Value }, result.Value)  : BadRequest();
    }
    
    [HttpPut("/{measurementId}")]
    public async Task<IActionResult> UpdateMeasurementsAsync( int measurementId, MeasurementsWriteDto updateMeasurementsDto, CancellationToken cancellationToken)
    {
	    var userId = GetUserId();
        var result =
            await _measurementsService.UpdateAsync(userId, measurementId, updateMeasurementsDto, cancellationToken);
        
        return result.IsSuccess ?  NoContent() : BadRequest();
    }

    [HttpDelete("/{measurementId}")]
    public async Task<IActionResult> DeleteMeasurementsAsync( int measurementId, CancellationToken cancellationToken)
    {
	    var userId = GetUserId();
        var result = await _measurementsService.DeleteAsync(userId, measurementId, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest();
    }
    
    private int GetUserId()
    {
	    var userIdResult =  _userAccessor.GetUserId();
	    if (!userIdResult.IsSuccess)
		    throw new UnauthorizedAccessException();
	    return userIdResult.Value;
    }
}