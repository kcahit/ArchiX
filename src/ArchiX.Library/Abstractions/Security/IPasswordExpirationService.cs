using ArchiX.Library.Entities;

namespace ArchiX.Library.Abstractions.Security
{
    public interface IPasswordExpirationService
    {
        bool IsExpired(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null);

        int? GetDaysUntilExpiration(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null);

        DateTimeOffset? GetExpirationDate(User user, PasswordPolicyOptions policy);
    }
}
