using ProjeKodlariOkuma;

class Program
{
    static void Main()
    {
        var kokDizin = @"C:\_git\ArchiX\Dev\ArchiX";
        var uzantilarArr = new[] { ".cs", ".csproj" };
        var hedefDizin = @"C:\_git\ArchiX\notlarim\Ciktilar";
        var dosyaAdi = "proje_kodlari";
        var format = "ndjson";

        // JsonDosyaUret.Uret(kokDizin, uzantilarArr, hedefDizin, dosyaAdi, format);
        JsonDosyaUret_Parcali.Uret(kokDizin, uzantilarArr, hedefDizin, dosyaAdi + "_parcali", format);

        Console.WriteLine();
        Console.WriteLine("Çıkmak için herhangi bir tuşa basın...");
        Console.ReadKey(intercept: true);
    }
}

//var kokDizin = @"C:\_git\ArchiX\Dev\ArchiX"; 
//var uzantilarArr = new[] { ".cs", ".csproj" };
//var hedefDizin = @"C:\_git\ArchiX\notlarim";
//var dosyaAdi = "proje_kodlari";
//var format = "ndjson";


///*
// * islem = 1 : Sadece TreeView olustur
// * islem = 2 : Sadece kod dosyalarini oku ve json olustur
// * islem = 3 : Hem kod dosyalarini oku hem de TreeView olustur
// * 
// */
//int islem = 2;

//ProjeKodlariOkuma.DosyaTarayici.DosyaOlustur(kokDizin, uzantilarArr, hedefDizin, dosyaAdi, format,islem);
////ProjeKodlariOkuma.TreeViewOlustur.TreeViewOlusturMethod(
////    Path.Combine(hedefDizin, dosyaAdi + "." + format),
////    Path.Combine(hedefDizin, dosyaAdi + "_treeview.txt")
////    );



