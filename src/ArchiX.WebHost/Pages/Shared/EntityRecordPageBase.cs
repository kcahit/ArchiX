using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Web.Pages.Shared;

/// <summary>
/// Generic base class for entity CRUD record pages.
/// Supports Create, Update, SoftDelete operations with ApplicationId=1 protection.
/// </summary>
public abstract class EntityRecordPageBase<TEntity, TFormModel> : PageModel 
    where TEntity : BaseEntity, new()
    where TFormModel : class, new()
{
    protected readonly AppDbContext Db;

    protected EntityRecordPageBase(AppDbContext db)
    {
        Db = db;
    }

    public bool IsNew { get; set; }
    public TEntity? Entity { get; set; }

    [BindProperty]
    public TFormModel Form { get; set; } = new();

    /// <summary>
    /// Entity name for messages (e.g., "Application", "Parameter").
    /// </summary>
    protected abstract string EntityName { get; }

    /// <summary>
    /// List page URL (e.g., "/Definitions/Application").
    /// </summary>
    protected abstract string ListPageUrl { get; }

    /// <summary>
    /// Map entity to form model (for edit mode).
    /// </summary>
    protected abstract TFormModel EntityToForm(TEntity entity);

    /// <summary>
    /// Apply form values to entity (for create/update).
    /// </summary>
    protected abstract void ApplyFormToEntity(TFormModel form, TEntity entity);

    public virtual async Task OnGetAsync([FromQuery] int? id, CancellationToken ct)
    {
        IsNew = id == null || id == 0;

        if (!IsNew)
        {
            Entity = await Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);

            if (Entity == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Form = EntityToForm(Entity);
        }
    }

    public virtual async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"[{EntityName}] OnPostCreateAsync BAŞLADI");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[{EntityName}] ModelState INVALID");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[{EntityName}] Validation Error: {error.ErrorMessage}");
                }
                IsNew = true;
                return Page();
            }

            var entity = new TEntity();
            ApplyFormToEntity(Form, entity);

            // TODO: Get real userId from HttpContext.User
            entity.MarkCreated(userId: 1);
            Db.Set<TEntity>().Add(entity);
            
            Console.WriteLine($"[{EntityName}] SaveChangesAsync çağrılıyor...");
            var affected = await Db.SaveChangesAsync(ct);
            Console.WriteLine($"[{EntityName}] SaveChangesAsync tamamlandı - {affected} satır etkilendi");

            return HandlePostSuccessRedirect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{EntityName}] OnPostCreateAsync EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            
            // AJAX request ise JSON error döndür
            if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                Request.Headers["X-ArchiX-Tab"] == "1")
            {
                return StatusCode(500, new { success = false, message = "Kayıt oluşturulurken bir hata oluştu." });
            }
            
            // Normal request ise Error page'e yönlendir
            throw; // Global exception handler yakalayacak
        }
    }

    public virtual async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"[{EntityName}] OnPostUpdateAsync BAŞLADI - ID: {id}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[{EntityName}] ModelState INVALID");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[{EntityName}] Validation Error: {error.ErrorMessage}");
                }
                await OnGetAsync(id, ct);
                return Page();
            }

            var entity = await Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity == null)
            {
                Console.WriteLine($"[{EntityName}] Entity NOT FOUND - ID: {id}");
                
                // AJAX request ise JSON döndür
                if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                    Request.Headers["X-ArchiX-Tab"] == "1")
                {
                    return NotFound(new { success = false, message = "Kayıt bulunamadı." });
                }
                
                return NotFound();
            }

            Console.WriteLine($"[{EntityName}] Entity bulundu - ID: {entity.Id}");
            ApplyFormToEntity(Form, entity);

            // TODO: Get real userId from HttpContext.User
            entity.MarkUpdated(userId: 1);

            Console.WriteLine($"[{EntityName}] SaveChangesAsync çağrılıyor...");
            var affected = await Db.SaveChangesAsync(ct);
            Console.WriteLine($"[{EntityName}] SaveChangesAsync tamamlandı - {affected} satır etkilendi");
            
            return HandlePostSuccessRedirect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{EntityName}] OnPostUpdateAsync EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            
            // AJAX request ise JSON error döndür
            if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                Request.Headers["X-ArchiX-Tab"] == "1")
            {
                return StatusCode(500, new { success = false, message = "Kayıt güncellenirken bir hata oluştu." });
            }
            
            // Normal request ise Error page'e yönlendir
            throw; // Global exception handler yakalayacak
        }
    }

    public virtual async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"[{EntityName}] OnPostDeleteAsync BAŞLADI - ID: {id}");
            
            if (id == 1)
            {
                Console.WriteLine($"[{EntityName}] Sistem kaydı (ID=1) silinemez");
                ModelState.AddModelError(string.Empty, $"Sistem kaydı (ID=1) silinemez.");
                
                // AJAX request ise JSON döndür
                if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                    Request.Headers["X-ArchiX-Tab"] == "1")
                {
                    return BadRequest(new { success = false, message = "Sistem kaydı silinemez." });
                }
                
                await OnGetAsync(id, ct);
                return Page();
            }

            var entity = await Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity == null)
            {
                Console.WriteLine($"[{EntityName}] Entity NOT FOUND - ID: {id} (muhtemelen zaten silinmiş)");
                
                // AJAX request ise 404 JSON döndür
                if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                    Request.Headers["X-ArchiX-Tab"] == "1")
                {
                    return NotFound(new { success = false, message = "Kayıt bulunamadı. Muhtemelen daha önce silinmiş." });
                }
                
                return NotFound();
            }

            Console.WriteLine($"[{EntityName}] Entity bulundu - ID: {entity.Id}, soft delete yapılıyor...");
            // TODO: Get real userId from HttpContext.User
            entity.SoftDelete(userId: 1);
            
            Console.WriteLine($"[{EntityName}] SaveChangesAsync çağrılıyor...");
            var affected = await Db.SaveChangesAsync(ct);
            Console.WriteLine($"[{EntityName}] SaveChangesAsync tamamlandı - {affected} satır etkilendi");

            return HandlePostSuccessRedirect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{EntityName}] OnPostDeleteAsync EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            
            // AJAX request ise JSON error döndür
            if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
                Request.Headers["X-ArchiX-Tab"] == "1")
            {
                return StatusCode(500, new { success = false, message = "Kayıt silinirken bir hata oluştu." });
            }
            
            // Normal request ise Error page'e yönlendir
            throw; // Global exception handler yakalayacak
        }
    }

    /// <summary>
    /// Handle post-submit redirect (accordion or normal navigation).
    /// </summary>
    protected virtual IActionResult HandlePostSuccessRedirect()
    {
        Console.WriteLine($"[{EntityName}] HandlePostSuccessRedirect - X-Requested-With: {Request.Headers.XRequestedWith}, X-ArchiX-Tab: {Request.Headers["X-ArchiX-Tab"]}");
        
        // Accordion içinden çağrılmışsa sadece 200 OK dön (frontend reload yapacak)
        if (Request.Headers.XRequestedWith == "XMLHttpRequest" || 
            Request.Headers["X-ArchiX-Tab"] == "1")
        {
            Console.WriteLine($"[{EntityName}] AJAX request - OkResult döndürülüyor");
            return new OkResult();
        }

        Console.WriteLine($"[{EntityName}] Normal request - RedirectToPage: {ListPageUrl}");
        return RedirectToPage(ListPageUrl);
    }
}
