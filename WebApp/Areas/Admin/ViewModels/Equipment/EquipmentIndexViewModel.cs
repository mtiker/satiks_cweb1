using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Equipment;

public class EquipmentIndexViewModel : PagedListViewModel<EquipmentListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> labs,
        IEnumerable<SelectListItem> categories,
        string? labId,
        string? categoryId,
        string? condition,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["lab"]       = labId,
            ["category"]  = categoryId,
            ["condition"] = condition
        },
        Definitions =
        [
            new("lab",       "Lab",       [.. labs]),
            new("category",  "Category",  [.. categories]),
            new("condition", "Condition", EquipmentConditionOptions.AsSelectList())
        ]
    };
}

public class EquipmentListItem
{
    public Guid    Id                 { get; set; }
    public string  Name               { get; set; } = string.Empty;
    public string? SerialNumber       { get; set; } = string.Empty;
    public Guid    LaboratoryId       { get; set; }
    public string  LaboratoryName     { get; set; } = string.Empty;
    public string  CategoryName       { get; set; } = string.Empty;
    public string  EquipmentCondition { get; set; } = string.Empty;
    public string  AvailabilityStatus { get; set; } = string.Empty;
    public bool RequiresTraining { get; set; }
}

public static class EquipmentConditionOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "New", Text = "New" },
        new() { Value = "Good", Text = "Good" },
        new() { Value = "Fair", Text = "Fair" },
        new() { Value = "NeedsRepair", Text = "Needs Repair" },
        new() { Value = "Decommissioned", Text = "Decommissioned" }
    ];
}