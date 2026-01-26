using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ArchiX.Library.Web.Templates.Modern.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    public new int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsDevelopment { get; set; }

    public void OnGet([FromQuery] int? statusCode)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        StatusCode = statusCode ?? HttpContext.Response.StatusCode;
        IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        
        // Exception bilgisini al (eğer varsa)
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error != null)
        {
            var ex = exceptionFeature.Error;
            
            // Development: Full exception
            if (IsDevelopment)
            {
                ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
            }
            else
            {
                // Production: Kullanıcı dostu mesaj
                ErrorMessage = StatusCode switch
                {
                    404 => "Aradığınız sayfa bulunamadı.",
                    403 => "Bu sayfaya erişim yetkiniz yok.",
                    400 => "Geçersiz istek.",
                    500 => "Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.",
                    _ => "Bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                };
            }
        }
        else if (StatusCode.HasValue)
        {
            // HTTP status code based message
            ErrorMessage = StatusCode switch
            {
                404 => "Aradığınız sayfa bulunamadı.",
                403 => "Bu sayfaya erişim yetkiniz yok.",
                400 => "Geçersiz istek.",
                500 => "Sunucu hatası oluştu.",
                _ => $"HTTP {StatusCode} hatası."
            };
        }
    }
}
