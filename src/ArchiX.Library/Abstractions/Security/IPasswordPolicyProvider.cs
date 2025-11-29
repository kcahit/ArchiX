namespace ArchiX.Library.Abstractions.Security;

public interface IPasswordPolicyProvider
{
    ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default);
    void Invalidate(int applicationId = 1);
}
