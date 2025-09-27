// File: src/ArchiX.Library/Infrastructure/Http/HttpApiProblem.cs
#nullable enable
using System.Text.Json.Serialization;

namespace ArchiX.Library.Infrastructure.Http;

/// <summary>RFC7807 uyumlu problem ayrıntısı.</summary>
/// <param name="Type">Problem türü URI’si.</param>
/// <param name="Title">Kısa başlık.</param>
/// <param name="Status">HTTP durum kodu.</param>
/// <param name="Detail">Ayrıntı.</param>
/// <param name="Instance">Örnek URI’si.</param>
/// <param name="Extensions">Ek alanlar.</param>
public sealed record HttpApiProblem(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("detail")] string? Detail,
    [property: JsonPropertyName("instance")] string? Instance,
    IDictionary<string, object?>? Extensions = null)
{
    /// <summary>Özet metin.</summary>
    public override string ToString()
    {
        var code = Status is { } s ? s.ToString() : "?";
        return $"ProblemDetails[{code}] {Title ?? "(no title)"} - {Detail ?? "(no detail)"}";
    }
}
