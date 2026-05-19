using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.TrainingCertification;

public class TrainingCertIndexViewModel : PagedListViewModel<CertificationListItem>
{
    public static FilterState BuildFilters(
        IEnumerable<SelectListItem> statuses,
        IEnumerable<SelectListItem> users,
        IEnumerable<SelectListItem> categories,
        string? status,
        string? userId,
        string? equipmentCategoryId,
        string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new()
        {
            ["status"]           = status,
            ["userId"]           = userId,
            ["equipmentCategory"] = equipmentCategoryId
        },
        Definitions =
        [
            new("status",            "Status",            statuses.ToList()),
            new("userId",            "User",              users.ToList()),
            new("equipmentCategory", "Equipment Category", categories.ToList())
        ]
    };
}

public class CertificationListItem
{
    public Guid      Id                    { get; set; }
    public string    Status                { get; set; } = string.Empty;
    public Guid      AppUserId             { get; set; }
    public string    AppUserFullName        { get; set; } = string.Empty;
    public Guid      EquipmentCategoryId   { get; set; }
    public string    EquipmentCategoryName { get; set; } = string.Empty;
    public DateTime  CertifiedDate         { get; set; }
    public DateTime? ExpiryDate            { get; set; }
    public string?   CertificateReference  { get; set; }
}

public static class TrainingCertStatusOptions
{
    public static List<SelectListItem> AsSelectList() =>
    [
        new() { Value = "Pending", Text = "Pending" },
        new() { Value = "Approved", Text = "Approved" },
        new() { Value = "Rejected", Text = "Rejected" },
        new() { Value = "Revoked", Text = "Revoked" },
        new() { Value = "Expired", Text = "Expired" }
    ];
}