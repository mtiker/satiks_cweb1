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
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) => _service = service;

    // IDOR: authenticated user sees only their own bookings
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<IEnumerable<BookingDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
        => Ok(await _service.AllAsync(IdentityHelpers.GetUserId(User)));

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<BookingDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<BookingDto>> Get(Guid id)
    {
        var dto = await _service.FindAsync(id, IdentityHelpers.GetUserId(User));
        return dto == null ? NotFound() : Ok(dto);
    }

    // Admin: see all bookings
    [HttpGet("admin/all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType<IEnumerable<BookingDto>>((int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllAdmin()
        => Ok(await _service.AllAsync());

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<BookingDto>((int)HttpStatusCode.Created)]
    [ProducesResponseType<RestApiErrorResponse>((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BookingDto>> Create([FromBody] BookingCreateDto dto)
    {
        try
        {
            var created = await _service.AddAsync(dto, IdentityHelpers.GetUserId(User));
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new RestApiErrorResponse
                { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new RestApiErrorResponse
                { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Consumes("application/json")][Produces("application/json")]
    [ProducesResponseType<BookingDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<BookingDto>> Update(Guid id, [FromBody] BookingUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest();
        try { return Ok(await _service.UpdateAsync(dto, IdentityHelpers.GetUserId(User))); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return NotFound(); } // mask ownership leak
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
        => await _service.RemoveAsync(id, IdentityHelpers.GetUserId(User)) ? NoContent() : NotFound();

    // Admin workflow actions
    [HttpPost("{id:guid}/confirm")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType<BookingDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<BookingDto>> Confirm(Guid id)
    {
        try { return Ok(await _service.ConfirmAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<BookingDto>((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<BookingDto>> Cancel(Guid id)
    {
        // Admin can cancel any booking (no userId filter); user can only cancel their own
        var isAdmin = User.IsInRole("admin");
        Guid? userId = isAdmin ? null : IdentityHelpers.GetUserId(User);
        try { return Ok(await _service.CancelAsync(id, userId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return NotFound(); }
    }
}