using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ArchiX.Library.Config
{
    /// <summary>
    /// DB işlemlerinin çalışıp çalışmayacağını belirleyen politika.
    /// Kaynak: <c>ArchiX:AllowDbOps</c>.
    /// </summary>
    public static class ShouldRunDbOps
    {
        private const string AllowKey = "ArchiX:AllowDbOps";
        private static IConfiguration? _configuration;

        /// <summary>
        /// Library için yapılandırmayı atar.
        /// </summary>
        /// <param name="configuration">Uygulama yapılandırması.</param>
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Yalnızca <c>ArchiX:AllowDbOps</c> değerini okur.
        /// </summary>
        public static bool IsEnabled()
        {
            return _configuration?.GetValue<bool>(AllowKey, false) ?? false;
        }

        /// <summary>
        /// Ortam ve konfigürasyona göre karar verir.
        /// Production ise true; aksi halde <c>ArchiX:AllowDbOps</c>.
        /// </summary>
        public static bool Evaluate(IConfiguration config, IHostEnvironment env)
        {
            if (env.IsProduction()) return true;
            return config.GetValue<bool>(AllowKey, false);
        }
    }
}
