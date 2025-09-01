using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ArchiX.Library.Diagnostics
{
    /// <summary>
    /// Exception nesnelerinden log ve kullanıcıya uygun mesaj üretmek için kullanılan yardımcı sınıf.
    /// </summary>
    public class ExceptionLogger
    {
        /// <summary>
        /// Log dosyası yolu (opsiyonel).
        /// </summary>
        private readonly string? _logFilePath;

        /// <summary>
        /// Son üretilen mesaj.
        /// </summary>
        public string mesaj { get; private set; } = "";

        /// <summary>
        /// Exception stack trace bilgisi.
        /// </summary>
        public string? stackTrace { get; private set; }

        /// <summary>
        /// Exception yardım bağlantısı.
        /// </summary>
        public string? helpLink { get; private set; }

        /// <summary>
        /// Exception kaynağı.
        /// </summary>
        public string? source { get; private set; }

        /// <summary>
        /// Exception’un gerçekleştiği metot.
        /// </summary>
        public string? targetSite { get; private set; }

        /// <summary>
        /// İç exception bilgisi.
        /// </summary>
        public string? innerException { get; private set; }

        /// <summary>
        /// Exception HResult değeri.
        /// </summary>
        public string? hResult { get; private set; }

        /// <summary>
        /// Exception data içeriği.
        /// </summary>
        public string? data { get; private set; }

        /// <summary>
        /// Detaylı hata mesajı.
        /// </summary>
        public string detayMesaj { get; private set; } = "";

        /// <summary>
        /// Exception nesnesinden ExceptionLogger oluşturur.
        /// </summary>
        /// <param name="exception">Yakalanan exception nesnesi.</param>
        public ExceptionLogger(Exception exception)
        {
            stackTrace = exception.StackTrace;
            helpLink = exception.HelpLink;
            source = exception.Source;
            targetSite = exception.TargetSite?.ToString();
            innerException = exception.InnerException?.ToString();
            hResult = exception.HResult.ToString();
            data = exception.Data?.ToString();
            detayMesaj = exception.Message;

            mesaj = HandleException(exception);
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
        /// Exception detaylarını log dosyasına kaydeder (yorum satırında).
        /// </summary>
        /// <param name="ex">Yakalanan exception nesnesi.</param>
        public void LogException(Exception ex)
        {
            // var logMessage = $"[{DateTimeOffset.UtcNow:u}] {ex.GetType()} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
            // File.AppendAllText(_logFilePath ?? "error_log.txt", logMessage);
        }

        /// <summary>
        /// Exception tipine göre kullanıcı dostu mesaj döndürür.
        /// </summary>
        /// <param name="ex">Yakalanan exception nesnesi.</param>
        /// <returns>Kullanıcıya uygun mesaj.</returns>
        public string HandleException(Exception ex)
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
