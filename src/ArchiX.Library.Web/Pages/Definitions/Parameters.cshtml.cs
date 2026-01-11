using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Definitions;

public class ParametersModel : PageModel
{
    [SuppressMessage(
        "Performance",
        "CA1822:Members that do not access instance data or call instance methods can be marked as static",
        Justification = "Razor Pages handler methods must be instance methods.")]
    public void OnGet()
    {
    }
}
