using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ArchiX.Library.Diagnostics
{
    public class ExceptionLogger
    {
        private readonly string? _logFilePath;

        public string mesaj { get; private set; } = "";

        // Detay alanlar
        public string? stackTrace { get; private set; }
        public string? helpLink { get; private set; }
        public string? source { get; private set; }
        public string? targetSite { get; private set; }
        public string? innerException { get; private set; }
        public string? hResult { get; private set; }
        public string? data { get; private set; }
        public string detayMesaj { get; private set; } = "";

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

        // İsteğe bağlı log dosyası yolu
        public ExceptionLogger(string path = "error_log.txt")
        {
            _logFilePath = path;
        }

        // Hataları dosyaya kaydetmek istersen aç
        public void LogException(Exception ex)
        {
            // var logMessage = $"[{DateTimeOffset.UtcNow:u}] {ex.GetType()} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
            // File.AppendAllText(_logFilePath ?? "error_log.txt", logMessage);
        }

        // Özel hata tipine göre mesaj üret
        public string HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => "Null değer içeriyor.",
                ArgumentOutOfRangeException => "Argüman belirtilen aralığın dışındadır.",
                ArgumentException => "Geçersiz veya eksik argüman.", // <-- eklendi
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
