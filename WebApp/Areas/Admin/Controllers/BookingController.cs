using App.BLL.Contracts;
using App.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.Booking;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class BookingController(
    IBookingService bookingService,
    IEquipmentService equipmentService,
    UserManager<AppUser> userManager) : Controller
{
    private readonly IBookingService _bookingService = bookingService;
    private readonly IEquipmentService _equipmentService = equipmentService;
    private readonly UserManager<AppUser> _userManager = userManager;
    public async Task<IActionResult> Index(
        string? search,
        string? status,
        string? userId,
        string? equipmentId,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _bookingService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(b =>
                (b.EquipmentName != null && b.EquipmentName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.UserFullName != null && b.UserFullName.Contains(search, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(status))
            all = all.Where(b => b.BookingStatus == status);



        if (!string.IsNullOrWhiteSpace(userId))
            all = all.Where(b => b.AppUserId.ToString() == userId);

        if (!string.IsNullOrWhiteSpace(equipmentId))
            all = all.Where(b => b.EquipmentId.ToString() == equipmentId);

        var ordered = all.OrderByDescending(b => b.StartTime).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookingListItem
            {
                Id            = b.Id,
                BookingStatus = b.BookingStatus,
                StartTime     = b.StartTime,
                EndTime       = b.EndTime,
                Purpose       = b.Purpose,
                EquipmentId   = b.EquipmentId,
                EquipmentName = b.EquipmentName ?? string.Empty,
                AppUserId     = b.AppUserId,
                UserFullName  = b.UserFullName ?? string.Empty,
            })
            .ToList();

        var equipmentOptions = (await _equipmentService.AllAsync())
            .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            .ToList();

        var userOptions = _userManager.Users
            .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = u.Email })
            .ToList();

        var vm = new BookingIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = BookingIndexViewModel.BuildFilters(
                BookingStatusOptions.AsSelectList(), userOptions, equipmentOptions, status, userId, equipmentId, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _bookingService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id)
    {
        try { await _bookingService.ConfirmAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Booking confirmed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try { await _bookingService.CancelAsync(id, appUserId: null); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Booking cancelled.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _bookingService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _bookingService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Booking deleted.";
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new BookingCreateEditViewModel
        {
            EquipmentSelectList = new SelectList(
                await _equipmentService.AllAsync(), "Id", "Name"),
            UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email"),
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.EquipmentSelectList = new SelectList(
                await _equipmentService.AllAsync(), "Id", "Name", vm.EquipmentId);
            vm.UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", vm.AppUserId);
            return View(vm);
        }

        try
        {
            var dto = new App.DTO.v1.BookingCreateDto
            {
                EquipmentId = vm.EquipmentId,
                StartTime = vm.StartTime.ToUniversalTime(),
                EndTime = vm.EndTime.ToUniversalTime(),
                Purpose = vm.Purpose,
            };
            await _bookingService.AddAsync(dto, vm.AppUserId);
            TempData["Success"] = "Booking created.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            vm.EquipmentSelectList = new SelectList(
                await _equipmentService.AllAsync(), "Id", "Name", vm.EquipmentId);
            vm.UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", vm.AppUserId);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _bookingService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new BookingCreateEditViewModel
        {
            Id = dto.Id,
            EquipmentId = dto.EquipmentId,
            AppUserId = dto.AppUserId,
            StartTime = dto.StartTime.ToLocalTime(),
            EndTime = dto.EndTime.ToLocalTime(),
            Purpose = dto.Purpose,
            BookingStatus = dto.BookingStatus,
            EquipmentSelectList = new SelectList(
                await _equipmentService.AllAsync(), "Id", "Name", dto.EquipmentId),
            UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", dto.AppUserId),
            StatusSelectList = new SelectList(
                new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, dto.BookingStatus),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookingCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.EquipmentSelectList = new SelectList(
                await _equipmentService.AllAsync(), "Id", "Name", vm.EquipmentId);
            vm.UserSelectList = new SelectList(
                _userManager.Users.ToList(), "Id", "Email", vm.AppUserId);
            vm.StatusSelectList = new SelectList(
                new[] { "Pending", "Confirmed", "Cancelled", "Completed" }, vm.BookingStatus);
            return View(vm);
        }

        try
        {
            var dto = new App.DTO.v1.BookingUpdateDto
            {
                Id = vm.Id,
                StartTime = vm.StartTime.ToUniversalTime(),
                EndTime = vm.EndTime.ToUniversalTime(),
                Purpose = vm.Purpose,
                BookingStatus = vm.BookingStatus ?? "Pending",
            };
            await _bookingService.UpdateAsync(dto, appUserId: null); // admin bypass
            TempData["Success"] = "Booking updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}