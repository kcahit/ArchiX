using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicySchemaValidatorTests
    {
        [Fact]
        public void Validate_ReturnsNoErrors_WhenValidJson()
        {
            // Arrange
            var validJson = """
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
            var errors = PasswordPolicySchemaValidator.Validate(validJson);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ReturnsError_WhenJsonIsEmpty()
        {
            // Act
            var errors = PasswordPolicySchemaValidator.Validate("");

            // Assert
            Assert.Single(errors);
            Assert.Contains("JSON boþ olamaz", errors[0]);
        }

        [Fact]
        public void Validate_ReturnsError_WhenJsonIsInvalid()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(invalidJson);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("JSON parse hatasý"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenVersionMissing()
        {
            // Arrange
            var json = """
            {
              "minLength": 12,
              "maxLength": 128
            }
            """;

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(json);

            // Assert
            Assert.Contains(errors, e => e.Contains("'version' alaný zorunlu"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenMinLengthNegative()
        {
            // Arrange
            var json = """
            {
              "version": 1,
              "minLength": 0,
              "maxLength": 128
            }
            """;

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(json);

            // Assert
            Assert.Contains(errors, e => e.Contains("'minLength' en az 1 olmalý"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenMaxLengthLessThanMinLength()
        {
            // Arrange
            var json = """
            {
              "version": 1,
              "minLength": 100,
              "maxLength": 50
            }
            """;

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(json);

            // Assert
            Assert.Contains(errors, e => e.Contains("'maxLength' 'minLength'ten küçük olamaz"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenRequiredBooleanFieldsMissing()
        {
            // Arrange
            var json = """
            {
              "version": 1,
              "minLength": 12,
              "maxLength": 128
            }
            """;

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(json);

            // Assert
            Assert.Contains(errors, e => e.Contains("'requireUpper'"));
            Assert.Contains(errors, e => e.Contains("'requireLower'"));
            Assert.Contains(errors, e => e.Contains("'requireDigit'"));
            Assert.Contains(errors, e => e.Contains("'requireSymbol'"));
        }

        [Fact]
        public void Validate_ReturnsError_WhenHashObjectMissing()
        {
            // Arrange
            var json = """
            {
              "version": 1,
              "minLength": 12,
              "maxLength": 128,
              "requireUpper": true,
              "requireLower": true,
              "requireDigit": true,
              "requireSymbol": true,
              "allowedSymbols": "!@#",
              "minDistinctChars": 5,
              "maxRepeatedSequence": 3,
              "blockList": [],
              "historyCount": 10,
              "lockoutThreshold": 5,
              "lockoutSeconds": 900
            }
            """;

            // Act
            var errors = PasswordPolicySchemaValidator.Validate(json);

            // Assert
            Assert.Contains(errors, e => e.Contains("'hash' alaný zorunlu"));
        }

        [Fact]
        public void ValidateOrThrow_ThrowsException_WhenInvalid()
        {
            // Arrange
            var invalidJson = "{ \"version\": 1 }";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                PasswordPolicySchemaValidator.ValidateOrThrow(invalidJson));
            Assert.Contains("þema hatasý", ex.Message);
        }

        [Fact]
        public void ValidateOrThrow_DoesNotThrow_WhenValid()
        {
            // Arrange
            var validJson = """
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

            // Act & Assert (should not throw)
            PasswordPolicySchemaValidator.ValidateOrThrow(validJson);
        }
    }
}
