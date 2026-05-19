using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.MaintenanceRecord;

public class MaintenanceRecordIndexViewModel : PagedListViewModel<MaintenanceListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> statusOptions,
        IEnumerable<SelectListItem> equipments,
        string? isScheduled,
        string? equipmentId,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["isScheduled"] = isScheduled,
            ["equipment"]   = equipmentId
        },
        Definitions =
        [
            new("isScheduled", "Status", statusOptions.ToList()),
            new("equipment",   "Equipment", equipments.ToList())
        ]
    };
}

public class MaintenanceListItem
{
    public Guid      Id                  { get; set; }
    public string    Status              { get; set; } = string.Empty;
    public Guid      EquipmentId         { get; set; }
    public string    EquipmentName       { get; set; } = string.Empty;
    public DateTime  ScheduledDate       { get; set; }
    public DateTime? CompletedDate       { get; set; }
    public string?   Description         { get; set; }
    public string?   PerformedByUserName { get; set; }
}

public static class MaintenanceStatusOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "scheduled", Text = "Scheduled" },
        new() { Value = "completed", Text = "Completed" }
    ];
}