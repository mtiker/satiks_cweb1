using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Areas.Admin.ViewModels.EquipmentCategory;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class EquipmentCategoryController(IEquipmentCategoryService categoryService) : Controller
{
    private readonly IEquipmentCategoryService _categoryService = categoryService;

    public async Task<IActionResult> Index(
        string? search,
        string? requiresTraining,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _categoryService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(requiresTraining))
            all = all.Where(c => c.RequiresTraining.ToString().ToLower() == requiresTraining.ToLower());

        var ordered = all.OrderBy(c => c.Name).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new EquipmentCategoryListItem
            {
                Id               = c.Id,
                Name             = c.Name,
                RequiresTraining = c.RequiresTraining ? "Yes" : "No",
                EquipmentCount   = c.EquipmentCount,
                Description      = c.Description,
            })
            .ToList();

        var vm = new EquipmentCategoryIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = EquipmentCategoryIndexViewModel.BuildFilters(
                RequiresTrainingOptions.AsSelectList(), requiresTraining, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _categoryService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public IActionResult Create() => View(new EquipmentCategoryCreateEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipmentCategoryCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _categoryService.AddAsync(new EquipmentCategoryCreateDto
            {
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.Description,
                RequiresTraining = vm.RequiresTraining,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _categoryService.FindAsync(id);
        if (dto == null) return NotFound();

        return View(new EquipmentCategoryCreateEditViewModel
        {
            Id = dto.Id,
            NameEn = dto.NameEn ?? string.Empty,
            NameEt = dto.NameEt,
            Description = dto.Description,
            RequiresTraining = dto.RequiresTraining,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EquipmentCategoryCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _categoryService.UpdateAsync(new EquipmentCategoryUpdateDto
            {
                Id = vm.Id,
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Description = vm.Description,
                RequiresTraining = vm.RequiresTraining,
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _categoryService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _categoryService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }
}