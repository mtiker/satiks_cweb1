using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.Manufacturer;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class ManufacturerController(IManufacturerService manufacturerService) : Controller
{
    private readonly IManufacturerService _manufacturerService = manufacturerService;

    public async Task<IActionResult> Index(
        string? search,
        string? country,
        int page = 1,
        int pageSize = 20)
    {
        var all = (await _manufacturerService.AllAsync()).ToList();

        var countryOptions = all
            .Where(m => m.Country != null)
            .Select(m => m.Country!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .Select(c => new SelectListItem(c, c))
            .ToList();

        IEnumerable<ManufacturerDto> filtered = all;

        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(m => m.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(country))
            filtered = filtered.Where(m => m.Country != null && m.Country.Equals(country, StringComparison.OrdinalIgnoreCase));

        var ordered = filtered.OrderBy(m => m.Name).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ManufacturerListItem
            {
                Id           = m.Id,
                Name         = m.Name,
                Country      = m.Country,
                Website      = m.Website,
                SupportPhone = m.SupportPhone,
                SupportEmail = m.SupportEmail,
            })
            .ToList();

        var vm = new ManufacturerIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = ManufacturerIndexViewModel.BuildFilters(countryOptions, country, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _manufacturerService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public IActionResult Create() => View(new ManufacturerCreateEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManufacturerCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _manufacturerService.AddAsync(new ManufacturerCreateDto
            {
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Country = vm.Country,
                Website = vm.Website,
                SupportPhone = vm.SupportPhone,
                SupportEmail = vm.SupportEmail,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Manufacturer created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _manufacturerService.FindAsync(id);
        if (dto == null) return NotFound();

        return View(new ManufacturerCreateEditViewModel
        {
            Id = dto.Id,
            NameEn = dto.NameEn ?? dto.Name,
            NameEt = dto.NameEt,
            Country = dto.Country,
            Website = dto.Website,
            SupportPhone = dto.SupportPhone,
            SupportEmail = dto.SupportEmail,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ManufacturerCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _manufacturerService.UpdateAsync(new ManufacturerUpdateDto
            {
                Id = vm.Id,
                Name = vm.NameEn,
                NameEt = vm.NameEt,
                Country = vm.Country,
                Website = vm.Website,
                SupportPhone = vm.SupportPhone,
                SupportEmail = vm.SupportEmail,
            });
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

        TempData["Success"] = "Manufacturer updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _manufacturerService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _manufacturerService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Manufacturer deleted.";
        return RedirectToAction(nameof(Index));
    }
}
