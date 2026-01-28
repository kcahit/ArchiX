using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Web.Pages.Shared;
using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Web.Pages.Definitions;

public class ParametersModel : EntityListPageBase<Parameter>
{
    public ParametersModel(AppDbContext db) : base(db) { }

    protected override string EntityName => "Parameter";
    protected override string RecordEndpoint => "/Definitions/Parameters/Record";
    protected override string GridId => "paramgrid";

    protected override List<GridColumnDefinition> GetColumns() => new()
    {
        new("Id", "ID", Width: "80px"),
        new("Group", "Grup", Width: "150px"),
        new("Key", "Anahtar", Width: "220px"),
        new("DataType", "Veri Tipi", Width: "180px"),
        new("Description", "Açıklama", Width: "220px"),
        new("Value", "Değer", Width: "220px")
    };

    protected override Dictionary<string, object?> EntityToRow(Parameter entity) => new()
    {
        ["Id"] = entity.Id,
        ["Group"] = entity.Group,
        ["Key"] = entity.Key,
        ["DataType"] = entity.DataType?.Name ?? entity.ParameterDataTypeId.ToString(),
        ["Description"] = entity.Description,
        ["Value"] = entity.Value
    };

    protected override IQueryable<Parameter> GetQuery()
    {
        return base.GetQuery()
            .Include(p => p.DataType)
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Key);
    }
}
