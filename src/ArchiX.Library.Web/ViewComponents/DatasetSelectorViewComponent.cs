using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents;

public sealed class DatasetSelectorViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DatasetSelectorViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "gridTable";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/DatasetSelector/Default.cshtml", model);
    }
}
