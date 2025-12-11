using System;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Entities;

namespace ArchiX.Library.Runtime.Security
{
    public class PasswordExpirationService : IPasswordExpirationService
    {
        public bool IsExpired(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null)
        {
            if (policy?.MaxPasswordAgeDays == null)
                return false;

            if (policy.MaxPasswordAgeDays <= 0)
                throw new InvalidOperationException("MaxPasswordAgeDays must be greater than zero.");

            if (user?.PasswordChangedAtUtc == null)
                return false;

            var currentTime = now ?? DateTimeOffset.UtcNow;
            var expirationDate = user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);

            return currentTime > expirationDate;
        }

        public int? GetDaysUntilExpiration(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null)
        {
            if (policy?.MaxPasswordAgeDays == null)
                return null;

            if (user?.PasswordChangedAtUtc == null)
                return null;

            var currentTime = now ?? DateTimeOffset.UtcNow;
            var expirationDate = user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);
            var daysRemaining = (expirationDate - currentTime).Days;

            return daysRemaining >= 0 ? daysRemaining : 0;
        }

        public DateTimeOffset? GetExpirationDate(User user, PasswordPolicyOptions policy)
        {
            if (policy?.MaxPasswordAgeDays == null || user?.PasswordChangedAtUtc == null)
                return null;

            return user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);
        }
    }
}
