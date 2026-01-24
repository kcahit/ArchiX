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
        if (!ModelState.IsValid)
            return Page();

        var entity = new TEntity();
        ApplyFormToEntity(Form, entity);

        // TODO: Get real userId from HttpContext.User
        entity.MarkCreated(userId: 1);
        Db.Set<TEntity>().Add(entity);
        await Db.SaveChangesAsync(ct);

        return HandlePostSuccessRedirect();
    }

    public virtual async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var entity = await Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity == null) return NotFound();

        ApplyFormToEntity(Form, entity);

        // TODO: Get real userId from HttpContext.User
        entity.MarkUpdated(userId: 1);

        await Db.SaveChangesAsync(ct);
        return HandlePostSuccessRedirect();
    }

    public virtual async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, CancellationToken ct)
    {
        if (id == 1)
        {
            ModelState.AddModelError(string.Empty, $"Sistem kaydı (ID=1) silinemez.");
            await OnGetAsync(id, ct);
            return Page();
        }

        var entity = await Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity == null) return NotFound();

        // TODO: Get real userId from HttpContext.User
        entity.SoftDelete(userId: 1);
        await Db.SaveChangesAsync(ct);

        return HandlePostSuccessRedirect();
    }

    /// <summary>
    /// Handle post-submit redirect (accordion or normal navigation).
    /// </summary>
    protected virtual IActionResult HandlePostSuccessRedirect()
    {
        // Accordion içinden çağrılmışsa parent'ı refresh et
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            Response.Headers["X-ArchiX-Reload-Parent"] = "1";
            return Content("<script>if(window.parent){window.parent.location.reload();}</script>", "text/html");
        }

        return RedirectToPage(ListPageUrl);
    }
}
