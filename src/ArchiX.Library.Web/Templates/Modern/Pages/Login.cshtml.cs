using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Web.Security.Redirects;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public IActionResult OnPost(string? returnUrl = null)
        {
            const string defaultUrl = "/Dashboard";

            TempData["StatusMessage"] = $"Hos geldiniz, {Input.Email}!";

            return SafeRedirect.LocalRedirectOrDefault(this, returnUrl, defaultUrl);
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Sifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni hatirla")]
        public bool RememberMe { get; set; }
    }
}
