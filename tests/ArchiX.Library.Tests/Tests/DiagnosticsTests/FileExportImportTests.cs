// tests/ArchiXTest.ApiWeb/Test/DiagnosticsTests/FileExportImportTests.cs
using System.Globalization;
using System.Text;

using Xunit;

namespace ArchiX.Library.Tests.Tests.DiagnosticsTests
{
    /// <summary>
    /// 11. Test – Dosya/Veri Aktarımları (Export/Import) senaryoları.
    /// </summary>
    public class FileExportImportTests
    {
        [Fact]
        public async Task ExportToCsv_Should_Create_Valid_Csv_String()
        {
            var data = new List<ProductDto>
            {
                new() { Id = 1, Name = "Kalem", Price = 10.5m },
                new() { Id = 2, Name = "Defter", Price = 20m }
            };

            var csv = await FakeExportService.ExportToCsvAsync(data);

            Assert.StartsWith("Id,Name,Price", csv.Trim());

            var lines = SplitLines(csv);
            Assert.Equal(3, lines.Length); // header + 2 satır
        }

        [Fact]
        public async Task ImportFromCsv_Should_Parse_Valid_Records()
        {
            var csv = "Id,Name,Price\n1,Silgi,5.5\n2,Kitap,50";
            var items = await FakeImportService.ImportFromCsvAsync<ProductDto>(csv);

            Assert.Equal(2, items.Count);
            Assert.Equal("Silgi", items[0].Name);
            Assert.Equal(50m, items[1].Price);
        }

        // Yardımcı: tüm platformlarda güvenli satır bölme
        private static string[] SplitLines(string s) =>
            s.Replace("\r\n", "\n").Replace("\r", "\n")
             .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        private record ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public decimal Price { get; set; }
        }

        private static class FakeExportService
        {
            public static Task<string> ExportToCsvAsync<T>(IEnumerable<T> data)
            {
                var props = typeof(T).GetProperties();
                var sb = new StringBuilder();

                // header
                sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

                // rows
                foreach (var item in data)
                {
                    var values = props.Select(p => p.GetValue(item)?.ToString() ?? "");
                    sb.AppendLine(string.Join(",", values));
                }

                return Task.FromResult(sb.ToString());
            }
        }

        private static class FakeImportService
        {
            public static Task<List<T>> ImportFromCsvAsync<T>(string csv) where T : new()
            {
                var lines = SplitLines(csv);
                if (lines.Length <= 1) return Task.FromResult(new List<T>());

                var headers = lines[0].Split(',');
                var props = typeof(T).GetProperties();

                var list = new List<T>();
                foreach (var line in lines.Skip(1))
                {
                    var values = line.Split(',');
                    var obj = new T();

                    for (int i = 0; i < headers.Length && i < values.Length; i++)
                    {
                        var prop = props.FirstOrDefault(p =>
                            p.Name.Equals(headers[i], StringComparison.OrdinalIgnoreCase));
                        if (prop is null) continue;

                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        object? converted = ConvertTo(values[i], targetType);
                        prop.SetValue(obj, converted);
                    }

                    list.Add(obj);
                }

                return Task.FromResult(list);
            }

            private static object? ConvertTo(string input, Type targetType)
            {
                if (targetType == typeof(string)) return input;
                if (targetType.IsEnum) return Enum.Parse(targetType, input, ignoreCase: true);
                if (targetType == typeof(Guid)) return Guid.Parse(input);

                // Sayısal ve DateTime türleri için kültür bağımsız dönüşüm
                return Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
            }
        }
    }
}
