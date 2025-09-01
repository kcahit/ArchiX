using Xunit;
using ArchiX.Library.Result;

namespace ArchiXTest.ApiWeb.Tests.CommonTests
{
    public class ResultGenericTests
    {
        [Fact]
        public void Success_Should_Return_Success_Result_With_Value()
        {
            // Act
            var result = Result<string>.Success("ok");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal("ok", result.Value);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Failure_Should_Return_Failure_Result_With_Error()
        {
            // Arrange
            var error = new Error("VAL002", "Geçersiz değer");

            // Act
            var result = Result<string>.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal(error, result.Error);
        }
    }
}
