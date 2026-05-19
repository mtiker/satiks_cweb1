using System.Security.Claims;
using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models.Booking;

namespace WebApp.Areas.User.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("User")]
[Authorize]
public class BookingController(IBookingService bookingService, IEquipmentService equipmentService) : Controller
{
    private readonly IBookingService _bookingService = bookingService;
    private readonly IEquipmentService _equipmentService = equipmentService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingService.AllAsync(CurrentUserId);
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? equipmentId)
    {
        var equipment = (await _equipmentService.AllAsync())
            .Where(e => e.IsAvailableForBooking)
            .ToList();

        var vm = new BookingCreateViewModel
        {
            EquipmentSelectList = new SelectList(equipment, "Id", "Name", equipmentId),
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
        };
        if (equipmentId.HasValue)
            vm.EquipmentId = equipmentId.Value;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var equipment = (await _equipmentService.AllAsync())
                .Where(e => e.IsAvailableForBooking).ToList();
            vm.EquipmentSelectList = new SelectList(equipment, "Id", "Name", vm.EquipmentId);
            return View(vm);
        }

        try
        {
            var dto = new BookingCreateDto
            {
                EquipmentId = vm.EquipmentId,
                StartTime = vm.StartTime.ToUniversalTime(),
                EndTime = vm.EndTime.ToUniversalTime(),
                Purpose = vm.Purpose,
            };
            await _bookingService.AddAsync(dto, CurrentUserId);
            TempData["Success"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var equipment = (await _equipmentService.AllAsync())
                .Where(e => e.IsAvailableForBooking).ToList();
            vm.EquipmentSelectList = new SelectList(equipment, "Id", "Name", vm.EquipmentId);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _bookingService.CancelAsync(id, CurrentUserId);
            TempData["Success"] = "Booking cancelled.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        return RedirectToAction(nameof(Index));
    }
}
