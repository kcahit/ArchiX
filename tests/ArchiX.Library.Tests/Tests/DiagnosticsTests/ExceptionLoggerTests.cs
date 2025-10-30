using ArchiX.Library.Diagnostics;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.DiagnosticsTests
{
    public sealed class ExceptionLoggerTests
    {
        [Theory]
        [InlineData(typeof(ArgumentNullException), "Null değer içeriyor.")]
        [InlineData(typeof(ArgumentOutOfRangeException), "Argüman belirtilen aralığın dışındadır.")]
        [InlineData(typeof(ArgumentException), "Geçersiz veya eksik argüman.")]
        [InlineData(typeof(IndexOutOfRangeException), "Dizi sınırı aşıldı! Veriyi kontrol et.")]
        [InlineData(typeof(DivideByZeroException), "Sıfıra bölme hatası.")]
        [InlineData(typeof(FormatException), "Yanlış formatta veri. Örn: string'i sayıya çevirme.")]
        [InlineData(typeof(NullReferenceException), "Null bir nesneye erişilmeye çalışılıyor.")]
        [InlineData(typeof(InvalidOperationException), "Nesne geçersiz bir durumda. Örn: kapalı bir kaynağı kullanmak.")]
        public void HandleException_ShouldReturnExpectedMessage(Type exceptionType, string expected)
        {
            // Arrange
            var ex = (Exception)Activator.CreateInstance(exceptionType)!;

            // Act
            var logger = new ExceptionLogger(ex);

            // Assert
            Assert.Equal(expected, logger.Mesaj);
        }

        [Fact]
        public void HandleException_ShouldReturnBilinmeyenHata_ForUnknownException()
        {
            // Arrange
            var ex = new ApplicationException("Custom error");

            // Act
            var logger = new ExceptionLogger(ex);

            // Assert
            Assert.Equal("Bilinmeyen hata", logger.Mesaj);
            Assert.Equal("Custom error", logger.DetayMesaj);
            Assert.Equal(typeof(ApplicationException).FullName, ex.GetType().FullName);
        }
    }
}
