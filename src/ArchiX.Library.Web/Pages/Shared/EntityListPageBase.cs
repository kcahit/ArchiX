using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Web.Pages.Shared;

/// <summary>
/// Generic base class for entity list pages with grid + accordion CRUD.
/// Supports soft delete, includeDeleted toggle, and GridRecordAccordion pattern.
/// </summary>
public abstract class EntityListPageBase<TEntity> : PageModel where TEntity : BaseEntity
{
    protected readonly AppDbContext Db;

    protected EntityListPageBase(AppDbContext db)
    {
        Db = db;
    }

    public GridTableViewModel Grid { get; set; } = null!;

    /// <summary>
    /// Entity name for URL/title generation (e.g., "Application", "Parameter").
    /// </summary>
    protected abstract string EntityName { get; }

    /// <summary>
    /// Record endpoint URL (e.g., "/Definitions/Application/Record").
    /// </summary>
    protected abstract string RecordEndpoint { get; }

    /// <summary>
    /// Grid ID (e.g., "appgrid", "paramgrid").
    /// </summary>
    protected virtual string GridId => $"{EntityName.ToLowerInvariant()}grid";

    /// <summary>
    /// Define grid columns (Field, Title, Width).
    /// </summary>
    protected abstract List<GridColumnDefinition> GetColumns();

    /// <summary>
    /// Convert entity to grid row dictionary.
    /// </summary>
    protected abstract Dictionary<string, object?> EntityToRow(TEntity entity);

    public async Task OnGetAsync([FromQuery] int? includeDeleted, CancellationToken ct)
    {
        var query = GetQuery();

        if (includeDeleted == 1)
        {
            query = query.IgnoreQueryFilters();
        }

        var entities = await query.ToListAsync(ct);

        var columns = GetColumns();
        var rows = entities.Select(EntityToRow).ToList();

        Grid = new GridTableViewModel
        {
            Id = GridId,
            Columns = columns,
            Rows = rows,
            ShowActions = true,
            ShowToolbar = true,
            Toolbar = new GridToolbarViewModel
            {
                TotalRecords = entities.Count,
                IsFormOpenEnabled = 1,
                RecordEndpoint = RecordEndpoint,
                ShowDeletedToggle = true
            }
        };
    }

    /// <summary>
    /// Override to customize query (e.g., Include, Where).
    /// </summary>
    protected virtual IQueryable<TEntity> GetQuery()
    {
        return Db.Set<TEntity>().AsQueryable();
    }
}
