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
public class LaboratoriesController : ControllerBase
{
    private readonly ILaboratoryService _service;
    public LaboratoriesController(ILaboratoryService service) => _service = service;

    [HttpGet]
    [ProducesResponseType<IEnumerable<LaboratoryDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<LaboratoryDto>>> GetAll()
        => Ok(await _service.AllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<LaboratoryDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<LaboratoryDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<LaboratoryDto>((int)HttpStatusCode.Created)]
    public async Task<ActionResult<LaboratoryDto>> Create([FromBody] LaboratoryCreateDto dto)
    {
        var created = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<LaboratoryDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<LaboratoryDto>> Update(Guid id, [FromBody] LaboratoryUpdateDto dto)
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
