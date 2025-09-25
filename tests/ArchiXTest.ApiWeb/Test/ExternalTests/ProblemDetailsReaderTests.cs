// File: tests/ArchiXTest.ApiWeb/Test/ExternalTests/ProblemDetailsReaderTests.cs
#nullable enable
using System.Net;
using System.Text;

using ArchiX.Library.Infrastructure.Http;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.ExternalTests
{
    /// <summary>ProblemDetailsReader davranış testleri.</summary>
    public sealed class ProblemDetailsReaderTests
    {
        [Fact]
        public async Task TryReadAsync_WithProblemJson_ParsesFields()
        {
            // Arrange
            var json = """
            {"type":"urn:archix:test","title":"Bad input","status":400,"detail":"name is required","instance":"/v1/items/42","x":"y"}
            """;
            using var res = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/problem+json")
            };

            // Act
            var pd = await ProblemDetailsReader.TryReadAsync(res);

            // Assert
            Assert.NotNull(pd);
            Assert.Equal("urn:archix:test", pd!.Type);
            Assert.Equal("Bad input", pd.Title);
            Assert.Equal(400, pd.Status);
            Assert.Equal("name is required", pd.Detail);
            Assert.Equal("/v1/items/42", pd.Instance);
            Assert.NotNull(pd.Extensions);
            Assert.True(pd.Extensions!.ContainsKey("x"));
        }

        [Fact]
        public async Task TryReadAsync_WithPlainJson_NotProblem_ReturnsNull()
        {
            // Arrange
            var json = """{"message":"oops"}""";
            using var res = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Act
            var pd = await ProblemDetailsReader.TryReadAsync(res);

            // Assert
            Assert.Null(pd);
        }

        [Fact]
        public void ToOneLine_ShortensDetail()
        {
            // Arrange
            var longDetail = new string('a', 600);
            var pd = new HttpApiProblem("t", "ttl", 500, longDetail, "i");

            // Act
            var line = ProblemDetailsReader.ToOneLine(pd);

            // Assert
            Assert.StartsWith("500 ttl | ", line);
            Assert.True(line.Length <= 520);
        }
    }
}
