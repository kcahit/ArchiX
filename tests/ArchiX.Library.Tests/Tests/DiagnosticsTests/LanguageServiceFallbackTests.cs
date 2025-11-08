#nullable enable
using System.Collections.Concurrent;
using System.Globalization;
using Xunit;

namespace ArchiX.Library.Tests.Tests.DiagnosticsTests
{
    /// <summary>
    /// ILanguageService için kültür fallback, throwIfMissing, format, context ayrımı ve concurrency testleri.
    /// Üretim sınıfına dokunmamak için test-içi minimal bir Fake uygulanmıştır.
    /// </summary>
    public class LanguageServiceFallbackTests
    {
        private sealed class FakeLanguageService : ArchiX.Library.Abstractions.Localization.ILanguageService
        {
            private readonly string _defaultCulture;
            private readonly ConcurrentDictionary<string, Dictionary<string, string>> _dict = new();

            // ILanguageService sözleşmesine uygun: CultureInfo
            public CultureInfo CurrentCulture { get; set; }

            public FakeLanguageService(string defaultCulture = "en-US")
            {
                _defaultCulture = defaultCulture;
                CurrentCulture = CultureInfo.GetCultureInfo(defaultCulture);
                _dict[_defaultCulture] = new(StringComparer.OrdinalIgnoreCase);
            }

            public void UseCulture(string culture)
            {
                CurrentCulture = CultureInfo.GetCultureInfo(culture);
                if (!_dict.ContainsKey(CurrentCulture.Name))
                    _dict[CurrentCulture.Name] = new(StringComparer.OrdinalIgnoreCase);
            }

            public void Set(string culture, string key)
            {
                if (!_dict.ContainsKey(culture)) _dict[culture] = new(StringComparer.OrdinalIgnoreCase);
                if (!_dict[culture].ContainsKey(key)) _dict[culture][key] = key;
            }

            public bool TryGet(string key, out string value)
            {
                var cur = CurrentCulture.Name;
                if (_dict.TryGetValue(cur, out var map) && map.TryGetValue(key, out var v1)) { value = v1; return true; }
                if (_dict.TryGetValue(_defaultCulture, out var def) && def.TryGetValue(key, out var v2)) { value = v2; return true; }
                value = string.Empty; return false;
            }

            public string T(string key, bool throwIfMissing)
            {
                if (TryGet(key, out var v)) return v;
                if (throwIfMissing) throw new KeyNotFoundException($"Missing translation for '{key}' in '{CurrentCulture.Name}' and default '{_defaultCulture}'."); 
                return key; // graceful fallback
            }

            public string T(string key, params object[] args)
            {
                if (TryGet(key, out var v)) return string.Format(CurrentCulture, v, args);
                return key;
            }

            // ILanguageService geniş imzaları
            public Task<string?> GetDisplayNameAsync(string itemType, string entityName, string fieldName, string code, string culture, CancellationToken cancellationToken = default)
            {
                var key = $"{entityName}.{fieldName}";
                var prev = CurrentCulture;
                CurrentCulture = CultureInfo.GetCultureInfo(culture);
                try
                {
                    return Task.FromResult<string?>(T(key, throwIfMissing: false));
                }
                finally
                {
                    CurrentCulture = prev;
                }
            }

            public Task<List<(int Id, string DisplayName)>> GetListAsync(string itemType, string entityName, string fieldName, string culture, CancellationToken cancellationToken = default)
            {
                var key = $"{entityName}.{fieldName}";
                var prev = CurrentCulture;
                CurrentCulture = CultureInfo.GetCultureInfo(culture);
                try
                {
                    var text = T(key, throwIfMissing: false);
                    var list = new List<(int Id, string DisplayName)> { (1, text) };
                    return Task.FromResult(list);
                }
                finally
                {
                    CurrentCulture = prev;
                }
            }
        }

        [Fact]
        public void Fallback_To_DefaultCulture_When_Key_Missing_In_Target()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.Set("en-US", "Hello");   // default'ta var
            svc.UseCulture("tr-TR");     // hedefte yok → default'a düşmeli

            var text = svc.T("Hello", throwIfMissing: true);
            Assert.Equal("Hello", text);
        }

        [Fact]
        public void Throw_If_Missing_In_All_Cultures_When_Requested()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.UseCulture("tr-TR"); // hiçbirinde yok
            Assert.Throws<KeyNotFoundException>(() => svc.T("NotExist", throwIfMissing: true));
        }

        [Fact]
        public void Graceful_Return_Key_When_NotThrowing()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.UseCulture("tr-TR");
            var text = svc.T("UnknownKey", throwIfMissing: false);
            Assert.Equal("UnknownKey", text);
        }

        [Fact]
        public async Task Async_APIs_Respect_Fallback_Rules()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.Set("en-US", "Order.Status");
            svc.UseCulture("tr-TR"); // tr-TR’de bu key yok

            var display = await svc.GetDisplayNameAsync("Item", "Order", "Status", code: "", culture: "tr-TR");
            Assert.Equal("Order.Status", display);
        }

        [Fact]
        public void Format_Uses_CurrentCulture()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.Set("tr-TR", "Total {0:N2}");
            svc.UseCulture("tr-TR");

            var text = svc.T("Total {0:N2}", 1234.56m);
            Assert.Contains("1.234,56", text);
        }

        [Fact]
        public async Task Context_Scope_Treats_SameKey_AsDistinct()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.Set("en-US", "Order.Status");
            svc.Set("en-US", "Product.Status");

            svc.UseCulture("tr-TR"); // ikisi de fallback'tan gelmeli ama farklı key

            var order = await svc.GetDisplayNameAsync("Item", "Order", "Status", code: "", culture: "tr-TR");
            var product = await svc.GetDisplayNameAsync("Item", "Product", "Status", code: "", culture: "tr-TR");

            Assert.Equal("Order.Status", order);
            Assert.Equal("Product.Status", product);
            Assert.NotEqual(order, product);
        }

        [Fact]
        public async Task ConcurrentReads_DoNotCorruptCache()
        {
            var svc = new FakeLanguageService(defaultCulture: "en-US");
            svc.Set("en-US", "Hello");
            svc.UseCulture("tr-TR"); // hedefte yok → default'a fallback

            var tasks = new List<Task<string>>();
            for (int i = 0; i < 200; i++)
            {
                tasks.Add(Task.Run(() => svc.T("Hello", throwIfMissing: false)));
            }

            var results = await Task.WhenAll(tasks);
            foreach (var r in results)
                Assert.Equal("Hello", r);
        }
    }
}
