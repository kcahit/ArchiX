using ArchiX.Library.Formatting;
using Xunit;

namespace ArchiX.Library.Tests.Tests.FormattingTests
{
    public sealed class JsonTextFormatterTests
    {
        [Fact]
        public void Minify_RemovesWhitespace()
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
            var minified = JsonTextFormatter.Minify(json);

            // Assert
            Assert.DoesNotContain("\n", minified);
            Assert.DoesNotContain("  ", minified);
            Assert.Contains("\"version\":1", minified);
        }

        [Fact]
        public void Minify_PreservesJsonStructure()
        {
            // Arrange
            var json = """
            {
              "name": "test",
              "value": 123
            }
            """;

            // Act
            var minified = JsonTextFormatter.Minify(json);

            // Assert
            Assert.Contains("\"name\":\"test\"", minified);
            Assert.Contains("\"value\":123", minified);
        }

        [Fact]
        public void Minify_HandlesNestedObjects()
        {
            // Arrange
            var json = """
            {
              "outer": {
                "inner": {
                  "value": 42
                }
              }
            }
            """;

            // Act
            var minified = JsonTextFormatter.Minify(json);

            // Assert
            Assert.DoesNotContain("\n", minified);
            Assert.Contains("\"outer\":{", minified);
            Assert.Contains("\"inner\":{", minified);
        }

        [Fact]
        public void TryValidate_ReturnsTrueForValidJson()
        {
            // Arrange
            var validJson = "{\"test\":123}";

            // Act
            var isValid = JsonTextFormatter.TryValidate(validJson, out var error);

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void TryValidate_ReturnsFalseForInvalidJson()
        {
            // Arrange
            var invalidJson = "{ invalid }";

            // Act
            var isValid = JsonTextFormatter.TryValidate(invalidJson, out var error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
        }

        [Fact]
        public void Minify_HandlesEmptyObject()
        {
            // Arrange
            var json = "{ }";

            // Act
            var minified = JsonTextFormatter.Minify(json);

            // Assert
            Assert.Equal("{}", minified);
        }

        [Fact]
        public void Minify_HandlesArrays()
        {
            // Arrange
            var json = """
            {
              "list": [
                "item1",
                "item2"
              ]
            }
            """;

            // Act
            var minified = JsonTextFormatter.Minify(json);

            // Assert
            Assert.DoesNotContain("\n", minified);
            Assert.Contains("\"list\":[", minified);
            Assert.Contains("\"item1\"", minified);
        }
    }
}
