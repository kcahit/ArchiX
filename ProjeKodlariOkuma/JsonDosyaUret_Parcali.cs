using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ProjeKodlariOkuma;


public sealed class JsonDosyaUret_Parcali
{
    private const int MaxFileSizeBytes = 100 * 1024; // 100 KB

    public static void Uret(string kokDizin, string[] uzantilar, string hedefDizin, string dosyaAdi, string format)
    {
        Directory.CreateDirectory(hedefDizin);

        var extSet = uzantilar
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToLowerInvariant().StartsWith('.') ? s.Trim().ToLowerInvariant() : "." + s.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var paths = Directory.EnumerateFiles(kokDizin, "*.*", SearchOption.AllDirectories)
                             .Where(p => extSet.Contains(Path.GetExtension(p)))
                             .ToArray();

        var kayitlar = new List<Kayit>(paths.Length);
        foreach (var path in paths)
        {
            var relPath = Path.GetRelativePath(kokDizin, path).Replace('\\', '/');
            var dizin = Path.GetDirectoryName(relPath)?.Replace('\\', '/') ?? string.Empty;
            var dosya = Path.GetFileName(path);
            var icerik = File.ReadAllText(path, Encoding.UTF8);
            var proje = FindNearestProjectName(path) ?? InferTopFolderAsProject(kokDizin, path);

            kayitlar.Add(new Kayit(proje, dizin, dosya, icerik));
        }

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        var gruplar = kayitlar.GroupBy(k => k.Proje);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fmt = format.Trim().ToLowerInvariant();

        foreach (var grup in gruplar)
        {
            int parcaNo = 1;
            var buffer = new List<string>();
            int bufferSize = 0;

            foreach (var kayit in grup)
            {
                var json = JsonSerializer.Serialize(kayit, jsonOptions);
                var size = Encoding.UTF8.GetByteCount(json);

                // Eğer bu kayit tek başına 100 KB’tan büyükse, ayrı dosya olarak yazılır
                if (size > MaxFileSizeBytes)
                {
                    var outPath = Path.Combine(hedefDizin,
                        $"{timestamp}_{dosyaAdi}_{grup.Key}_Parca_{parcaNo:D2}.json");
                    File.WriteAllText(outPath, json, new UTF8Encoding(false));
                    Console.WriteLine("JSON çıktı: " + outPath);
                    parcaNo++;
                    continue;
                }

                // Buffer dolduysa yaz
                if (bufferSize + size > MaxFileSizeBytes && buffer.Count > 0)
                {
                    var outPath = Path.Combine(hedefDizin,
                        $"{timestamp}_{dosyaAdi}_{grup.Key}_Parca_{parcaNo:D2}.json");
                    File.WriteAllLines(outPath, buffer, new UTF8Encoding(false));
                    Console.WriteLine("JSON çıktı: " + outPath);
                    parcaNo++;
                    buffer.Clear();
                    bufferSize = 0;
                }

                buffer.Add(json);
                bufferSize += size;
            }

            // Son buffer’ı yaz
            if (buffer.Count > 0)
            {
                var outPath = Path.Combine(hedefDizin,
                    $"{timestamp}_{dosyaAdi}_{grup.Key}_Parca_{parcaNo:D2}.json");
                File.WriteAllLines(outPath, buffer, new UTF8Encoding(false));
                Console.WriteLine("JSON çıktı: " + outPath);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Cikmak icin herhangi bir tusa basin...");
        Console.ReadKey(intercept: true);
    }

    private static string? FindNearestProjectName(string filePath)
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(filePath)!);
        while (dir is not null)
        {
            var csprojs = dir.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojs.Length > 0)
                return Path.GetFileNameWithoutExtension(csprojs[0].Name);

            dir = dir.Parent;
        }
        return null;
    }

    private static string InferTopFolderAsProject(string root, string filePath)
    {
        var rel = Path.GetRelativePath(root, filePath);
        var first = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? "UnknownProject" : first!;
    }
}
