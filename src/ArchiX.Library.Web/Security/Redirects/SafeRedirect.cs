using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Security.Redirects;

public static class SafeRedirect
{
    public static IActionResult LocalRedirectOrDefault(PageModel page, string? returnUrl, string defaultPath)
    {
        if (page is null) throw new ArgumentNullException(nameof(page));

        if (!string.IsNullOrWhiteSpace(returnUrl) && page.Url?.IsLocalUrl(returnUrl) == true)
            return page.LocalRedirect(returnUrl);

        return page.LocalRedirect(defaultPath);
    }
}
