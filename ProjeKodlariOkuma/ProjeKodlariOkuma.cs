namespace ProjeKodlariOkuma
{
    internal class ProjeKodlariOkuma
    {
     
        internal static void KodlariOLustur(string kokDizin, string[] uzantilar, string hedefDizin, string format, bool ciktiAyrıDosyalarmi=true)
        {
            ArgumentException.ThrowIfNullOrEmpty(kokDizin);
            ArgumentNullException.ThrowIfNull(uzantilar);
            ArgumentException.ThrowIfNullOrEmpty(hedefDizin);
            ArgumentException.ThrowIfNullOrEmpty(format);
            Console.WriteLine($"Kök Dizin: {kokDizin}");
            Console.WriteLine($"Uzantılar: {string.Join(", ", uzantilar)}");
            Console.WriteLine($"Hedef Dizin: {hedefDizin}");
            Console.WriteLine($"Format: {format}");
            Console.WriteLine($"Ayrı Dosyalar: {ciktiAyrıDosyalarmi}");  
            Console.WriteLine($"");
            /*
             * Kök dizine git
             * uzantılara göre dosyaları bul
             * her dosyayı oku 
             * BUna göre Proje adı, dizin yapısı, dosya adları, kod içeriklerini al ve json formatında kaydet ya da njson formatında bir çıktı oluştur
             * bu kodlarda türkçe karakter sorunu olmamalı, esc karakterleri doğru işlenmeli, tab karakterleri doğru işlenmeli gibi detaylara dikkat et.
             * 

            */

            Console.ReadLine();
        }

   

    }
}
