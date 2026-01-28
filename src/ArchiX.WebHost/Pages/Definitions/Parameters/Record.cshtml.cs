using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Web.Pages.Shared;
using ArchiX.Library.Web.ViewModels.Definitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHost.Pages.Definitions.Parameters;

public class RecordModel : EntityRecordPageBase<Parameter, ParameterFormModel>
{
    public RecordModel(AppDbContext db) : base(db) { }

    protected override string EntityName => "Parameter";
    protected override string ListPageUrl => "/Definitions/Parameters";

    public Parameter? Parameter => Entity;

    public List<ParameterDataTypeOption> DataTypes { get; private set; } = [];

    public override async Task OnGetAsync([FromQuery] int? id, CancellationToken ct)
    {
        await LoadDataTypesAsync(ct);
        await base.OnGetAsync(id, ct);
    }

    public override async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        await LoadDataTypesAsync(ct);
        return await base.OnPostCreateAsync(ct);
    }

    public override async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, CancellationToken ct)
    {
        await LoadDataTypesAsync(ct);
        return await base.OnPostUpdateAsync(id, ct);
    }

    protected override ParameterFormModel EntityToForm(Parameter entity) => new()
    {
        Group = entity.Group,
        Key = entity.Key,
        ParameterDataTypeId = entity.ParameterDataTypeId,
        Description = entity.Description,
        Value = entity.Value
    };

    protected override void ApplyFormToEntity(ParameterFormModel form, Parameter entity)
    {
        entity.Group = form.Group.Trim();
        entity.Key = form.Key.Trim();
        entity.ParameterDataTypeId = form.ParameterDataTypeId;
        entity.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
        entity.Value = string.IsNullOrWhiteSpace(form.Value) ? null : form.Value.Trim();
    }

    private async Task LoadDataTypesAsync(CancellationToken ct)
    {
        DataTypes = await Db.ParameterDataTypes
            .OrderBy(dt => dt.Code)
            .ThenBy(dt => dt.Name)
            .Select(dt => new ParameterDataTypeOption
            {
                Id = dt.Id,
                Name = dt.Name,
                Category = dt.Category,
                Code = dt.Code
            })
            .ToListAsync(ct);
    }

    public sealed class ParameterDataTypeOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Category { get; init; }
        public int Code { get; init; }
    }
}
