using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicySymbolsConsistencyCheckerTests
    {
        [Fact]
        public void CheckConsistency_ReturnsNull_WhenSymbolsMatch()
        {
            // Arrange
            var symbols = "!@#$%^&*_-+=:?.,;";

            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(symbols);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CheckConsistency_ReturnsNull_WhenSymbolsMatchButDifferentOrder()
        {
            // Arrange
            var symbols = ";.,?:=+-_*&^%$#@!"; // Ayný karakterler, farklý sýra

            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(symbols);

            // Assert
            Assert.Null(result); // Normalize edildiði için tutarlý sayýlýr
        }

        [Fact]
        public void CheckConsistency_ReturnsNull_WhenSymbolsHaveDuplicates()
        {
            // Arrange
            var symbols = "!@#$%^&*_-+=:?.,;!!!"; // Tekrar eden karakterler

            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(symbols);

            // Assert
            Assert.Null(result); // Normalize edildikten sonra ayný
        }

        [Fact]
        public void CheckConsistency_ReturnsWarning_WhenSymbolsEmpty()
        {
            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency("");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("boþ", result);
        }

        [Fact]
        public void CheckConsistency_ReturnsWarning_WhenSymbolsDifferent()
        {
            // Arrange
            var symbols = "!@#$%"; // Eksik karakterler

            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(symbols);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("UI örneðinden farklý", result);
            Assert.Contains("Beklenen", result);
        }

        [Fact]
        public void CheckConsistency_ReturnsWarning_WhenExtraSymbolsAdded()
        {
            // Arrange
            var symbols = "!@#$%^&*_-+=:?.,;|<>"; // Fazladan karakterler

            // Act
            var result = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(symbols);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("UI örneðinden farklý", result);
        }

        [Fact]
        public void HasInconsistency_ReturnsTrue_WhenInconsistent()
        {
            // Arrange
            var symbols = "!@#";

            // Act
            var hasIssue = PasswordPolicySymbolsConsistencyChecker.HasInconsistency(symbols);

            // Assert
            Assert.True(hasIssue);
        }

        [Fact]
        public void HasInconsistency_ReturnsFalse_WhenConsistent()
        {
            // Arrange
            var symbols = "!@#$%^&*_-+=:?.,;";

            // Act
            var hasIssue = PasswordPolicySymbolsConsistencyChecker.HasInconsistency(symbols);

            // Assert
            Assert.False(hasIssue);
        }

        [Fact]
        public void DefaultAllowedSymbols_HasExpectedValue()
        {
            // Assert
            Assert.Equal("!@#$%^&*_-+=:?.,;", PasswordPolicySymbolsConsistencyChecker.DefaultAllowedSymbols);
        }
    }
}
