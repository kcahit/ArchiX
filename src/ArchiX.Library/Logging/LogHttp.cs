using System.Collections.Generic;

namespace ArchiX.Library.Logging;

public sealed class LogHttp
{
    // Mevcut alanlar (korundu)
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int? Status { get; set; }
    public string? Route { get; set; }
    public IDictionary<string, string?>? Query { get; set; }
    public IDictionary<string, string?>? Headers { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestId { get; set; }

    // Writer kullanımı
    public string? BodyHash { get; set; }   // JsonlLogWriter doldurur
    public string? RawBody { get; set; }    // Middleware geçici doldurur; writer null yapar

    // Yeni alanlar (tamamı nullable → kırılma yok)
    public string? Scheme { get; set; }         // http / https
    public string? Host { get; set; }           // example.com
    public int? Port { get; set; }              // 80 / 443 / özel
    public string? Protocol { get; set; }       // HTTP/1.1, HTTP/2
    public string? PathBase { get; set; }       // reverse proxy vb.
    public string? QueryString { get; set; }    // ham "?a=1&b=2"
    public string? Referrer { get; set; }       // Header: Referer
    public string? Origin { get; set; }         // CORS
    public string? ContentType { get; set; }    // Request content-type
    public long? ContentLength { get; set; }    // Request content-length
    public string? LocalIp { get; set; }        // Server IP
    public int? LocalPort { get; set; }         // Server Port
    public int? ClientPort { get; set; }        // İstemci Port
    public IReadOnlyList<string>? CookieNames { get; set; } // Sadece isimler (PII’den kaçın)
}
