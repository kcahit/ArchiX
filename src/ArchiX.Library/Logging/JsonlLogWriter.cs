using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiX.Library.Logging
{
    /// <summary>
    /// JSONL (JSON Lines) formatında log dosyası yazıcısı.
    /// Log kayıtlarını dosyalara yazar, parçalama (split) ve retention yönetimi yapar.
    /// </summary>
    public sealed class JsonlLogWriter
    {
        private readonly LoggingOptions _options;
        private readonly JsonWriterOptions _jsonWriterOptions;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Hassas kabul edilen key listesi. Bu alanlar loglarda maskelenir.
        /// </summary>
        private static readonly string[] SensitiveKeys =
        {
            "password", "token", "authorization", "apiKey", "apikey", "email"
        };

        /// <summary>
        /// Yeni bir <see cref="JsonlLogWriter"/> örneği oluşturur.
        /// </summary>
        /// <param name="options">Loglama ayarları.</param>
        /// <exception cref="ArgumentNullException">Options null ise fırlatılır.</exception>
        public JsonlLogWriter(LoggingOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _jsonWriterOptions = new JsonWriterOptions
            {
                Indented = false,
                SkipValidation = false
            };

            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            Directory.CreateDirectory(_options.BasePath);
        }

        /// <summary>
        /// Belirtilen log kaydını asenkron olarak dosyaya yazar.
        /// </summary>
        /// <param name="record">Log kaydı.</param>
        /// <param name="ct">İptal token.</param>
        public async Task WriteAsync(LogRecord record, CancellationToken ct = default)
        {
            if (record is null) return;

            // Body hash + PII maskesi
            if (!string.IsNullOrEmpty(record.Http?.RawBody))
            {
                record.Http!.BodyHash = ComputeSha256(record.Http.RawBody);
                record.Http.RawBody = null;
            }
            MaskSensitive(record.Http?.Query);
            MaskSensitive(record.Http?.Headers);

            // 1) Aktif dosyayı bul
            var (activePath, part) = GetActiveFilePath();

            // 2) Boyut kontrolü (dosya max sınırı aşıldıysa yeni parça aç)
            var maxBytes = _options.GetMaxFileSizeBytes();
            var preSize = File.Exists(activePath) ? new FileInfo(activePath).Length : 0L;
            if (preSize > maxBytes)
            {
                part++;
                activePath = GetPartPath(part);
            }

            // 3) Tek satırlık JSON kaydı
            await using (var stream = new FileStream(activePath, FileMode.Append, FileAccess.Write, FileShare.Read, 64 * 1024, useAsync: true))
            await using (var writer = new Utf8JsonWriter(stream, _jsonWriterOptions))
            {
                JsonSerializer.Serialize(writer, record, _serializerOptions);
                await writer.FlushAsync(ct).ConfigureAwait(false);
                await stream.WriteAsync("\n"u8.ToArray(), ct).ConfigureAwait(false);
            }

            // 4) Retention (opsiyonel)
            _ = Task.Run(() => TryApplyRetentionSafe(), CancellationToken.None);
        }

        /// <summary>
        /// Güncel log dosyası yolunu ve parça numarasını döndürür.
        /// </summary>
        private (string path, int part) GetActiveFilePath()
        {
            var basePath = GetPartPath(1); // errors.jsonl
            var baseName = Path.GetFileNameWithoutExtension(basePath); // errors
            var dir = Path.GetDirectoryName(basePath)!;

            var pattern = $"{baseName}*.jsonl";
            var all = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);

            if (all.Length == 0)
                return (basePath, 1);

            int MaxPart(string fileFullPath)
            {
                var name = Path.GetFileNameWithoutExtension(fileFullPath);
                if (name.Equals(baseName, StringComparison.OrdinalIgnoreCase))
                    return 1;

                var idx = name.LastIndexOf("_part", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return 1;
                var numStr = name[(idx + "_part".Length)..];
                return int.TryParse(numStr, out var n) && n > 1 ? n : 1;
            }

            var maxPart = 1;
            var active = basePath;

            foreach (var f in all)
            {
                var p = MaxPart(f);
                if (p > maxPart)
                {
                    maxPart = p;
                    active = f;
                }
                else if (p == 1 && maxPart == 1)
                {
                    var fiCandidate = new FileInfo(f);
                    var fiCurrent = new FileInfo(active);
                    if (fiCandidate.LastWriteTimeUtc > fiCurrent.LastWriteTimeUtc)
                        active = f;
                }
            }

            return (active, maxPart);
        }

        /// <summary>
        /// Belirtilen parça numarasına göre dosya yolunu döndürür.
        /// </summary>
        private string GetPartPath(int part)
        {
            var prefix = _options.DailyFilePrefix; // "errors"
            var name = part <= 1 ? $"{prefix}.jsonl" : $"{prefix}_part{part}.jsonl";
            return Path.Combine(_options.BasePath, name);
        }

        /// <summary>
        /// Retention politikası uygular: Eski log dosyalarını siler.
        /// </summary>
        private void TryApplyRetentionSafe()
        {
            try
            {
                var dir = new DirectoryInfo(_options.BasePath);
                if (!dir.Exists) return;

                var threshold = DateTime.Now.AddDays(-_options.RetainDays);
                foreach (var fi in dir.EnumerateFiles($"{_options.DailyFilePrefix}*.jsonl"))
                {
                    if (fi.LastWriteTime < threshold)
                    {
                        try { fi.Delete(); } catch { }
                    }
                }
            }
            catch
            {
                // Retention hatasını yut
            }
        }

        /// <summary>
        /// Parametre çantasındaki hassas verileri maskeler.
        /// </summary>
        private static void MaskSensitive(IDictionary<string, string?>? bag)
        {
            if (bag is null) return;
            foreach (var key in bag.Keys.ToList())
            {
                if (IsSensitive(key))
                {
                    bag[key] = "***";
                }
            }
        }

        /// <summary>
        /// Anahtarın hassas olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsSensitive(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            var lower = key.Trim().ToLowerInvariant();
            return SensitiveKeys.Any(s => lower.Contains(s));
        }

        /// <summary>
        /// Verilen string için SHA-256 hash değeri üretir.
        /// </summary>
        /// <param name="input">Hash alınacak string.</param>
        /// <returns>SHA-256 hex string.</returns>
        public static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
