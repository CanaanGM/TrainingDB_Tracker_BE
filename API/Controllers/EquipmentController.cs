using DataLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("equipment")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetEquipmentsAync(CancellationToken cancellationToken)
    {
        var equipments = await _equipmentService.GetAsync(cancellationToken);
        return Ok(equipments.Value);
    }

    [HttpPost("")]
    // [ServiceFilter(typeof(AuthenticatedUserFilter))]
    public async Task<IActionResult> UpsertEquipmentAsync([FromBody] EquipmentWriteDto newEquipmentWriteDto,
        CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.UpsertAsync(newEquipmentWriteDto, cancellationToken));
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateEquipmentBulkAsync([FromBody] List<EquipmentWriteDto> newEquipmentWriteDtos,
        CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.CreateBulkAsync(newEquipmentWriteDtos, cancellationToken));
    }

    [HttpDelete("{equipmentName}")]
    public async Task<IActionResult> DeleteEquipmentAsync(string equipmentName, CancellationToken cancellationToken)
    {
        return Ok(await _equipmentService.DeleteAsync(equipmentName, cancellationToken));
    }
}
