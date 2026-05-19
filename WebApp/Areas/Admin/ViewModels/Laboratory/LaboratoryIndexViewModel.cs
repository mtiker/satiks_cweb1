using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Laboratory;

public class LaboratoryIndexViewModel : PagedListViewModel<LaboratoryListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> departments,
        IEnumerable<SelectListItem> locations,
        string? departmentId,
        string? locationId,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["department"] = departmentId,
            ["location"]   = locationId
        },
        Definitions =
        [
            new("department", "Department", departments.ToList()),
            new("location",   "Location",   locations.ToList())
        ]
    };
}

public class LaboratoryListItem
{
    public Guid    Id                   { get; set; }
    public string  Name                 { get; set; } = string.Empty;
    public Guid    DepartmentId         { get; set; }
    public string  DepartmentName       { get; set; } = string.Empty;
    public Guid    LocationId           { get; set; }
    public string  LocationBuildingName { get; set; } = string.Empty;
    public int?    MaxOccupancy         { get; set; }
    public int     EquipmentCount       { get; set; }
    public string? Description          { get; set; }
}