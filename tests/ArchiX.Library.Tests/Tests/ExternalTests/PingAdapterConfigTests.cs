// File: tests/ArchiX.Library.Tests.Tests.ExternalTests/PingAdapterConfigTests.cs
#nullable enable
using ArchiX.Library.External;
using Xunit;

namespace ArchiX.Library.Tests.Tests.ExternalTests
{
    /// <summary>PingAdapter konfigürasyon bağlama/doğrulama testleri.</summary>
    public sealed class PingAdapterConfigTests
    {
        /// <summary>Geçerli konfigürasyonla IPingAdapter çözümlenir.</summary>
        [Fact]
        public void AddPingAdapter_WithValidConfig_ResolvesService()
        {
            // Arrange
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["ExternalServices:DemoApi:BaseAddress"] = "http://localhost:5055/",
                ["ExternalServices:DemoApi:TimeoutSeconds"] = "5"
            });

            var services = new ServiceCollection();

            // Act
            services.AddPingAdapter(config, "ExternalServices:DemoApi");
            var sp = services.BuildServiceProvider();

            // Assert
            var svc = sp.GetService<ArchiX.Library.Abstractions.External.IPingAdapter>();
            Assert.NotNull(svc);
        }

        /// <summary>BaseAddress eksikse InvalidOperationException fırlatır.</summary>
        [Fact]
        public void AddPingAdapter_MissingBaseAddress_Throws()
        {
            // Arrange
            var config = BuildConfig(new Dictionary<string, string?>());
            var services = new ServiceCollection();

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => services.AddPingAdapter(config, "ExternalServices:DemoApi"));
            Assert.Contains("okunamadı", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Geçersiz URL verilirse InvalidOperationException fırlatır.</summary>
        [Fact]
        public void AddPingAdapter_InvalidUrl_Throws()
        {
            // Arrange
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["ExternalServices:DemoApi:BaseAddress"] = "not-a-url"
            });
            var services = new ServiceCollection();

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => services.AddPingAdapter(config, "ExternalServices:DemoApi"));
            Assert.Contains("geçersiz", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Hafıza içi konfigürasyon oluşturur.</summary>
        private static IConfiguration BuildConfig(IDictionary<string, string?> dict)
            => new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }
}
