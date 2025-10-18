using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ArchiX.Library.Diagnostics
{
    /// <summary>
    /// Exception nesnelerinden log ve kullanıcıya uygun mesaj üretmek için kullanılan yardımcı sınıf.
    /// </summary>
    public sealed class ExceptionLogger
    {
        /// <summary>
        /// Log dosyası yolu (opsiyonel).
        /// </summary>
        private readonly string? _logFilePath;

        /// <summary>
        /// Son üretilen mesaj.
        /// </summary>
        public string Mesaj { get; private set; } = string.Empty;

        /// <summary>
        /// Exception stack trace bilgisi.
        /// </summary>
        public string? StackTrace { get; private set; }

        /// <summary>
        /// Exception yardım bağlantısı.
        /// </summary>
        public string? HelpLink { get; private set; }

        /// <summary>
        /// Exception kaynağı.
        /// </summary>
        public string? Source { get; private set; }

        /// <summary>
        /// Exception’un gerçekleştiği metot.
        /// </summary>
        public string? TargetSite { get; private set; }

        /// <summary>
        /// İç exception bilgisi.
        /// </summary>
        public string? InnerException { get; private set; }

        /// <summary>
        /// Exception HResult değeri.
        /// </summary>
        public string? HResult { get; private set; }

        /// <summary>
        /// Exception data içeriği.
        /// </summary>
        public string? Data { get; private set; }

        /// <summary>
        /// Detaylı hata mesajı.
        /// </summary>
        public string DetayMesaj { get; private set; } = string.Empty;

        /// <summary>
        /// Exception nesnesinden ExceptionLogger oluşturur.
        /// </summary>
        /// <param name="exception">Yakalanan exception nesnesi.</param>
        public ExceptionLogger(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception); // CA1510

            StackTrace = exception.StackTrace;
            HelpLink = exception.HelpLink;
            Source = exception.Source;
            TargetSite = exception.TargetSite?.ToString();
            InnerException = exception.InnerException?.ToString();
            HResult = exception.HResult.ToString();
            Data = exception.Data?.ToString();
            DetayMesaj = exception.Message;

            Mesaj = HandleException(exception);
        }

        /// <summary>
        /// İsteğe bağlı log dosyası yoluyla ExceptionLogger oluşturur.
        /// </summary>
        /// <param name="path">Log dosyası yolu (varsayılan: error_log.txt).</param>
        public ExceptionLogger(string path = "error_log.txt")
        {
            _logFilePath = path;
        }

        /// <summary>
        /// Exception detaylarını log dosyasına kaydetmek için iskelet metod.
        /// </summary>
        /// <param name="_">Kullanılmayan parametre.</param>
        public static void LogException(Exception _)
        {
            // var logMessage = $"[{DateTimeOffset.UtcNow:u}] {_.GetType()} - {_.Message}{Environment.NewLine}{_.StackTrace}{Environment.NewLine}";
            // File.AppendAllText("error_log.txt", logMessage);
        }

        /// <summary>
        /// Exception tipine göre kullanıcı dostu mesaj döndürür.
        /// </summary>
        /// <param name="ex">Yakalanan exception nesnesi.</param>
        /// <returns>Kullanıcıya uygun mesaj.</returns>
        private static string HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => "Null değer içeriyor.",
                ArgumentOutOfRangeException => "Argüman belirtilen aralığın dışındadır.",
                ArgumentException => "Geçersiz veya eksik argüman.",
                IndexOutOfRangeException => "Dizi sınırı aşıldı! Veriyi kontrol et.",
                InvalidOperationException => "Nesne geçersiz bir durumda. Örn: kapalı bir kaynağı kullanmak.",
                NullReferenceException => "Null bir nesneye erişilmeye çalışılıyor.",
                DivideByZeroException => "Sıfıra bölme hatası.",
                FormatException => "Yanlış formatta veri. Örn: string'i sayıya çevirme.",
                FileNotFoundException => "Belirtilen dosya bulunamadı.",
                DirectoryNotFoundException => "Klasör bulunamadı.",
                UnauthorizedAccessException => "Dosya/işlem için yetki sorunu.",
                IOException => "Dosya işlemlerinde hata.",
                OutOfMemoryException => "Yetersiz bellek.",
                StackOverflowException => "Sonsuz döngü / stack taşması.",
                AccessViolationException => "Geçersiz bellek erişimi.",
                TypeInitializationException => "Sınıfın static ctor'u düzgün çalışmadı.",
                NotSupportedException => "Desteklenmeyen bir işlem.",
                SocketException => "Ağ bağlantı hatası.",
                ThreadAbortException => "Bir thread program tarafından sonlandırıldı.",
                TaskCanceledException => "Async işlemler iptal edildi.",
                SynchronizationLockException => "Yanlış senkronizasyon kullanımı.",
                InvalidCastException => "Yanlış tür dönüştürme (örn: (string)obj).",
                OverflowException => "Değer türün sınırlarını aştı (örn: int.MaxValue + 1).",
                CryptographicException => "Şifreleme işlemi hatası.",
                KeyNotFoundException => "Dictionary anahtarı bulunamadı.",
                COMException => "COM bileşeni hatası.",
                DllNotFoundException => "Eksik DLL var.",
                _ => "Bilinmeyen hata"
            };
        }
    }
}
