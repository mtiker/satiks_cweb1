using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Areas.Admin.ViewModels.Department;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class DepartmentController(IDepartmentService departmentService) : Controller
{
    private readonly IDepartmentService _departmentService = departmentService;

    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _departmentService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(d => d.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        var ordered = all.OrderBy(d => d.Name).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DepartmentListItem
            {
                Id              = d.Id,
                Name            = d.Name,
                CostCenter      = d.CostCenter,
                LaboratoryCount = d.LaboratoryCount,
                Description     = d.Description,
            })
            .ToList();

        var vm = new DepartmentIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = DepartmentIndexViewModel.BuildFilters(search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _departmentService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public IActionResult Create() => View(new DepartmentCreateEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _departmentService.AddAsync(new DepartmentCreateDto
            {
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.Description,
                CostCenter = vm.CostCenter,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Department created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _departmentService.FindAsync(id);
        if (dto == null) return NotFound();

        return View(new DepartmentCreateEditViewModel
        {
            Id = dto.Id,
            NameEn = dto.NameEn ?? string.Empty,
            NameEt = dto.NameEt,
            Description = dto.Description,
            CostCenter = dto.CostCenter,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DepartmentCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _departmentService.UpdateAsync(new DepartmentUpdateDto
            {
                Id = vm.Id,
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.Description,
                CostCenter = vm.CostCenter,
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Department updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _departmentService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _departmentService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Department deleted.";
        return RedirectToAction(nameof(Index));
    }
}