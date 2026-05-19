using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.MaintenanceRecord;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class MaintenanceRecordController(IMaintenanceRecordService maintenanceService, IEquipmentService equipmentService) : Controller
{
    private readonly IMaintenanceRecordService _maintenanceService = maintenanceService;
    private readonly IEquipmentService _equipmentService = equipmentService;

    public async Task<IActionResult> Index(
        string? search,
        string? isScheduled,
        string? equipmentId,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _maintenanceService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(m =>
                (m.EquipmentName != null && m.EquipmentName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                m.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(isScheduled))
        {
            if (isScheduled == "scheduled")
                all = all.Where(m => m.IsScheduled && !m.CompletedDate.HasValue);
            else
                all = all.Where(m => m.CompletedDate.HasValue);
        }

        if (!string.IsNullOrWhiteSpace(equipmentId))
            all = all.Where(m => m.EquipmentId.ToString() == equipmentId);



        var ordered = all.OrderByDescending(m => m.ScheduledDate).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MaintenanceListItem
            {
                Id                  = m.Id,
                Status              = m.IsScheduled && !m.CompletedDate.HasValue ? "Scheduled" : "Completed",
                EquipmentId         = m.EquipmentId,
                EquipmentName       = m.EquipmentName ?? string.Empty,
                ScheduledDate       = m.ScheduledDate,
                CompletedDate       = m.CompletedDate,
                Description         = m.Description,
                PerformedByUserName = m.PerformedByUserName,
            })
            .ToList();

        var equipmentOptions = (await _equipmentService.AllAsync())
            .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            .ToList();

        var vm = new MaintenanceRecordIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = MaintenanceRecordIndexViewModel.BuildFilters(
                MaintenanceStatusOptions.AsSelectList(), equipmentOptions, isScheduled, equipmentId, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _maintenanceService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public async Task<IActionResult> Create() => View(await BuildViewModel(new MaintenanceRecordCreateEditViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceRecordCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _maintenanceService.AddAsync(new MaintenanceRecordCreateDto
            {
                ScheduledDate = vm.ScheduledDate,
                CompletedDate = vm.CompletedDate,
                Description = vm.Description,
                Resolution = vm.Resolution,
                Cost = vm.Cost,
                IsScheduled = vm.IsScheduled,
                EquipmentId = vm.EquipmentId,
                PerformedByUserId = vm.PerformedByUserId,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(await BuildViewModel(vm));
        }

        TempData["Success"] = "Maintenance record created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _maintenanceService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new MaintenanceRecordCreateEditViewModel
        {
            Id = dto.Id,
            ScheduledDate = dto.ScheduledDate,
            CompletedDate = dto.CompletedDate,
            Description = dto.Description,
            Resolution = dto.Resolution,
            Cost = dto.Cost,
            IsScheduled = dto.IsScheduled,
            EquipmentId = dto.EquipmentId,
            PerformedByUserId = dto.PerformedByUserId,
        };

        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MaintenanceRecordCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _maintenanceService.UpdateAsync(new MaintenanceRecordUpdateDto
            {
                Id = vm.Id,
                ScheduledDate = vm.ScheduledDate,
                CompletedDate = vm.CompletedDate,
                Description = vm.Description,
                Resolution = vm.Resolution,
                Cost = vm.Cost,
                IsScheduled = vm.IsScheduled,
                EquipmentId = vm.EquipmentId,
                PerformedByUserId = vm.PerformedByUserId,
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(await BuildViewModel(vm));
        }

        TempData["Success"] = "Maintenance record updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _maintenanceService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _maintenanceService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Maintenance record deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<MaintenanceRecordCreateEditViewModel> BuildViewModel(MaintenanceRecordCreateEditViewModel vm)
    {
        var equipment = await _equipmentService.AllAsync();
        vm.EquipmentSelectList = new SelectList(equipment, "Id", "Name", vm.EquipmentId);
        return vm;
    }
}