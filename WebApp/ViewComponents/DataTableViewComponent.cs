using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Shared;

namespace WebApp.ViewComponents;

public class DataTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string rowsPartial,
        object model,
        DataTableConfig config)
    {
        var isEmpty = model switch
        {
            ICollection<object> c => c.Count == 0,
            IEnumerable<object> e => !e.Any(),
            _                     => false
        };

        return View(new DataTableViewModel
        {
            RowsPartialName = rowsPartial,
            RowsModel       = model,
            Config          = config,
            IsEmpty         = isEmpty
        });
    }
}