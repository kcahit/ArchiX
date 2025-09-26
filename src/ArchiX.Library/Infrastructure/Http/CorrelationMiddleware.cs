// File: src/ArchiX.Library/Infrastructure/Http/CorrelationMiddleware.cs
#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>Gelen istekte korelasyon kimliğini okur/üretir ve yanıta yazar.</summary>
    /// <param name="next">Sonraki middleware.</param>
    /// <param name="log">Logger.</param>
    public sealed class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> log)
    {
        private readonly RequestDelegate _next = next ?? throw new System.ArgumentNullException(nameof(next));
        private readonly ILogger<CorrelationMiddleware> _log = log ?? throw new System.ArgumentNullException(nameof(log));

        /// <summary>İstek/yanıt boyunca korelasyon kimliğini taşır.</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            System.ArgumentNullException.ThrowIfNull(context);

            var headers = context.Request.Headers;
            var corrId = headers.TryGetValue(HttpCorrelation.HeaderName, out var v) && !string.IsNullOrWhiteSpace(v)
                ? v.ToString()
                : HttpCorrelation.NewId();

            context.Request.Headers[HttpCorrelation.HeaderName] = corrId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HttpCorrelation.HeaderName] = corrId;
                return Task.CompletedTask;
            });

            using (_log.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = corrId }))
            {
                await _next(context).ConfigureAwait(false);
            }
        }
    }
}
