using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.CalibrationRecord;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class CalibrationRecordController(ICalibrationRecordService calibrationService, IEquipmentService equipmentService) : Controller
{
    private readonly ICalibrationRecordService _calibrationService = calibrationService;
    private readonly IEquipmentService _equipmentService = equipmentService;

    public async Task<IActionResult> Index(
        string? search,
        string? passed,
        string? equipmentId,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _calibrationService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(c =>
                (c.EquipmentName != null && c.EquipmentName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (c.CertificateNumber != null && c.CertificateNumber.Contains(search, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(passed))
        {
            bool passedBool = passed == "true";
            all = all.Where(c => c.Passed == passedBool);
        }



        if (!string.IsNullOrWhiteSpace(equipmentId))
            all = all.Where(c => c.EquipmentId.ToString() == equipmentId);

        var ordered = all.OrderByDescending(c => c.CalibrationDate).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CalibrationListItem
            {
                Id                  = c.Id,
                PassedFailed        = c.Passed ? "Passed" : "Failed",
                EquipmentId         = c.EquipmentId,
                EquipmentName       = c.EquipmentName ?? string.Empty,
                CalibrationDate     = c.CalibrationDate,
                NextCalibrationDue  = c.NextCalibrationDue,
                CertificateNumber   = c.CertificateNumber,
                CalibratedByUserName = c.CalibratedByUserName,
            })
            .ToList();

        var vm = new CalibrationRecordIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = CalibrationRecordIndexViewModel.BuildFilters(
                PassedOptions.AsSelectList(),
                (await _equipmentService.AllAsync()).Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = e.Id.ToString(), Text = e.Name }),
                passed, equipmentId, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _calibrationService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public async Task<IActionResult> Create() => View(await BuildViewModel(new CalibrationRecordCreateEditViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CalibrationRecordCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _calibrationService.AddAsync(new CalibrationRecordCreateDto
            {
                CalibrationDate = vm.CalibrationDate,
                NextCalibrationDue = vm.NextCalibrationDue,
                CertificateNumber = vm.CertificateNumber,
                Notes = vm.Notes,
                Passed = vm.Passed,
                EquipmentId = vm.EquipmentId,
                CalibratedByUserId = vm.CalibratedByUserId,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(await BuildViewModel(vm));
        }

        TempData["Success"] = "Calibration record created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _calibrationService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new CalibrationRecordCreateEditViewModel
        {
            Id = dto.Id,
            CalibrationDate = dto.CalibrationDate,
            NextCalibrationDue = dto.NextCalibrationDue,
            CertificateNumber = dto.CertificateNumber,
            Notes = dto.Notes,
            Passed = dto.Passed,
            EquipmentId = dto.EquipmentId,
            CalibratedByUserId = dto.CalibratedByUserId,
        };

        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CalibrationRecordCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _calibrationService.UpdateAsync(new CalibrationRecordUpdateDto
            {
                Id = vm.Id,
                CalibrationDate = vm.CalibrationDate,
                NextCalibrationDue = vm.NextCalibrationDue,
                CertificateNumber = vm.CertificateNumber,
                Notes = vm.Notes,
                Passed = vm.Passed,
                EquipmentId = vm.EquipmentId,
                CalibratedByUserId = vm.CalibratedByUserId,
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

        TempData["Success"] = "Calibration record updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _calibrationService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _calibrationService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Calibration record deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CalibrationRecordCreateEditViewModel> BuildViewModel(CalibrationRecordCreateEditViewModel vm)
    {
        var equipment = await _equipmentService.AllAsync();
        vm.EquipmentSelectList = new SelectList(equipment, "Id", "Name", vm.EquipmentId);
        return vm;
    }
}