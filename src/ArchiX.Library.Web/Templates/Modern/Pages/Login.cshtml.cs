using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Web.Templates.Modern.Pages
{
   
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= "/Dashboard";

            // GEÇÝCÝ OLARAK DOÐRULAMA KONTROLÜ KAPATILDI ÞÝFRE YOK ÞÝMDÝLÝK
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            // TODO: Authentication logic will be implemented here
            // Temporary: Accept any login for demo purposes

            TempData["StatusMessage"] = $"Hoþ geldiniz, {Input.Email}!";

            // Redirect to Dashboard (Modern template)
            return Redirect(returnUrl);
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Þifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Þifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni hatýrla")]
        public bool RememberMe { get; set; }
    }
}
