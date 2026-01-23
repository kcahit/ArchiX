using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Web.ViewModels.Definitions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHost.Pages.Definitions;

public class ApplicationRecordModel : PageModel
{
    private readonly AppDbContext _db;

    public ApplicationRecordModel(AppDbContext db)
    {
        _db = db;
    }

    public bool IsNew { get; set; }
    public Application? Application { get; set; }
    public ApplicationFormModel? Form { get; set; }

    public async Task OnGetAsync([FromQuery] int? id, CancellationToken ct)
    {
        IsNew = id == null || id == 0;

        if (!IsNew)
        {
            Application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id, ct);

            if (Application == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Form = new ApplicationFormModel
            {
                Code = Application.Code,
                Name = Application.Name,
                DefaultCulture = Application.DefaultCulture,
                TimeZoneId = Application.TimeZoneId,
                Description = Application.Description
            };
        }
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm] ApplicationFormModel form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var app = new Application
        {
            Code = form.Code,
            Name = form.Name,
            DefaultCulture = form.DefaultCulture,
            TimeZoneId = form.TimeZoneId,
            Description = form.Description
        };

        app.MarkCreated(userId: 1); // TODO: gerçek userId
        _db.Applications.Add(app);
        await _db.SaveChangesAsync(ct);

        return RedirectToPage("/Definitions/Application");
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, [FromForm] ApplicationFormModel form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app == null) return NotFound();

        app.Code = form.Code;
        app.Name = form.Name;
        app.DefaultCulture = form.DefaultCulture;
        app.TimeZoneId = form.TimeZoneId;
        app.Description = form.Description;

        app.MarkUpdated(userId: 1); // TODO: gerçek userId

        await _db.SaveChangesAsync(ct);
        return RedirectToPage("/Definitions/Application");
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, CancellationToken ct)
    {
        if (id == 1)
            throw new InvalidOperationException("System application cannot be deleted.");

        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app == null) return NotFound();

        app.SoftDelete(userId: 1); // TODO: gerçek userId
        await _db.SaveChangesAsync(ct);

        return RedirectToPage("/Definitions/Application");
    }
}
