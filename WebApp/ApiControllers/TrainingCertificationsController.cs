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
public class TrainingCertificationsController : ControllerBase
{
    private readonly ITrainingCertificationService _service;
    public TrainingCertificationsController(ITrainingCertificationService service) => _service = service;

    // IDOR: authenticated users see only their own
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<IEnumerable<TrainingCertificationDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<TrainingCertificationDto>>> GetAll()
        => Ok(await _service.AllAsync(IdentityHelpers.GetUserId(User)));

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<TrainingCertificationDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<TrainingCertificationDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id, IdentityHelpers.GetUserId(User));
        return dto == null ? NotFound() : Ok(dto);
    }

    // Admin GETs
    [HttpGet("admin/all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType<IEnumerable<TrainingCertificationDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<TrainingCertificationDto>>> GetAllAdmin()
        => Ok(await _service.AllAsync());

    [HttpGet("admin/by-status/{status}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType<IEnumerable<TrainingCertificationDto>>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<IEnumerable<TrainingCertificationDto>>> GetByStatus(string status)
    {
        try { return Ok(await _service.GetByStatusAsync(status)); }
        catch (ArgumentException ex)
        {
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }

    // User write endpoints (authenticated)
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<TrainingCertificationDto>((int)HttpStatusCode.Created)]
    [ProducesResponseType<RestApiErrorResponse>((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<TrainingCertificationDto>> Create([FromBody] TrainingCertificationCreateDto dto)
    {
        try
        {
            var created = await _service.AddAsync(dto, IdentityHelpers.GetUserId(User));
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<TrainingCertificationDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<TrainingCertificationDto>> Update(Guid id, [FromBody] TrainingCertificationUserUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();
        try { return Ok(await _service.UpdateAsync(dto, IdentityHelpers.GetUserId(User))); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
        => await _service.RemoveAsync(id, IdentityHelpers.GetUserId(User)) ? NoContent() : NotFound();

    // Admin write: approval workflow
    [HttpPut("{id:guid}/status")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<TrainingCertificationDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<TrainingCertificationDto>> UpdateStatus(Guid id, [FromBody] TrainingCertificationAdminUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();
        try
        {
            return Ok(await _service.UpdateStatusAsync(dto, IdentityHelpers.GetUserId(User)));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException ex)
        {
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }
}
