using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ProjeKodlariOkuma
{
    /*
        //     * Kök dizine git
        //     * uzantılara göre dosyaları bul
        //     * her dosyayı oku 
        //     * BUna göre Proje adı, dizin yapısı, dosya adları, kod içeriklerini al ve json formatında kaydet ya da njson formatında bir çıktı oluştur
        //     * bu kodlarda türkçe karakter sorunu olmamalı, esc karakterleri doğru işlenmeli, tab karakterleri doğru işlenmeli gibi detaylara dikkat et.
        //     * 

        //    */



    internal static class DosyaTarayici
    {
        private sealed record Kayit(
            string Proje,
            string Dizin,
            string Dosya,
            string Icerik
        );


        internal static void DosyaOlustur(string kokDizin, string[] uzantilar, string hedefDizin, string dosyaAdi, string format, int islem)
        {
            ArgumentException.ThrowIfNullOrEmpty(kokDizin);
            ArgumentNullException.ThrowIfNull(uzantilar);
            ArgumentException.ThrowIfNullOrEmpty(hedefDizin);
            ArgumentException.ThrowIfNullOrEmpty(dosyaAdi);
            ArgumentException.ThrowIfNullOrEmpty(format);

            Console.WriteLine($"İşlem: {islem}");

            if (islem == 1)
            {
                ProjeKodlariOkuma.TreeViewOlustur.TreeViewOlusturMethod(
                    Path.Combine(hedefDizin, dosyaAdi + "." + format),
                    Path.Combine(hedefDizin, dosyaAdi + "_treeview.txt")
                    );
            }
            else if (islem == 2)
            {
                ProjeKodlariOkuma.DosyaTarayici.DosyaOlustur(kokDizin, uzantilar, hedefDizin, dosyaAdi, format, 0);
            }
            else if (islem == 3)
            {
                ProjeKodlariOkuma.DosyaTarayici.DosyaOlustur(kokDizin, uzantilar, hedefDizin, dosyaAdi, format, 0);
                ProjeKodlariOkuma.TreeViewOlustur.TreeViewOlusturMethod(
                   Path.Combine(hedefDizin, dosyaAdi + "." + format),
                   Path.Combine(hedefDizin, dosyaAdi + "_treeview.txt")
                   );
            }
            else
            {
                Console.WriteLine("Uyari: Gecersiz islem parametresi: " + islem);
                Console.ReadLine();
                return;
            }


            if (!Directory.Exists(kokDizin))
            {
                Console.WriteLine("Uyari: Dizin bulunamadi: " + kokDizin);
                Console.ReadLine();
                return;
            }

            var extSet = uzantilar.Where(s => !string.IsNullOrWhiteSpace(s))
                                  .Select(s => s.Trim().StartsWith(".") ? s.Trim().ToLowerInvariant()
                                                                        : "." + s.Trim().ToLowerInvariant())
                                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Console.WriteLine("Kok Dizin: " + kokDizin);
            Console.WriteLine("Uzantilar: " + string.Join(", ", extSet));
            Console.WriteLine("Taramaya baslaniyor...");

            var paths = Directory.EnumerateFiles(kokDizin, "*.*", SearchOption.AllDirectories)
                                 .Where(p => extSet.Contains(Path.GetExtension(p)))
                                 .ToArray();

            foreach (var p in paths)
            {
                var rel = Path.GetRelativePath(kokDizin, p).Replace('\\', '/');
                Console.WriteLine(rel);
            }
            Console.WriteLine("Toplam dosya: " + paths.Length);

            // JSON/NDJSON olustur
            Directory.CreateDirectory(hedefDizin);
            format = format.Trim().ToLowerInvariant();
            if (format != "json" && format != "ndjson")
                throw new ArgumentOutOfRangeException(nameof(format), "json veya ndjson olmali.");

            var kayitlar = new List<Kayit>(paths.Length);
            foreach (var path in paths)
            {
                var relPath = Path.GetRelativePath(kokDizin, path).Replace('\\', '/');
                var dizin = Path.GetDirectoryName(relPath)?.Replace('\\', '/') ?? "";
                var dosya = Path.GetFileName(path);
                var icerik = ReadAllTextUtf8(path);
                var proje = FindNearestProjectName(path) ?? InferTopFolderAsProject(kokDizin, path);

                kayitlar.Add(new Kayit(
                    Proje: proje,
                    Dizin: dizin,
                    Dosya: dosya,
                    Icerik: icerik
                ));
            }

            var outPath = Path.Combine(hedefDizin, $"{dosyaAdi}.{format}");
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            };

            using (var fs = File.Create(outPath))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                if (format == "json")
                {
                    sw.Write(JsonSerializer.Serialize(kayitlar, jsonOptions));
                }
                else // ndjson
                {
                    foreach (var k in kayitlar)
                        sw.WriteLine(JsonSerializer.Serialize(k, jsonOptions));
                }
            }

            Console.WriteLine("JSON cikti: " + outPath);
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
                if (csprojs.Length > 0) return Path.GetFileNameWithoutExtension(csprojs[0].Name);
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
