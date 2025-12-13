using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// PasswordPolicy JSON þemasýnýn versiyon yükseltmelerini yöneten strateji (PK-07).
/// </summary>
public interface IPasswordPolicyVersionUpgrader
{
    /// <summary>
    /// JSON string'i okur, gerekirse yükseltir ve güncel modeli döner.
    /// </summary>
    PasswordPolicyOptions UpgradeIfNeeded(string json);
}