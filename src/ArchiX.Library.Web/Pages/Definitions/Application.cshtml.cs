using ArchiX.Library.Context;
using ArchiX.Library.Web.Pages.Shared;
using ArchiX.Library.Web.ViewModels.Grid;

namespace ArchiX.Library.Web.Pages.Definitions;

public class ApplicationModel : EntityListPageBase<Library.Entities.Application>
{
    public ApplicationModel(AppDbContext db) : base(db) { }

    protected override string EntityName => "Application";
    protected override string RecordEndpoint => "/Definitions/Application/Record";
    protected override string GridId => "appgrid"; // Override: "applicationgrid" yerine "appgrid"

    protected override List<GridColumnDefinition> GetColumns() => new()
    {
        new("Id", "ID", Width: "80px"),
        new("Code", "Kod", Width: "150px"),
        new("Name", "Ad", Width: "200px"),
        new("DefaultCulture", "Dil", Width: "100px"),
        new("StatusId", "Durum", Width: "100px")
    };

    protected override Dictionary<string, object?> EntityToRow(Library.Entities.Application entity) => new()
    {
        ["Id"] = entity.Id,
        ["Code"] = entity.Code,
        ["Name"] = entity.Name,
        ["DefaultCulture"] = entity.DefaultCulture,
        ["StatusId"] = entity.StatusId
    };

    // Application'a özel: ID=1 silinemez
    protected override bool ShouldHideDeleteButton(Library.Entities.Application entity)
    {
        return entity.Id == 1;
    }
}

