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
public class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _service;
    public ManufacturersController(IManufacturerService service) => _service = service;

    [HttpGet]
    [ProducesResponseType<IEnumerable<ManufacturerDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<ManufacturerDto>>> GetAll()
        => Ok(await _service.AllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ManufacturerDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ManufacturerDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<ManufacturerDto>((int)HttpStatusCode.Created)]
    public async Task<ActionResult<ManufacturerDto>> Create([FromBody] ManufacturerCreateDto dto)
    {
        var created = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<ManufacturerDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ManufacturerDto>> Update(Guid id, [FromBody] ManufacturerUpdateDto dto)
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
