using App.BLL.Contracts;
using App.DTO.v1;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("/api/v{version:apiVersion}/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _service;
    public EquipmentController(IEquipmentService service) => _service = service;

    [HttpGet]
    [ProducesResponseType<IEnumerable<EquipmentDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetAll()
        => Ok(await _service.AllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EquipmentDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<EquipmentDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet("available")]
    [ProducesResponseType<IEnumerable<EquipmentDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetAvailable()
        => Ok(await _service.GetAvailableAsync());

    [HttpGet("by-laboratory/{laboratoryId:guid}")]
    [ProducesResponseType<IEnumerable<EquipmentDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetByLaboratory(Guid laboratoryId)
        => Ok(await _service.GetByLaboratoryAsync(laboratoryId));

    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType<IEnumerable<EquipmentDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetByCategory(Guid categoryId)
        => Ok(await _service.GetByCategoryAsync(categoryId));

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<EquipmentDto>((int)HttpStatusCode.Created)]
    public async Task<ActionResult<EquipmentDto>> Create([FromBody] EquipmentCreateDto dto)
    {
        var created = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<EquipmentDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<EquipmentDto>> Update(Guid id, [FromBody] EquipmentUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();
        try { return Ok(await _service.UpdateAsync(dto)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
        => await _service.RemoveAsync(id) ? NoContent() : NotFound();
}