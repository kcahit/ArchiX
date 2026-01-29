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
        ValidateValueAgainstType();
        return await base.OnPostCreateAsync(ct);
    }

    public override async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, CancellationToken ct)
    {
        await LoadDataTypesAsync(ct);
        ValidateValueAgainstType();
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

    private void ValidateValueAgainstType()
    {
        var dt = DataTypes.FirstOrDefault(x => x.Id == Form.ParameterDataTypeId);
        if (dt is null) return;

        var code = dt.Code;
        var name = dt.Name.ToLowerInvariant();
        var value = Form.Value;

        // Allow null/empty -> no validation (optional field)
        if (string.IsNullOrWhiteSpace(value)) return;

        bool AddError(string message)
        {
            ModelState.AddModelError("Form.Value", message);
            return false;
        }

        bool ParseInt(long min, long max)
        {
            if (!long.TryParse(value, out var v)) return AddError($"{dt.Name} için sayısal değer girin.");
            if (v < min || v > max) return AddError($"{dt.Name} aralığı {min}..{max}.");
            return true;
        }

        bool ParseDecimal(int precision, int scale)
        {
            if (!decimal.TryParse(value, out var d)) return AddError($"{dt.Name} için ondalık değer girin.");
            var parts = value.Split('.');
            var integerDigits = parts[0].TrimStart('-').Length;
            var fracDigits = parts.Length > 1 ? parts[1].Length : 0;
            if (integerDigits + fracDigits > precision || fracDigits > scale)
                return AddError($"{dt.Name} en fazla {precision} basamak, {scale} ondalık.");
            return true;
        }

        if (code == 200) { ParseInt(0, 255); return; }
        if (code == 210) { ParseInt(short.MinValue, short.MaxValue); return; }
        if (code == 220) { ParseInt(int.MinValue, int.MaxValue); return; }
        if (code == 230) { ParseInt(long.MinValue, long.MaxValue); return; }
        if (code == 240) { ParseDecimal(18, 6); return; }

        if (code >= 300 && code < 400)
        {
            if (code == 300 || name.Contains("date"))
            {
                if (!DateTime.TryParse(value, out _)) AddError("Geçerli bir tarih girin (gg.aa.yyyy).");
                return;
            }
            if (code == 310 || name.Contains("time"))
            {
                if (!TimeSpan.TryParse(value, out _)) AddError("Geçerli bir saat girin (hh:mm:ss).");
                return;
            }
            if (!DateTime.TryParse(value, out _)) AddError("Geçerli bir tarih-saat girin.");
            return;
        }

        if (name.Contains("bool") || code == 900)
        {
            var normalized = value.ToLowerInvariant();
            if (normalized is not ("true" or "false" or "1" or "0" or "yes" or "no"))
            {
                AddError("Bool için Evet/Hayır seçin.");
            }
            else
            {
                Form.Value = normalized is "true" or "1" or "yes" ? "true" : "false";
            }
        }
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
