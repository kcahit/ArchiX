using System.Text.Json;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Runtime.Security;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyVersionUpgraderTests
    {
        private readonly ILogger<PasswordPolicyVersionUpgrader> _logger;

        public PasswordPolicyVersionUpgraderTests()
        {
            _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<PasswordPolicyVersionUpgrader>();
        }

        [Fact]
        public void UpgradeIfNeeded_ReturnsModel_WhenV1Format()
        {
            // Arrange
            var upgrader = new PasswordPolicyVersionUpgrader(_logger);
            var v1Json = """
            {
              "version": 1,
              "minLength": 12,
              "maxLength": 128,
              "requireUpper": true,
              "requireLower": true,
              "requireDigit": true,
              "requireSymbol": true,
              "allowedSymbols": "!@#$%^&*",
              "minDistinctChars": 5,
              "maxRepeatedSequence": 3,
              "blockList": ["password"],
              "historyCount": 10,
              "lockoutThreshold": 5,
              "lockoutSeconds": 900,
              "hash": {
                "algorithm": "Argon2id",
                "memoryKb": 65536,
                "parallelism": 2,
                "iterations": 3,
                "saltLength": 16,
                "hashLength": 32,
                "fallback": {
                  "algorithm": "PBKDF2-SHA512",
                  "iterations": 210000
                },
                "pepperEnabled": false
              }
            }
            """;

            // Act
            var result = upgrader.UpgradeIfNeeded(v1Json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Version);
            Assert.Equal(12, result.MinLength);
            Assert.Equal("Argon2id", result.Hash.Algorithm);
        }

        [Fact]
        public void UpgradeIfNeeded_DefaultsToV1_WhenVersionMissing()
        {
            // Arrange
            var upgrader = new PasswordPolicyVersionUpgrader(_logger);
            var jsonWithoutVersion = """
            {
              "minLength": 10,
              "maxLength": 100,
              "requireUpper": false
            }
            """;

            // Act
            var result = upgrader.UpgradeIfNeeded(jsonWithoutVersion);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.MinLength);
        }

        [Fact]
        public void UpgradeIfNeeded_HandlesV2Format()
        {
            // Arrange
            var upgrader = new PasswordPolicyVersionUpgrader(_logger);
            var v2Json = """
            {
              "version": 2,
              "minLength": 14,
              "maxLength": 256
            }
            """;

            // Act
            var result = upgrader.UpgradeIfNeeded(v2Json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(14, result.MinLength);
        }

        [Fact]
        public void UpgradeIfNeeded_ThrowsForUnsupportedVersion()
        {
            // Arrange
            var upgrader = new PasswordPolicyVersionUpgrader(_logger);
            var v99Json = """
            {
              "version": 99,
              "minLength": 8
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<NotSupportedException>(() => upgrader.UpgradeIfNeeded(v99Json));
            Assert.Contains("version 99", ex.Message);
        }

        [Fact]
        public void UpgradeIfNeeded_ThrowsWhenJsonEmpty()
        {
            // Arrange
            var upgrader = new PasswordPolicyVersionUpgrader(_logger);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => upgrader.UpgradeIfNeeded(""));
            Assert.Throws<ArgumentException>(() => upgrader.UpgradeIfNeeded("   "));
        }
    }
}
