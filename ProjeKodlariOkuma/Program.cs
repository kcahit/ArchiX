

var kokDizin = @"C:\_git\ArchiX\Dev\ArchiX"; 
var uzantilarArr = new[] { ".cs", ".csproj" };
var hedefDizin = @"C:\_git\ArchiX\notlarim";
var dosyaAdi = "proje_kodlari";
var format = "json";
int islem = 3;


/*
 * islem = 1 : Sadece TreeView olustur
 * islem = 2 : Sadece kod dosyalarini oku ve json olustur
 * islem = 3 : Hem kod dosyalarini oku hem de TreeView olustur
 * 
 */
ProjeKodlariOkuma.DosyaTarayici.DosyaOlustur(kokDizin, uzantilarArr, hedefDizin, dosyaAdi, format,islem);
//ProjeKodlariOkuma.TreeViewOlustur.TreeViewOlusturMethod(
//    Path.Combine(hedefDizin, dosyaAdi + "." + format),
//    Path.Combine(hedefDizin, dosyaAdi + "_treeview.txt")
//    );



