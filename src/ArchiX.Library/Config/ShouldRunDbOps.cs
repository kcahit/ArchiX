using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ArchiX.Library.Config;

/// <summary>
/// DB işlemlerinin çalıştırılıp çalıştırılmayacağını belirleyen policy sınıfı.
/// </summary>
public static class ShouldRunDbOps
{
    /// <summary>
    /// Ortama ve konfigürasyona göre DB işlemlerinin çalışmasına izin verilip verilmeyeceğini döner.
    /// Production ortamında her zaman true döner.
    /// </summary>
    /// <param name="config">Uygulama konfigürasyonu.</param>
    /// <param name="env">Geçerli ortam bilgisi.</param>
    /// <returns>True ise DB işlemleri çalışır, false ise çalışmaz.</returns>
    public static bool Evaluate(IConfiguration config, IHostEnvironment env)
    {
        if (env.IsProduction()) return true;
        return config.GetValue("ArchiX:AllowDbOps", false);
    }
}
