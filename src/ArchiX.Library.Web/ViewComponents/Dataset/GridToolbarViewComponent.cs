using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents.Dataset;

public class GridToolbarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridToolbarViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "dsgrid";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/Dataset/GridToolbar/Default.cshtml", model);
    }
}
