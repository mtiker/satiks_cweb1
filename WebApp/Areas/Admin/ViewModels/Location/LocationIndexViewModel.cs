using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Location;

public class LocationIndexViewModel : PagedListViewModel<LocationListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> buildings,
        IEnumerable<SelectListItem> floors,
        string? buildingId,
        string? floor,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["building"] = buildingId,
            ["floor"]    = floor
        },
        Definitions =
        [
            new("building", "Building", buildings.ToList()),
            new("floor",    "Floor",    floors.ToList())
        ]
    };
}

public class LocationListItem
{
    public Guid   Id              { get; set; }
    public string BuildingName   { get; set; } = string.Empty;
    public string? Floor         { get; set; }
    public string? RoomNumber   { get; set; }
    public string? Address       { get; set; }
}