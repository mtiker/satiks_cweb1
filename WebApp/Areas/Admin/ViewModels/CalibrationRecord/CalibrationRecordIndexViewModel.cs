using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.CalibrationRecord;

public class CalibrationRecordIndexViewModel : PagedListViewModel<CalibrationListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> passedOptions,
        IEnumerable<SelectListItem> equipments,
        string? passed,
        string? equipmentId,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["passed"]    = passed,
            ["equipment"] = equipmentId
        },
        Definitions =
        [
            new("passed",    "Passed",    passedOptions.ToList()),
            new("equipment", "Equipment", equipments.ToList())
        ]
    };
}

public class CalibrationListItem
{
    public Guid    Id                  { get; set; }
    public string  PassedFailed        { get; set; } = string.Empty;
    public Guid    EquipmentId         { get; set; }
    public string  EquipmentName       { get; set; } = string.Empty;
    public DateTime CalibrationDate    { get; set; }
    public DateTime NextCalibrationDue { get; set; }
    public string? CertificateNumber   { get; set; }
    public string? CalibratedByUserName { get; set; }
}

public static class PassedOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "true", Text = "Passed" },
        new() { Value = "false", Text = "Failed" }
    ];
}