using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents.Grid;

public class GridRecordAccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridRecordAccordionViewModel model)
    {
        return View("~/Templates/Modern/Pages/Shared/Components/Grid/GridRecordAccordion/Default.cshtml", model);
    }
}
