// File: src/ArchiX.Library/Infrastructure/Http/DefaultHttpClientWrapper.cs
using System.Text.Json;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// Varsayılan HttpClient wrapper implementasyonu.
    /// Ortak davranışlar <see cref="HttpClientWrapperBase"/> içindedir.
    /// </summary>
    /// <param name="client">Kullanılacak <see cref="HttpClient"/> örneği.</param>
    /// <param name="jsonOptions">JSON serileştirme seçenekleri.</param>
    public sealed class DefaultHttpClientWrapper(HttpClient client, JsonSerializerOptions jsonOptions)
        : HttpClientWrapperBase(client, jsonOptions), IHttpClientWrapper
    {
    }
}
