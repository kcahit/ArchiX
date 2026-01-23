using ArchiX.Library.Context;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHost.Pages.Definitions;

public class ApplicationModel : PageModel
{
    private readonly AppDbContext _db;

    public ApplicationModel(AppDbContext db)
    {
        _db = db;
    }

    public GridTableViewModel Grid { get; set; } = null!;

    public async Task OnGetAsync([FromQuery] int? includeDeleted, CancellationToken ct)
    {
        var query = _db.Applications.AsQueryable();

        if (includeDeleted == 1)
        {
            query = query.IgnoreQueryFilters();
        }

        var applications = await query.ToListAsync(ct);

        var columns = new List<GridColumnDefinition>
        {
            new("Id", "ID", Width: "80px"),
            new("Code", "Kod", Width: "150px"),
            new("Name", "Ad", Width: "200px"),
            new("DefaultCulture", "Dil", Width: "100px"),
            new("StatusId", "Durum", Width: "100px")
        };

        var rows = applications.Select(a => new Dictionary<string, object?>
        {
            ["Id"] = a.Id,
            ["Code"] = a.Code,
            ["Name"] = a.Name,
            ["DefaultCulture"] = a.DefaultCulture,
            ["StatusId"] = a.StatusId
        }).ToList();

        Grid = new GridTableViewModel
        {
            Id = "appgrid",
            Columns = columns,
            Rows = rows,
            ShowActions = true,
            ShowToolbar = true,
            Toolbar = new GridToolbarViewModel
            {
                TotalRecords = applications.Count,
                IsFormOpenEnabled = 1,
                RecordEndpoint = "/Definitions/Application/Record",
                ShowDeletedToggle = true
            }
        };
    }
}
