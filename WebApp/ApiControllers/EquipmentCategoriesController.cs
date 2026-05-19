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
public class EquipmentCategoriesController : ControllerBase
{
    private readonly IEquipmentCategoryService _service;
    public EquipmentCategoriesController(IEquipmentCategoryService service) => _service = service;

    [HttpGet]
    [ProducesResponseType<IEnumerable<EquipmentCategoryDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentCategoryDto>>> GetAll()
        => Ok(await _service.AllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EquipmentCategoryDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<EquipmentCategoryDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet("requiring-training")]
    [ProducesResponseType<IEnumerable<EquipmentCategoryDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<EquipmentCategoryDto>>> GetRequiringTraining()
        => Ok(await _service.GetRequiringTrainingAsync());

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<EquipmentCategoryDto>((int)HttpStatusCode.Created)]
    public async Task<ActionResult<EquipmentCategoryDto>> Create([FromBody] EquipmentCategoryCreateDto dto)
    {
        var created = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<EquipmentCategoryDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<EquipmentCategoryDto>> Update(Guid id, [FromBody] EquipmentCategoryUpdateDto dto)
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
