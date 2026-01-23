using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents.Dataset;

public sealed class DatasetSelectorViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DatasetSelectorViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = "dsgrid";
        }

        return View("~/Templates/Modern/Pages/Shared/Components/Dataset/DatasetSelector/Default.cshtml", model);
    }
}
