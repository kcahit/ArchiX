using ArchiX.Library.Abstractions.Hosting;
using ArchiX.Library.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace ArchiX.Library.Web.ViewComponents
{
    public sealed class SidebarViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IApplicationContext _appContext;

        public SidebarViewComponent(IMenuService menuService, IApplicationContext appContext)
        {
            _menuService = menuService;
            _appContext = appContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = await _menuService.GetMenuForApplicationAsync(_appContext.ApplicationId);
            return View(model: menu);
        }
    }
}
