using ArchiX.Library.Context;
using ArchiX.Library.Web.Pages.Shared;
using ArchiX.Library.Web.ViewModels.Definitions;

namespace ArchiX.WebHost.Pages.Definitions.Application;

public class RecordModel : EntityRecordPageBase<Library.Entities.Application, ApplicationFormModel>
{
    public RecordModel(AppDbContext db) : base(db) { }

    protected override string EntityName => "Application";
    protected override string ListPageUrl => "/Definitions/Application";

    // Convenience property for view (optional)
    public Library.Entities.Application? Application => Entity;

    protected override ApplicationFormModel EntityToForm(Library.Entities.Application entity) => new()
    {
        Code = entity.Code,
        Name = entity.Name,
        DefaultCulture = entity.DefaultCulture,
        TimeZoneId = entity.TimeZoneId,
        Description = entity.Description
    };

    protected override void ApplyFormToEntity(ApplicationFormModel form, Library.Entities.Application entity)
    {
        entity.Code = form.Code;
        entity.Name = form.Name;
        entity.DefaultCulture = form.DefaultCulture;
        entity.TimeZoneId = form.TimeZoneId;
        entity.Description = form.Description;
    }
}
