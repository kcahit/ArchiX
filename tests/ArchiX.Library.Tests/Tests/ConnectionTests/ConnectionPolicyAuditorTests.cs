using ArchiX.Library.Services.Security;

using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests;

public sealed class ConnectionPolicyAuditorTests
{
    [Fact]
    public void MaskingService_DoesNotLeakPassword_CommonPattern()
    {
        var masking = new MaskingService();
        var raw = "Server=localhost;Database=Db;User Id=sa;Password=SuperSecret123!;Encrypt=True;";
        var masked = masking.Mask(raw, 4, 4);

        Assert.DoesNotContain("SuperSecret123", masked);
    }
}
