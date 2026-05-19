using WebApp.ViewModels.Shared;

namespace WebApp.Areas.Admin.ViewModels.Department;

public class DepartmentIndexViewModel : PagedListViewModel<DepartmentListItem>
{
    public static FilterState BuildFilters(string? search)
    => new()
    {
        SearchTerm = search,
        CurrentValues = new(),
        Definitions = []
    };
}

public class DepartmentListItem
{
    public Guid    Id             { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? CostCenter     { get; set; }
    public int     LaboratoryCount { get; set; }
    public string? Description    { get; set; }
}