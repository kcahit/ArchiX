using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Web.ViewModels.Definitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHost.Pages.Definitions.Application;

public class RecordModel : PageModel
{
    private readonly AppDbContext _db;

    public RecordModel(AppDbContext db)
    {
        _db = db;
    }

    public bool IsNew { get; set; }

    public Library.Entities.Application? Application { get; set; }

    [BindProperty]
    public ApplicationFormModel Form { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? id, CancellationToken ct)
    {
        IsNew = id == null || id == 0;

        if (!IsNew)
        {
            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id, ct);

            if (app == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Application = app;
            Form = new ApplicationFormModel
            {
                Code = app.Code,
                Name = app.Name,
                DefaultCulture = app.DefaultCulture,
                TimeZoneId = app.TimeZoneId,
                Description = app.Description
            };
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var app = new Library.Entities.Application
        {
            Code = Form.Code,
            Name = Form.Name,
            DefaultCulture = Form.DefaultCulture,
            TimeZoneId = Form.TimeZoneId,
            Description = Form.Description
        };

        // Audit: TODO - gerçek userId alınmalı
        app.MarkCreated(userId: 1);
        _db.Applications.Add(app);
        await _db.SaveChangesAsync(ct);

        // Accordion içinden çağrılmışsa parent'ı refresh et
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            Response.Headers["X-ArchiX-Reload-Parent"] = "1";
            return Content("<script>if(window.parent){window.parent.location.reload();}</script>", "text/html");
        }

        // Liste sayfasına dön
        return RedirectToPage("/Definitions/Application");
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app == null) return NotFound();

        app.Code = Form.Code;
        app.Name = Form.Name;
        app.DefaultCulture = Form.DefaultCulture;
        app.TimeZoneId = Form.TimeZoneId;
        app.Description = Form.Description;

        // Audit: TODO - gerçek userId alınmalı
        app.MarkUpdated(userId: 1);

        await _db.SaveChangesAsync(ct);

        // Accordion içinden çağrılmışsa parent'ı refresh et
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            Response.Headers["X-ArchiX-Reload-Parent"] = "1";
            return Content("<script>if(window.parent){window.parent.location.reload();}</script>", "text/html");
        }

        return RedirectToPage("/Definitions/Application");
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, CancellationToken ct)
    {
        if (id == 1)
        {
            ModelState.AddModelError(string.Empty, "Sistem kaydı (ID=1) silinemez.");
            return Page();
        }

        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app == null) return NotFound();

        // Audit: TODO - gerçek userId alınmalı
        app.SoftDelete(userId: 1);
        await _db.SaveChangesAsync(ct);

        // Accordion içinden çağrılmışsa parent'ı refresh et
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            Response.Headers["X-ArchiX-Reload-Parent"] = "1";
            return Content("<script>if(window.parent){window.parent.location.reload();}</script>", "text/html");
        }

        return RedirectToPage("/Definitions/Application");
    }
}
