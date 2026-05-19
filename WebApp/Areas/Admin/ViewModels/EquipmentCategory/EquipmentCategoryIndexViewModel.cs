using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.EquipmentCategory;

public class EquipmentCategoryIndexViewModel : PagedListViewModel<EquipmentCategoryListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> requiresTrainingOptions,
        string? requiresTraining,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["requiresTraining"] = requiresTraining
        },
        Definitions =
        [
            new("requiresTraining", "Requires Training", requiresTrainingOptions.ToList())
        ]
    };
}

public class EquipmentCategoryListItem
{
    public Guid    Id               { get; set; }
    public string  Name             { get; set; } = string.Empty;
    public string  RequiresTraining { get; set; } = string.Empty;
    public int     EquipmentCount   { get; set; }
    public string? Description      { get; set; }
}

public static class RequiresTrainingOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "true", Text = "Yes" },
        new() { Value = "false", Text = "No" }
    ];
}