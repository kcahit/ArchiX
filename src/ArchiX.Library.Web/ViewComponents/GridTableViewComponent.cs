using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents;

public class GridTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridTableViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "gridTable";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/GridTable/Default.cshtml", model);
    }
}
