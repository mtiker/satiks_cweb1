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
public class CalibrationController(
    ICalibrationRecordService calibrationService,
    IEquipmentService equipmentService) : Controller
{
    private readonly ICalibrationRecordService _calibrationService = calibrationService;
    private readonly IEquipmentService _equipmentService = equipmentService;

    public async Task<IActionResult> Index(string? equipmentId, string? passed)
    {
        var all = await _calibrationService.AllAsync();

        if (!string.IsNullOrWhiteSpace(equipmentId))
            all = all.Where(c => c.EquipmentId.ToString() == equipmentId);

        if (!string.IsNullOrWhiteSpace(passed))
        {
            bool passedBool = passed == "true";
            all = all.Where(c => c.Passed == passedBool);
        }

        var equipment = (await _equipmentService.AllAsync())
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            .ToList();

        ViewBag.EquipmentSelectList = new SelectList(equipment, "Value", "Text", equipmentId);
        ViewBag.PassedSelectList = new SelectList(new[]
        {
            new SelectListItem { Value = "",      Text = "All"    },
            new SelectListItem { Value = "true",  Text = "Passed" },
            new SelectListItem { Value = "false", Text = "Failed" },
        }, "Value", "Text", passed);
        ViewBag.SelectedEquipmentId = equipmentId;
        ViewBag.SelectedPassed      = passed;

        return View(all.OrderByDescending(c => c.CalibrationDate).ToList());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _calibrationService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _calibrationService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new CalibrationEditViewModel
        {
            Id                 = dto.Id,
            EquipmentName      = dto.EquipmentName ?? string.Empty,
            CertificateNumber  = dto.CertificateNumber,
            Passed             = dto.Passed,
            Notes              = dto.Notes,
            NextCalibrationDue = dto.NextCalibrationDue,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CalibrationEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var existing = await _calibrationService.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            await _calibrationService.UpdateAsync(new CalibrationRecordUpdateDto
            {
                Id                 = existing.Id,
                CalibrationDate    = existing.CalibrationDate,
                EquipmentId        = existing.EquipmentId,
                CalibratedByUserId = existing.CalibratedByUserId,
                CertificateNumber  = vm.CertificateNumber,
                Passed             = vm.Passed,
                Notes              = vm.Notes,
                NextCalibrationDue = vm.NextCalibrationDue,
            });

            TempData["Success"] = "Calibration record updated.";
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
