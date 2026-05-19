using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Manufacturer;

public class ManufacturerIndexViewModel : PagedListViewModel<ManufacturerListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> countries,
        string? country,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["country"] = country
        },
        Definitions =
        [
            new("country", "Country", countries.ToList())
        ]
    };
}

public class ManufacturerListItem
{
    public Guid    Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Country      { get; set; }
    public string? Website      { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
}