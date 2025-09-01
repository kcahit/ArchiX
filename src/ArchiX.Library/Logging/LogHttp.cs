using System.Collections.Generic;

namespace ArchiX.Library.Logging;

/// <summary>
/// HTTP isteği/yanıtı ile ilgili log bilgilerini tutar.
/// </summary>
public sealed class LogHttp
{
    // Mevcut alanlar
    /// <summary>HTTP metodu (GET, POST vb.).</summary>
    public string? Method { get; set; }

    /// <summary>İstek path bilgisi.</summary>
    public string? Path { get; set; }

    /// <summary>HTTP durum kodu.</summary>
    public int? Status { get; set; }

    /// <summary>Route şablonu (örn: /products/{id}).</summary>
    public string? Route { get; set; }

    /// <summary>Query parametreleri (anahtar=değer).</summary>
    public IDictionary<string, string?>? Query { get; set; }

    /// <summary>HTTP header bilgileri.</summary>
    public IDictionary<string, string?>? Headers { get; set; }

    /// <summary>İstemci IP adresi.</summary>
    public string? ClientIp { get; set; }

    /// <summary>İstemci user-agent değeri.</summary>
    public string? UserAgent { get; set; }

    /// <summary>RequestId (Trace sistemi tarafından atanır).</summary>
    public string? RequestId { get; set; }

    // Writer kullanımı
    /// <summary>Request body SHA-256 hash değeri.</summary>
    public string? BodyHash { get; set; }

    /// <summary>Raw body (middleware geçici doldurur, writer null yapar).</summary>
    public string? RawBody { get; set; }

    // Yeni alanlar
    /// <summary>İstek şeması (http / https).</summary>
    public string? Scheme { get; set; }

    /// <summary>İstek host değeri (örn: example.com).</summary>
    public string? Host { get; set; }

    /// <summary>İstek portu (örn: 80, 443).</summary>
    public int? Port { get; set; }

    /// <summary>Kullanılan HTTP protokolü (örn: HTTP/1.1, HTTP/2).</summary>
    public string? Protocol { get; set; }

    /// <summary>PathBase bilgisi (reverse proxy vb.).</summary>
    public string? PathBase { get; set; }

    /// <summary>Ham query string değeri (örn: ?a=1&amp;b=2).</summary>
    public string? QueryString { get; set; }

    /// <summary>Referrer (Header: Referer).</summary>
    public string? Referrer { get; set; }

    /// <summary>Origin (CORS kaynak bilgisi).</summary>
    public string? Origin { get; set; }

    /// <summary>Content-Type header değeri.</summary>
    public string? ContentType { get; set; }

    /// <summary>Content-Length header değeri.</summary>
    public long? ContentLength { get; set; }

    /// <summary>Sunucu (local) IP adresi.</summary>
    public string? LocalIp { get; set; }

    /// <summary>Sunucu (local) portu.</summary>
    public int? LocalPort { get; set; }

    /// <summary>İstemci portu.</summary>
    public int? ClientPort { get; set; }

    /// <summary>Cookie isimleri (değerler yazılmaz, PII’den kaçınmak için).</summary>
    public IReadOnlyList<string>? CookieNames { get; set; }
}
