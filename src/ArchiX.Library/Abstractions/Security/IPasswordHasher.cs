namespace ArchiX.Library.Abstractions.Security;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password, PasswordPolicyOptions policy, CancellationToken ct = default);
    Task<bool> VerifyAsync(string password, string encodedHash, PasswordPolicyOptions policy, CancellationToken ct = default);
}
