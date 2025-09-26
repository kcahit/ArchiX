// File: src/ArchiX.Library/Infrastructure/Http/CorrelationApplicationBuilderExtensions.cs
#nullable enable
using Microsoft.AspNetCore.Builder;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>Korelasyon middleware kayıt uzantısı.</summary>
    public static class CorrelationApplicationBuilderExtensions
    {
        /// <summary>Pipeline’a korelasyon middleware’ini ekler.</summary>
        public static IApplicationBuilder UseArchiXCorrelation(this IApplicationBuilder app)
        {
            System.ArgumentNullException.ThrowIfNull(app);
            return app.UseMiddleware<CorrelationMiddleware>();
        }
    }
}
