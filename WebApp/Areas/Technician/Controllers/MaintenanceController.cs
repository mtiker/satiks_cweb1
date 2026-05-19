using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Technician;

namespace WebApp.Areas.Technician.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Technician")]
[Authorize(Roles = "technician,admin")]
public class MaintenanceController(
    IMaintenanceRecordService maintenanceService,
    IEquipmentService equipmentService) : Controller
{
    private readonly IMaintenanceRecordService _maintenanceService = maintenanceService;
    private readonly IEquipmentService _equipmentService = equipmentService;

    public async Task<IActionResult> Index(string? equipmentId, string? status)
    {
        var all = await _maintenanceService.AllAsync();

        if (!string.IsNullOrWhiteSpace(equipmentId))
            all = all.Where(m => m.EquipmentId.ToString() == equipmentId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status == "scheduled")
                all = all.Where(m => m.IsScheduled && !m.CompletedDate.HasValue);
            else if (status == "completed")
                all = all.Where(m => m.CompletedDate.HasValue);
        }

        var equipment = (await _equipmentService.AllAsync())
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            .ToList();

        ViewBag.EquipmentSelectList = new SelectList(equipment, "Value", "Text", equipmentId);
        ViewBag.StatusSelectList = new SelectList(new[]
        {
            new SelectListItem { Value = "",          Text = "All"       },
            new SelectListItem { Value = "scheduled", Text = "Scheduled" },
            new SelectListItem { Value = "completed", Text = "Completed" },
        }, "Value", "Text", status);
        ViewBag.SelectedEquipmentId = equipmentId;
        ViewBag.SelectedStatus      = status;

        return View(all.OrderByDescending(m => m.ScheduledDate).ToList());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _maintenanceService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _maintenanceService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new MaintenanceEditViewModel
        {
            Id            = dto.Id,
            EquipmentName = dto.EquipmentName ?? string.Empty,
            Description   = dto.Description,
            Resolution    = dto.Resolution,
            CompletedDate = dto.CompletedDate,
            Cost          = dto.Cost,
            IsScheduled   = dto.IsScheduled,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MaintenanceEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var existing = await _maintenanceService.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            await _maintenanceService.UpdateAsync(new MaintenanceRecordUpdateDto
            {
                Id                = existing.Id,
                ScheduledDate     = existing.ScheduledDate,
                Description       = existing.Description,
                EquipmentId       = existing.EquipmentId,
                PerformedByUserId = existing.PerformedByUserId,
                IsScheduled       = vm.CompletedDate.HasValue ? false : existing.IsScheduled,
                CompletedDate     = vm.CompletedDate,
                Resolution        = vm.Resolution,
                Cost              = vm.Cost,
            });

            TempData["Success"] = "Maintenance record updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }
    }
}
