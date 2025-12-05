using System.Text.Json;

using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Varsayılan versiyon yükseltme stratejisi.
/// Şu an sadece v1 destekleniyor; ileride v2, v3 için chain eklenebilir.
/// </summary>
public sealed class PasswordPolicyVersionUpgrader : IPasswordPolicyVersionUpgrader
{
    private readonly ILogger<PasswordPolicyVersionUpgrader> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public PasswordPolicyVersionUpgrader(ILogger<PasswordPolicyVersionUpgrader> logger)
    {
        _logger = logger;
    }

    public PasswordPolicyOptions UpgradeIfNeeded(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON boş olamaz.", nameof(json));

        // Önce version alanını oku
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var version = root.TryGetProperty("version", out var vProp) && vProp.ValueKind == JsonValueKind.Number
            ? vProp.GetInt32()
            : 1; // Varsayılan v1

        if (version == 1)
        {
            // v1 → doğrudan deserialize
            var model = JsonSerializer.Deserialize<PasswordPolicyOptions>(json, JsonOpts)
                ?? throw new InvalidOperationException("PasswordPolicy deserialization başarısız.");

            _logger.LogDebug("PasswordPolicy v1 formatı yüklendi.");
            return model;
        }

        if (version == 2)
        {
            // v2 → v1'e dönüştür (örnek: yeni alanlar eklenmiş olabilir, eski model uyumlu hale getir)
            _logger.LogInformation("PasswordPolicy v2 formatı algılandı, v1'e dönüştürülüyor.");
            var upgraded = UpgradeV2ToV1(root);
            return upgraded;
        }

        throw new NotSupportedException($"PasswordPolicy version {version} desteklenmiyor.");
    }

    private PasswordPolicyOptions UpgradeV2ToV1(JsonElement v2Root)
    {
        // Örnek: v2'de yeni alanlar var, ama v1 modeline fit etmek için haritalama yapıyoruz
        // Şu an v2 yok, ama gelecek için placeholder
        var v1Json = v2Root.GetRawText(); // Basit örnek: aynı JSON'u v1 olarak parse et
        var model = JsonSerializer.Deserialize<PasswordPolicyOptions>(v1Json, JsonOpts)
            ?? throw new InvalidOperationException("v2 → v1 dönüşümü başarısız.");

        _logger.LogInformation("v2 → v1 dönüşümü tamamlandı.");
        return model;
    }
}
