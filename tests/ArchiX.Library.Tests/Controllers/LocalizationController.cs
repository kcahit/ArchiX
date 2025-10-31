using ArchiX.Library.LanguagePacks;

using Microsoft.AspNetCore.Mvc;

namespace ArchiXTest.ApiWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LocalizationController(ILanguageService lang) : ControllerBase
{
    private readonly ILanguageService _lang = lang;

    // GET /api/localization/display-name?itemType=...&entityName=...&fieldName=...&code=...&culture=tr-TR
    [HttpGet("display-name")]
    public async Task<ActionResult<string>> GetDisplayName(
        [FromQuery] string itemType,
        [FromQuery] string entityName,
        [FromQuery] string fieldName,
        [FromQuery] string code,
        [FromQuery] string culture,
        CancellationToken ct)
    {
        var name = await _lang.GetDisplayNameAsync(itemType, entityName, fieldName, code, culture, ct);
        return name is null ? NotFound() : Ok(name);
    }

    public sealed record DisplayItem(int Id, string DisplayName);

    // GET /api/localization/list?itemType=...&entityName=...&fieldName=...&culture=tr-TR
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<DisplayItem>>> GetList(
        [FromQuery] string itemType,
        [FromQuery] string entityName,
        [FromQuery] string fieldName,
        [FromQuery] string culture,
        CancellationToken ct)
    {
        var pairs = await _lang.GetListAsync(itemType, entityName, fieldName, culture, ct);
        var result = pairs.Select(p => new DisplayItem(p.Id, p.DisplayName));
        return Ok(result);
    }
}
