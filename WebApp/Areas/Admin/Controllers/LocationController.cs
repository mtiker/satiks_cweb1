using App.BLL.Contracts;
using App.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Admin.ViewModels.Location;

namespace WebApp.Areas.Admin.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("Admin")]
[Authorize(Roles = "admin")]
public class LocationController(ILocationService locationService) : Controller
{
    private readonly ILocationService _locationService = locationService;

    public async Task<IActionResult> Index(
        string? search,
        string? building,
        string? floor,
        int page = 1,
        int pageSize = 20)
    {
        var all = (await _locationService.AllAsync()).ToList();

        var buildingOptions = all
            .Select(l => l.BuildingName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .Select(b => new SelectListItem(b, b))
            .ToList();

        var floorOptions = all
            .Where(l => l.Floor != null)
            .Select(l => l.Floor)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .Select(f => new SelectListItem(f, f))
            .ToList();

        IEnumerable<LocationDto> filtered = all;

        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(l =>
                l.BuildingName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (l.RoomNumber != null && l.RoomNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (l.Address != null && l.Address.Contains(search, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(building))
            filtered = filtered.Where(l => l.BuildingName.Equals(building, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(floor))
            filtered = filtered.Where(l => l.Floor != null && l.Floor.Equals(floor, StringComparison.OrdinalIgnoreCase));

        var ordered = filtered.OrderBy(l => l.BuildingName).ThenBy(l => l.Floor).ThenBy(l => l.RoomNumber).ToList();
        var totalCount = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationListItem
            {
                Id           = l.Id,
                BuildingName = l.BuildingName,
                Floor        = l.Floor,
                RoomNumber   = l.RoomNumber,
                Address      = l.Address,
            })
            .ToList();

        var vm = new LocationIndexViewModel
        {
            Items       = items,
            TotalCount  = totalCount,
            Page        = page,
            PageSize    = pageSize,
            FilterState = LocationIndexViewModel.BuildFilters(buildingOptions, floorOptions, building, floor, search)
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dto = await _locationService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    public IActionResult Create() => View(new LocationCreateEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _locationService.AddAsync(new LocationCreateDto
            {
                BuildingName = vm.BuildingNameEn,
                BuildingNameEt = vm.BuildingNameEt,
                Floor = vm.Floor,
                RoomNumber = vm.RoomNumber,
                Address = vm.Address,
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Location created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _locationService.FindAsync(id);
        if (dto == null) return NotFound();

        return View(new LocationCreateEditViewModel
        {
            Id = dto.Id,
            BuildingNameEn = dto.BuildingNameEn ?? string.Empty,
            BuildingNameEt = dto.BuildingNameEt,
            Floor = dto.Floor,
            RoomNumber = dto.RoomNumber,
            Address = dto.Address,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LocationCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _locationService.UpdateAsync(new LocationUpdateDto
            {
                Id = vm.Id,
                BuildingName = vm.BuildingNameEn,
                BuildingNameEt = vm.BuildingNameEt,
                Floor = vm.Floor,
                RoomNumber = vm.RoomNumber,
                Address = vm.Address,
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(vm);
        }

        TempData["Success"] = "Location updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _locationService.FindAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try { await _locationService.RemoveAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }

        TempData["Success"] = "Location deleted.";
        return RedirectToAction(nameof(Index));
    }
}