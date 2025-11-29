// File: ProjeKodlariOkuma/TreeViewOlustur.cs
using System.Text;

namespace ProjeKodlariOkuma
{
    /// <summary>
    /// Kaynak dosyadaki yol listesine göre metin tabanlı ağaç çıktısı üretir.
    /// Her satır bir yol (örn: ArchiX.Library/Config/ShouldRunDbOps.cs).
    /// Kök düzey (ilk segment) satırına " (PROJE)" eklenir.
    /// </summary>
    internal static class TreeViewOlustur
    {
        // IDE0300: collection expression kullan.
        private static readonly char[] PathSeparators = ['\\', '/'];

        /// <summary>
        /// Kaynak dosyadan yolları okur, hedef dosyaya ASCII ağaç yazar.
        /// </summary>
        /// <param name="kaynakDosyaFullDizin">İçinde yollar olan giriş dosyası.</param>
        /// <param name="hedefDosyaFullDizin">Ağaç çıktısının yazılacağı dosya.</param>
        public static void Olustur(string kaynakDosyaFullDizin, string hedefDosyaFullDizin)
        {
            ArgumentException.ThrowIfNullOrEmpty(kaynakDosyaFullDizin);
            ArgumentException.ThrowIfNullOrEmpty(hedefDosyaFullDizin);

            var lines = File.ReadAllLines(kaynakDosyaFullDizin, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                            .Select(static l => l.Trim())
                            .Where(static l => l.Length > 0 && l[0] != '#')
                            .ToArray();

            // parentKey -> children set
            var children = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
            foreach (var line in lines)
            {
                var segments = line.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                    continue;

                var parent = string.Empty;
                for (var i = 0; i < segments.Length; i++)
                {
                    var name = segments[i];
                    if (!children.TryGetValue(parent, out var set))
                    {
                        set = new SortedSet<string>(StringComparer.Ordinal);
                        children[parent] = set;
                    }

                    set.Add(name);

                    parent = parent.Length == 0 ? name : $"{parent}/{name}";
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("ArchiX.sln");

            if (!children.TryGetValue(string.Empty, out var roots) || roots.Count == 0)
            {
                WriteOutput(hedefDosyaFullDizin, sb);
                return;
            }

            var rootList = roots.ToList();
            for (var i = 0; i < rootList.Count; i++)
            {
                var isLast = i == rootList.Count - 1;
                var root = rootList[i];
                sb.Append("+-- ").Append(root).AppendLine(" (PROJE)");
                YazAltAgac(children, root, 1, new List<bool> { isLast }, sb);
            }

            WriteOutput(hedefDosyaFullDizin, sb);
        }

        private static void YazAltAgac(
            Dictionary<string, SortedSet<string>> children,
            string nodeKey,
            int level,
            List<bool> lastFlags,
            StringBuilder sb)
        {
            var key = nodeKey;
            if (!children.TryGetValue(key, out var set) || set.Count == 0)
                return;

            var items = set.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                var child = items[i];
                var isLast = i == items.Count - 1;

                sb.Append(Prefix(lastFlags))
                  .Append("+-- ")
                  .AppendLine(child);

                var childKey = $"{key}/{child}";
                lastFlags.Add(isLast);
                YazAltAgac(children, childKey, level + 1, lastFlags, sb);
                lastFlags.RemoveAt(lastFlags.Count - 1);
            }
        }

        private static string Prefix(List<bool> lastFlags)
        {
            if (lastFlags.Count == 0)
                return string.Empty;

            var sb = new StringBuilder(lastFlags.Count * 4);
            for (var i = 0; i < lastFlags.Count - 1; i++)
            {
                sb.Append(lastFlags[i] ? ' ' : '|').Append("   ");
            }

            sb.Append(lastFlags[^1] ? ' ' : '|').Append("   ");
            return sb.ToString();
        }

        private static void WriteOutput(string hedefDosyaFullDizin, StringBuilder sb)
        {
            var dir = Path.GetDirectoryName(hedefDosyaFullDizin);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var sw = new StreamWriter(hedefDosyaFullDizin, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            sw.Write(sb.ToString());
        }
    }
}
