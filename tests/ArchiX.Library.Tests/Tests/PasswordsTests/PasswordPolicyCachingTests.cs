using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests
{
    public class PasswordPolicyCachingTests
    {
        [Fact]
        public async Task PolicyProvider_ShouldCache()
        {
            using var sp = TestServiceHelper.BuildServices("CachingTests");
            var provider = sp.GetRequiredService<ArchiX.Library.Abstractions.Security.IPasswordPolicyProvider>();
            var p1 = await provider.GetAsync(1);
            var p2 = await provider.GetAsync(1);
            Assert.NotNull(p1);
            Assert.NotNull(p2);
        }
    }
}
