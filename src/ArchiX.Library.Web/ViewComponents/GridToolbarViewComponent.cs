using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents;

public class GridToolbarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridToolbarViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "gridTable";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/GridToolbar/Default.cshtml", model);
    }
}
