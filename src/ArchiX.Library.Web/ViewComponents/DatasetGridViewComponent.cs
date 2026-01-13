using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents;

[ViewComponent(Name = "DatasetGrid")]
public class DatasetGridViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridTableViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "dsgrid";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/Dataset/DatasetGrid/Default.cshtml", model);
    }
}
