using System.Text;
using System.Text.Json;

namespace ProjeKodlariOkuma
{
    internal class TreeViewOlustur
    {
        // JSON dizi veya NDJSON satirlarini otomatik algilar
        internal static void TreeViewOlusturMethod(string kaynakDosyaFullDizin, string hedefDosyaFullDizin)
        {
            ArgumentException.ThrowIfNullOrEmpty(kaynakDosyaFullDizin);
            ArgumentException.ThrowIfNullOrEmpty(hedefDosyaFullDizin);

            if (!File.Exists(kaynakDosyaFullDizin))
            {
                Console.WriteLine("Uyari: Kaynak dosya yok: " + kaynakDosyaFullDizin);
                return;
            }

            var outDir = Path.GetDirectoryName(hedefDosyaFullDizin);
            if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

            // Kayitlari oku
            var text = File.ReadAllText(kaynakDosyaFullDizin, new UTF8Encoding(false));
            var span = text.AsSpan().TrimStart();
            var recs = new List<(string Proje, string Dizin, string Dosya)>();

            void Add(JsonElement el)
            {
                string proje = GetStr(el, "Proje");
                string dizin = Normalize(GetStr(el, "Dizin"));
                string dosya = GetStr(el, "Dosya");
                if (string.IsNullOrWhiteSpace(dosya) || dosya == "...") return;
                if (string.IsNullOrWhiteSpace(proje))
                    proje = InferProjectFromDir(dizin) ?? "UnknownProject";
                recs.Add((proje, dizin, dosya));
            }

            if (span.Length > 0 && span[0] == '[')
            {
                using var doc = JsonDocument.Parse(text);
                foreach (var el in doc.RootElement.EnumerateArray()) Add(el);
            }
            else
            {
                foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    var s = line.Trim();
                    if (s.Length == 0 || s == "...") continue;
                    using var doc = JsonDocument.Parse(s);
                    Add(doc.RootElement);
                }
            }

            // Agaci kur
            const string FILES = "\u0000files";
            static SortedDictionary<string, object> NewNode()
                => new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            static SortedSet<string> GetFiles(SortedDictionary<string, object> node)
            {
                if (!node.TryGetValue(FILES, out var o) || o is not SortedSet<string> set)
                {
                    set = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                    node[FILES] = set;
                }
                return set;
            }

            var projects = new SortedDictionary<string, SortedDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in recs)
            {
                var proj = r.Proje;
                var relDir = StripPrefixes(r.Dizin, proj);
                var parts = relDir.Split('/', StringSplitOptions.RemoveEmptyEntries)
                                  .Where(p => p != "...").ToArray();

                if (!projects.TryGetValue(proj, out var root))
                    projects[proj] = root = NewNode();

                Insert(root, parts, r.Dosya);
            }

            // Render: tek koku "PROJELER"
            var sb = new StringBuilder();
            sb.AppendLine("PROJELER");

            var projNames = projects.Keys.ToList();
            for (int i = 0; i < projNames.Count; i++)
            {
                var name = projNames[i];
                bool isLast = i == projNames.Count - 1;

                sb.AppendLine("+-- " + name + " (PROJE)");

                var childPrefix = isLast ? "    " : "|   ";
                Render(projects[name], childPrefix, sb);
            }

            File.WriteAllText(hedefDosyaFullDizin, sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine("TreeView cikti: " + hedefDosyaFullDizin);

            // ---- helpers ----
            static string GetStr(JsonElement el, string name)
                => el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() ?? "" : "";

            static string Normalize(string? p) => (p ?? "").Replace('\\', '/');

            static string? InferProjectFromDir(string dizin)
            {
                var parts = Normalize(dizin).Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && (parts[0].Equals("src", StringComparison.OrdinalIgnoreCase) ||
                                          parts[0].Equals("tests", StringComparison.OrdinalIgnoreCase)))
                    return parts[1];
                if (parts.Length >= 1 && parts[0].Contains('.')) return parts[0];
                return parts.Length >= 1 ? parts[0] : null;
            }

            static string StripPrefixes(string dizin, string proj)
            {
                var d = Normalize(dizin);
                var p1 = $"src/{proj}/";
                var p2 = $"tests/{proj}/";
                var p3 = $"{proj}/";
                if (d.StartsWith(p1, StringComparison.OrdinalIgnoreCase)) return d[p1.Length..];
                if (d.StartsWith(p2, StringComparison.OrdinalIgnoreCase)) return d[p2.Length..];
                if (d.StartsWith(p3, StringComparison.OrdinalIgnoreCase)) return d[p3.Length..];
                return d;
            }

            static void Insert(SortedDictionary<string, object> node, string[] parts, string file)
            {
                if (parts.Length == 0)
                {
                    GetFiles(node).Add(file);
                    return;
                }
                var head = parts[0];
                var tail = parts.Skip(1).ToArray();
                if (!node.TryGetValue(head, out var child) || child is not SortedDictionary<string, object> d)
                {
                    d = NewNode();
                    node[head] = d;
                }
                Insert(d, tail, file);
            }

            static void Render(SortedDictionary<string, object> node, string prefix, StringBuilder sb)
            {
                var dirNames = node.Keys.Where(k => k != FILES).ToList();
                for (int i = 0; i < dirNames.Count; i++)
                {
                    var name = dirNames[i];
                    bool isLastDir = i == dirNames.Count - 1 && (!node.TryGetValue(FILES, out var f) || ((SortedSet<string>)f).Count == 0);
                    sb.AppendLine(prefix + "+-- " + name);
                    Render((SortedDictionary<string, object>)node[name], prefix + (isLastDir ? "    " : "|   "), sb);
                }

                if (node.TryGetValue(FILES, out var filesObj) && filesObj is SortedSet<string> files)
                {
                    foreach (var f in files)
                        sb.AppendLine(prefix + "+-- " + f);
                }
            }
        }
    }
}
