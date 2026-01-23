using ArchiX.Library.Web.ViewModels.Dataset;

using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents.Dataset;

[ViewComponent(Name = "DatasetKombine")]
public sealed class DatasetKombineViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DatasetKombineViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.InstanceId))
            model.InstanceId = "dskombine";

        return View("~/Templates/Modern/Pages/Shared/Components/Dataset/DatasetKombine/Default.cshtml", model);
    }
}
