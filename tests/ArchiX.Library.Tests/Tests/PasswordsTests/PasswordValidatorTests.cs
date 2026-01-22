using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests
{
    public class PasswordValidatorTests
    {
        [Fact]
        public async Task ValidPassword_ShouldPass()
        {
            using var sp = TestServiceHelper.BuildServices("ValidatorTests");
            var validator = sp.GetRequiredService<ArchiX.Library.Abstractions.Security.IPasswordPolicyProvider>();
            var policy = await validator.GetAsync(1);
            Assert.NotNull(policy);
        }
    }
}
