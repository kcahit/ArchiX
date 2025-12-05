using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ProjeKodlariOkuma;

//public sealed record Kayit(string Proje, string Dizin, string Dosya, string Icerik);

public sealed class JsonDosyaUret
{
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
            var outPath = Path.Combine(hedefDizin, $"{timestamp}_{dosyaAdi}_{grup.Key}.json");
            using var fs = File.Create(outPath);
            using var sw = new StreamWriter(fs, new UTF8Encoding(false));

            if (fmt == "ndjson")
            {
                foreach (var k in grup)
                    sw.WriteLine(JsonSerializer.Serialize(k, jsonOptions));
            }
            else
            {
                sw.Write(JsonSerializer.Serialize(grup.ToList(), jsonOptions));
            }

            Console.WriteLine("JSON çıktı: " + outPath);
        }
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
