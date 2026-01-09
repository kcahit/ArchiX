using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IActionResult OnPost(string? returnUrl = null)
        {
            returnUrl ??= "/Dashboard";

            TempData["StatusMessage"] = $"Hoş geldiniz, {Input.Email}!";

            return Redirect(returnUrl);
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni hatırla")]
        public bool RememberMe { get; set; }
    }
}
