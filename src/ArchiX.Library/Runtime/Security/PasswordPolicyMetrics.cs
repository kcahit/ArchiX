using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// PasswordPolicy operasyonlarý için metrik kaydedicisi (PK-11).
/// </summary>
public sealed class PasswordPolicyMetrics
{
    private static readonly Meter Meter = new("ArchiX.PasswordPolicy", "1.0.0");

    private readonly Counter<long> _policyReadCounter;
    private readonly Counter<long> _policyInvalidateCounter;
    private readonly Counter<long> _policyUpdateCounter;
    private readonly Counter<long> _validationErrorCounter;

    public PasswordPolicyMetrics()
    {
        _policyReadCounter = Meter.CreateCounter<long>(
            "password_policy.read.total",
            description: "PasswordPolicy okuma sayýsý (cache hit/miss dahil)");

        _policyInvalidateCounter = Meter.CreateCounter<long>(
            "password_policy.invalidate.total",
            description: "PasswordPolicy cache invalidate sayýsý");

        _policyUpdateCounter = Meter.CreateCounter<long>(
            "password_policy.update.total",
            description: "PasswordPolicy güncelleme sayýsý");

        _validationErrorCounter = Meter.CreateCounter<long>(
            "password_policy.validation_error.total",
            description: "PasswordPolicy validasyon hatasý sayýsý");
    }

    public void RecordRead(int applicationId, bool fromCache)
    {
        _policyReadCounter.Add(1, 
            new KeyValuePair<string, object?>("app_id", applicationId),
            new KeyValuePair<string, object?>("from_cache", fromCache));
    }

    public void RecordInvalidate(int applicationId)
    {
        _policyInvalidateCounter.Add(1,
            new KeyValuePair<string, object?>("app_id", applicationId));
    }

    public void RecordUpdate(int applicationId, bool success)
    {
        _policyUpdateCounter.Add(1,
            new KeyValuePair<string, object?>("app_id", applicationId),
            new KeyValuePair<string, object?>("success", success));
    }

    public void RecordValidationError(int applicationId, string errorType)
    {
        _validationErrorCounter.Add(1,
            new KeyValuePair<string, object?>("app_id", applicationId),
            new KeyValuePair<string, object?>("error_type", errorType));
    }
}
