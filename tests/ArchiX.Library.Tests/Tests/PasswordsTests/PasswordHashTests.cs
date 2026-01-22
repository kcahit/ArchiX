using ArchiX.Library.Abstractions.Security;

using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests
{
    public class PasswordHashTests
    {
        [Fact]
        public async Task HashPassword_ShouldWork()
        {
            using var sp = TestServiceHelper.BuildServices("HashTests");
            var hasher = sp.GetRequiredService<IPasswordHasher>();
            var provider = sp.GetRequiredService<IPasswordPolicyProvider>();
            var policy = await provider.GetAsync(1);
            var hash = await hasher.HashAsync("Test123!", policy);
            Assert.NotNull(hash);
        }
    }
}

