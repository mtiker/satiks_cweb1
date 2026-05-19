using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.Equipment;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly ILaboratoryService _laboratoryService;
    private readonly IEquipmentCategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;

    public EquipmentController(
        IEquipmentService equipmentService,
        ILaboratoryService laboratoryService,
        IEquipmentCategoryService categoryService,
        IManufacturerService manufacturerService)
    {
        _equipmentService = equipmentService;
        _laboratoryService = laboratoryService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
    }

    public async Task<IActionResult> Index(
        string? search,
        string? lab,
        string? category,
        string? condition,
        int page = 1,
        int pageSize = 20)
    {
        var all = await _equipmentService.AllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            all = all.Where(e =>
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (e.SerialNumber != null && e.SerialNumber.Contains(search, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(lab))
            all = all.Where(e => e.LaboratoryId.ToString() == lab);

        if (!string.IsNullOrWhiteSpace(category))
            all = all.Where(e => e.EquipmentCategoryId.ToString() == category);

        if (!string.IsNullOrWhiteSpace(condition))
            all = all.Where(e => e.EquipmentCondition == condition);

        var ordered = all.OrderBy(e => e.Name).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EquipmentListItem
            {
                Id                 = e.Id,
                Name               = e.Name,
                SerialNumber       = e.SerialNumber,
                LaboratoryId       = e.LaboratoryId,
                LaboratoryName     = e.LaboratoryName ?? string.Empty,
                CategoryName       = e.CategoryName ?? string.Empty,
                EquipmentCondition = e.EquipmentCondition,
                AvailabilityStatus = e.IsAvailableForBooking ? "Available" : "Unavailable",
                RequiresTraining   = e.RequiresTraining,
            })
            .ToList();

        var labs = (await _laboratoryService.AllAsync())
            .Select(l => new SelectListItem(l.Name, l.Id.ToString()))
            .ToList();

        var categories = (await _categoryService.AllAsync())
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();

        var vm = new EquipmentIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = EquipmentIndexViewModel.BuildFilters(labs, categories, lab, category, condition, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _equipmentService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public async Task<IActionResult> Create()
        => View(await BuildViewModel(new EquipmentCreateEditViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipmentCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(await BuildViewModel(vm));

        try
        {
            await _equipmentService.AddAsync(new EquipmentCreateDto
            {
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                SerialNumber = vm.SerialNumber,
                ModelName = vm.ModelName,
                Description = vm.DescriptionEn,
                DescriptionEt = vm.DescriptionEt,
                PurchaseDate = vm.PurchaseDate,
                WarrantyExpiry = vm.WarrantyExpiry,
                PurchasePrice = vm.PurchasePrice,
                IsAvailableForBooking = vm.IsAvailableForBooking,
                EquipmentCondition = vm.EquipmentCondition,
                LaboratoryId = vm.LaboratoryId,
                EquipmentCategoryId = vm.EquipmentCategoryId,
                ManufacturerId = vm.ManufacturerId,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(await BuildViewModel(vm));
        }

        TempData["Success"] = "Equipment created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _equipmentService.FindAsync(id);
        if (dto == null) return NotFound();

        var vm = new EquipmentCreateEditViewModel
        {
            Id = dto.Id,
            NameEn = dto.NameEn ?? dto.Name,
            NameEt = dto.NameEt,
            SerialNumber = dto.SerialNumber,
            ModelName = dto.ModelName,
            DescriptionEn = dto.DescriptionEn,
            DescriptionEt = dto.DescriptionEt,
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpiry = dto.WarrantyExpiry,
            PurchasePrice = dto.PurchasePrice,
            IsAvailableForBooking = dto.IsAvailableForBooking,
            EquipmentCondition = dto.EquipmentCondition,
            LaboratoryId = dto.LaboratoryId,
            EquipmentCategoryId = dto.EquipmentCategoryId,
            ManufacturerId = dto.ManufacturerId,
        };

        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EquipmentCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(await BuildViewModel(vm));

        try
        {
            await _equipmentService.UpdateAsync(new EquipmentUpdateDto
            {
                Id = vm.Id,
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                SerialNumber = vm.SerialNumber,
                ModelName = vm.ModelName,
                Description = vm.DescriptionEn,
                DescriptionEt = vm.DescriptionEt,
                PurchaseDate = vm.PurchaseDate,
                WarrantyExpiry = vm.WarrantyExpiry,
                PurchasePrice = vm.PurchasePrice,
                IsAvailableForBooking = vm.IsAvailableForBooking,
                EquipmentCondition = vm.EquipmentCondition,
                LaboratoryId = vm.LaboratoryId,
                EquipmentCategoryId = vm.EquipmentCategoryId,
                ManufacturerId = vm.ManufacturerId,
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

        TempData["Success"] = "Equipment updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _equipmentService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _equipmentService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Equipment deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static readonly List<string> ConditionValues = Enum.GetNames<App.Domain.EquipmentCondition>().ToList();

    private async Task<EquipmentCreateEditViewModel> BuildViewModel(EquipmentCreateEditViewModel vm)
    {
        var labs = await _laboratoryService.AllAsync();
        var cats = await _categoryService.AllAsync();
        var mans = await _manufacturerService.AllAsync();

        vm.LaboratorySelectList = new SelectList(labs, "Id", "Name", vm.LaboratoryId);
        vm.CategorySelectList = new SelectList(cats, "Id", "Name", vm.EquipmentCategoryId);
        vm.ManufacturerSelectList = new SelectList(mans, "Id", "Name", vm.ManufacturerId);
        vm.ConditionSelectList = new SelectList(ConditionValues, vm.EquipmentCondition);
        return vm;
    }
}
