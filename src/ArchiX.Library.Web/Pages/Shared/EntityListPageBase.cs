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

    /// <summary>
    /// Check if delete button should be hidden for specific entity (e.g., ApplicationId=1).
    /// Override in derived class for entity-specific rules.
    /// </summary>
    protected virtual bool ShouldHideDeleteButton(TEntity entity)
    {
        return false; // Default: show delete button for all
    }

    public async Task OnGetAsync([FromQuery] int? includeDeleted, CancellationToken ct)
    {
        try
        {
            var query = GetQuery();

            if (includeDeleted == 1)
            {
                query = query.IgnoreQueryFilters();
            }

            var entities = await query.ToListAsync(ct);

            var columns = GetColumns();
            var rows = entities.Select(EntityToRow).ToList();

            // Entity-specific delete button hiding (e.g., ApplicationId=1)
            var hideDeleteForIds = entities.Where(ShouldHideDeleteButton).Select(e => e.Id).ToList();

            Grid = new GridTableViewModel
            {
                Id = GridId,
                Columns = columns,
                Rows = rows,
                ShowActions = true,
                ShowToolbar = true,
                HideDeleteForIds = hideDeleteForIds,
                Toolbar = new GridToolbarViewModel
                {
                    TotalRecords = entities.Count,
                    IsFormOpenEnabled = 1,
                    RecordEndpoint = RecordEndpoint,
                    ShowDeletedToggle = true
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{EntityName}] OnGetAsync EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            
            // Boş grid göster (kullanıcıya exception gösterme)
            Grid = new GridTableViewModel
            {
                Id = GridId,
                Columns = GetColumns(),
                Rows = new List<Dictionary<string, object?>>(),
                ShowActions = true,
                ShowToolbar = true,
                HideDeleteForIds = new List<int>(),
                Toolbar = new GridToolbarViewModel
                {
                    TotalRecords = 0,
                    IsFormOpenEnabled = 1,
                    RecordEndpoint = RecordEndpoint,
                    ShowDeletedToggle = true
                }
            };
            
            // ModelState'e hata ekle (kullanıcı görecek)
            ModelState.AddModelError(string.Empty, "Veriler yüklenirken bir hata oluştu. Lütfen sayfayı yenileyin.");
        }
    }

    /// <summary>
    /// Override to customize query (e.g., Include, Where).
    /// Default behavior: Exclude soft-deleted records (StatusId != 6).
    /// </summary>
    protected virtual IQueryable<TEntity> GetQuery()
    {
        // BaseEntity'de StatusId property'si var
        // Soft delete: StatusId = 6 (Deleted)
        return Db.Set<TEntity>().Where(e => e.StatusId != 6);
    }
}
