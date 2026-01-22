// File: src/ArchiX.Library/Infrastructure/Http/HttpPoliciesOptions.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>#57 HTTP retry/timeout politikaları (DB-driven).</summary>
    public sealed class HttpPoliciesOptions
    {
        /// <summary>Maksimum tekrar sayısı (0–10, varsayılan 2).</summary>
        [Range(0, 10)]
        public int RetryCount { get; init; } = 2;

        /// <summary>Exponential backoff taban gecikmesi milisaniye (10–60000, varsayılan 200).</summary>
        [Range(10, 60_000)]
        public int BaseDelayMs { get; init; } = 200;

        /// <summary>İstek başına zaman aşımı saniye (1–300, varsayılan 30).</summary>
        [Range(1, 300)]
        public int TimeoutSeconds { get; init; } = 30;

        /// <summary>Taban gecikmeyi <see cref="TimeSpan"/> olarak verir.</summary>
        public TimeSpan GetBaseDelay() => TimeSpan.FromMilliseconds(BaseDelayMs);

        /// <summary>Zaman aşımını <see cref="TimeSpan"/> olarak verir.</summary>
        public TimeSpan GetTimeout() => TimeSpan.FromSeconds(TimeoutSeconds);

        /// <summary>DataAnnotations doğrulaması yapar; geçersizse <see cref="InvalidOperationException"/> fırlatır.</summary>
        public void Validate()
        {
            var ctx = new ValidationContext(this);
            try
            {
                Validator.ValidateObject(this, ctx, validateAllProperties: true);
            }
            catch (ValidationException vex)
            {
                throw new InvalidOperationException($"Geçersiz HttpPoliciesOptions: {vex.Message}", vex);
            }
        }
    }
}

