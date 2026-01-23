using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents.Dataset;

[ViewComponent(Name = "DatasetRecord")]
public sealed class DatasetRecordViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Templates/Modern/Pages/Shared/Components/Dataset/DatasetRecord/Default.cshtml");
    }
}
