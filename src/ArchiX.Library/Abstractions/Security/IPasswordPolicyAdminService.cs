namespace ArchiX.Library.Abstractions.Security;

public interface IPasswordPolicyAdminService
{
    Task<string> GetRawJsonAsync(int applicationId = 1, CancellationToken ct = default);
    Task UpdateAsync(string json, int applicationId = 1, CancellationToken ct = default);
}
