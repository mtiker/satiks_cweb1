using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Booking;

public class BookingIndexViewModel : PagedListViewModel<BookingListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> statuses,
        IEnumerable<SelectListItem> users,
        IEnumerable<SelectListItem> equipments,
        string? status,
        string? userId,
        string? equipmentId,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["status"]    = status,
            ["userId"]    = userId,
            ["equipment"] = equipmentId
        },
        Definitions =
        [
            new("status",    "Status",    statuses.ToList()),
            new("userId",    "User",      users.ToList()),
            new("equipment", "Equipment", equipments.ToList())
        ]
    };
}

public class BookingListItem
{
    public Guid    Id            { get; set; }
    public string  BookingStatus { get; set; } = string.Empty;
    public DateTime StartTime    { get; set; }
    public DateTime EndTime      { get; set; }
    public string? Purpose       { get; set; }
    public Guid    EquipmentId   { get; set; }
    public string  EquipmentName { get; set; } = string.Empty;
    public Guid    AppUserId     { get; set; }
    public string  UserFullName  { get; set; } = string.Empty;
}

public static class BookingStatusOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "Pending", Text = "Pending" },
        new() { Value = "Confirmed", Text = "Confirmed" },
        new() { Value = "Cancelled", Text = "Cancelled" },
        new() { Value = "Completed", Text = "Completed" }
    ];
}