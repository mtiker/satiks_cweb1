using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.Laboratory;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class LaboratoryController(ILaboratoryService laboratoryService, IDepartmentService departmentService, ILocationService locationService) : Controller
{
    private readonly ILaboratoryService _laboratoryService = laboratoryService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly ILocationService _locationService = locationService;

    public async Task<IActionResult> Index(
        string? search,
        string? department,
        string? location,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _laboratoryService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(l => l.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(department))
            all = all.Where(l => l.DepartmentId.ToString() == department);

        if (!string.IsNullOrWhiteSpace(location))
            all = all.Where(l => l.LocationId.ToString() == location);

        var ordered = all.OrderBy(l => l.Name).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LaboratoryListItem
            {
                Id                   = l.Id,
                Name                 = l.Name,
                DepartmentId         = l.DepartmentId,
                DepartmentName       = l.DepartmentName ?? string.Empty,
                LocationId           = l.LocationId,
                LocationBuildingName = l.LocationBuildingName ?? string.Empty,
                MaxOccupancy         = l.MaxOccupancy,
                EquipmentCount       = l.EquipmentCount,
                Description          = l.Description,
            })
            .ToList();

        var departments = (await _departmentService.AllAsync())
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToList();

        var locations = (await _locationService.AllAsync())
            .Select(loc => new SelectListItem(loc.BuildingName, loc.Id.ToString()))
            .ToList();

        var vm = new LaboratoryIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = LaboratoryIndexViewModel.BuildFilters(departments, locations, department, location, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _laboratoryService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public async Task<IActionResult> Create() => View(await BuildViewModel(new LaboratoryCreateEditViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LaboratoryCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _laboratoryService.AddAsync(new LaboratoryCreateDto
            {
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.DescriptionEn,
                DescriptionEt = vm.DescriptionEt,
                MaxOccupancy = vm.MaxOccupancy,
                DepartmentId = vm.DepartmentId,
                LocationId = vm.LocationId,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(await BuildViewModel(vm));
        }

        TempData["Success"] = "Laboratory created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _laboratoryService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new LaboratoryCreateEditViewModel
        {
            Id = dto.Id,
            NameEn = dto.NameEn ?? dto.Name,
            NameEt = dto.NameEt,
            DescriptionEn = dto.DescriptionEn,
            DescriptionEt = dto.DescriptionEt,
            MaxOccupancy = dto.MaxOccupancy,
            DepartmentId = dto.DepartmentId,
            LocationId = dto.LocationId,
        };

        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LaboratoryCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        try
        {
            await _laboratoryService.UpdateAsync(new LaboratoryUpdateDto
            {
                Id = vm.Id,
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.DescriptionEn,
                DescriptionEt = vm.DescriptionEt,
                MaxOccupancy = vm.MaxOccupancy,
                DepartmentId = vm.DepartmentId,
                LocationId = vm.LocationId,
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

        TempData["Success"] = "Laboratory updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _laboratoryService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _laboratoryService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Laboratory deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<LaboratoryCreateEditViewModel> BuildViewModel(LaboratoryCreateEditViewModel vm)
    {
        var departments = await _departmentService.AllAsync();
        var locations = await _locationService.AllAsync();

        vm.DepartmentSelectList = new SelectList(departments, "Id", "Name", vm.DepartmentId);
        vm.LocationSelectList = new SelectList(locations, "Id", "BuildingName", vm.LocationId);
        return vm;
    }
}
