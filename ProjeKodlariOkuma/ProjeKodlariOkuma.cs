using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ProjeKodlariOkuma
{
    /*
        * Kök dizine git
        * Uzantılara göre dosyaları bul
        * Her dosyayı oku
        * Proje adı, dizin, dosya, içerik metadatasını topla
        * JSON ya da NDJSON olarak yaz
        * Türkçe karakterler bozulmasın, kaçışlar doğru olsun, TAB vb. korunacak
    */

    internal static class DosyaTarayici
    {
        private sealed record Kayit(string Proje, string Dizin, string Dosya, string Icerik);

        internal static void DosyaOlustur(
            string kokDizin,
            string[] uzantilar,
            string hedefDizin,
            string dosyaAdi,
            string format,
            int islem)
        {
            ArgumentException.ThrowIfNullOrEmpty(kokDizin);
            ArgumentNullException.ThrowIfNull(uzantilar);
            ArgumentException.ThrowIfNullOrEmpty(hedefDizin);
            ArgumentException.ThrowIfNullOrEmpty(dosyaAdi);
            ArgumentException.ThrowIfNullOrEmpty(format);

            Console.WriteLine("İşlem: " + islem);

            bool uretTree;
            bool uretJson;
            switch (islem)
            {
                case 1: uretTree = true; uretJson = false; break;
                case 2: uretTree = false; uretJson = true; break;
                case 3: uretTree = true; uretJson = true; break;
                default:
                    Console.WriteLine("Uyarı: Geçersiz islem parametresi: " + islem);
                    Console.ReadLine();
                    return;
            }

            if (!Directory.Exists(kokDizin))
            {
                Console.WriteLine("Uyarı: Dizin bulunamadı: " + kokDizin);
                Console.ReadLine();
                return;
            }

            var extSet = uzantilar
                .Where(static s => !string.IsNullOrWhiteSpace(s))
                .Select(static s =>
                {
                    var t = s.Trim();
                    var lower = t.ToLowerInvariant();
                    return lower.StartsWith('.') ? lower : "." + lower;
                })
                .ToHashSet(StringComparer.OrdinalIgnoreCase);


            Console.WriteLine("Kök Dizin: " + kokDizin);
            Console.WriteLine("Uzantılar: " + string.Join(", ", extSet));
            Console.WriteLine("Taramaya başlanıyor...");

            var paths = Directory.EnumerateFiles(kokDizin, "*.*", SearchOption.AllDirectories)
                                 .Where(p => extSet.Contains(Path.GetExtension(p)))
                                 .ToArray();

            foreach (var p in paths)
            {
                var rel = Path.GetRelativePath(kokDizin, p).Replace('\\', '/');
                Console.WriteLine(rel);
            }
            Console.WriteLine("Toplam dosya: " + paths.Length);

            Directory.CreateDirectory(hedefDizin);

            // Kayıtları hazırla
            var kayitlar = new List<Kayit>(paths.Length);
            var relPathList = new List<string>(paths.Length);
            foreach (var path in paths)
            {
                var relPath = Path.GetRelativePath(kokDizin, path).Replace('\\', '/');
                var dizin = Path.GetDirectoryName(relPath)?.Replace('\\', '/') ?? string.Empty;
                var dosya = Path.GetFileName(path);
                var icerik = ReadAllTextUtf8(path);
                var proje = FindNearestProjectName(path) ?? InferTopFolderAsProject(kokDizin, path);

                kayitlar.Add(new Kayit(proje, dizin, dosya, icerik));
                relPathList.Add(relPath);
            }

            // JSON / NDJSON
            var fmt = format.Trim().ToLowerInvariant();
            if (uretJson)
            {
                if (fmt != "json" && fmt != "ndjson")
                    throw new ArgumentOutOfRangeException(nameof(format), "json veya ndjson olmalı.");

                var outPath = Path.Combine(hedefDizin, $"{dosyaAdi}.{fmt}");
                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };

                using var fs = File.Create(outPath);
                using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                if (fmt == "json")
                {
                    sw.Write(JsonSerializer.Serialize(kayitlar, jsonOptions));
                }
                else
                {
                    foreach (var k in kayitlar)
                        sw.WriteLine(JsonSerializer.Serialize(k, jsonOptions));
                }

                Console.WriteLine("JSON çıktı: " + outPath);
            }

            // TreeView
            if (uretTree)
            {
                var pathListFile = Path.Combine(hedefDizin, dosyaAdi + "_paths.txt");
                File.WriteAllLines(pathListFile, relPathList, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                var treeOut = Path.Combine(hedefDizin, dosyaAdi + "_treeview.txt");
                TreeViewOlustur.Olustur(pathListFile, treeOut);

                Console.WriteLine("TreeView çıktı: " + treeOut);
            }

            Console.WriteLine("Beklemede. Enter...");
            Console.ReadLine();
        }

        private static string ReadAllTextUtf8(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return sr.ReadToEnd();
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
}
