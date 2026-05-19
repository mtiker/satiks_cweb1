using App.BLL.Contracts;
using App.DTO.v1;
using Asp.Versioning;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("/api/v{version:apiVersion}/[controller]")]
public class CalibrationRecordsController : ControllerBase
{
    private readonly ICalibrationRecordService _service;
    public CalibrationRecordsController(ICalibrationRecordService service) => _service = service;

    // IDOR: authenticated user sees only their own calibration records
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<IEnumerable<CalibrationRecordDto>>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<IEnumerable<CalibrationRecordDto>>> GetAll()
        => Ok(await _service.AllAsync(IdentityHelpers.GetUserId(User)));

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<CalibrationRecordDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CalibrationRecordDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id, IdentityHelpers.GetUserId(User));
        return dto == null ? NotFound() : Ok(dto);
    }

    // Admin: see all calibration records
    [HttpGet("admin/all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType<IEnumerable<CalibrationRecordDto>>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<IEnumerable<CalibrationRecordDto>>> GetAllAdmin()
        => Ok(await _service.AllAsync());

    [HttpGet("overdue")]
    [AllowAnonymous]
    [ProducesResponseType<IEnumerable<CalibrationRecordDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CalibrationRecordDto>>> GetOverdue()
        => Ok(await _service.GetOverdueAsync());

    [HttpGet("by-equipment/{equipmentId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<IEnumerable<CalibrationRecordDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CalibrationRecordDto>>> GetByEquipment(Guid equipmentId)
        => Ok(await _service.GetByEquipmentAsync(equipmentId));

    [HttpGet("by-equipment/{equipmentId:guid}/latest")]
    [AllowAnonymous]
    [ProducesResponseType<CalibrationRecordDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CalibrationRecordDto>> GetLatestByEquipment(Guid equipmentId)
    {
        var dto = await _service.GetLatestByEquipmentAsync(equipmentId);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "technician,admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<CalibrationRecordDto>((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<CalibrationRecordDto>> Create([FromBody] CalibrationRecordCreateDto dto)
    {
        var created = await _service.AddAsync(dto, IdentityHelpers.GetUserId(User));
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "technician,admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<CalibrationRecordDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CalibrationRecordDto>> Update(Guid id, [FromBody] CalibrationRecordUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();
        try { return Ok(await _service.UpdateAsync(dto, IdentityHelpers.GetUserId(User))); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
        => await _service.RemoveAsync(id, IdentityHelpers.GetUserId(User)) ? NoContent() : NotFound();
}
