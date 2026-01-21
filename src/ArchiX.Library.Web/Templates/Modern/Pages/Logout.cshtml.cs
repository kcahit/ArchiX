using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages
{
    public class LogoutModel : PageModel
    {
        public string Reason { get; set; } = "manual";

        public IActionResult OnGet(string? reason = null)
        {
            Reason = reason ?? "manual";
            
            // Don't do any server-side cleanup here
            // Just show the logout page
            // JavaScript will handle sessionStorage cleanup
            
            return Page();
        }
    }
}


