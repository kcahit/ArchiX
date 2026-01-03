using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.WebHost.Pages.Raporlar;

public class GridListeModel : PageModel
{
    public IReadOnlyList<GridColumnDefinition> Columns { get; private set; } = new List<GridColumnDefinition>();
    public IEnumerable<IDictionary<string, object?>> Rows { get; private set; } = Enumerable.Empty<IDictionary<string, object?>>();

    public void OnGet()
    {
        Columns = new List<GridColumnDefinition>
        {
            new("id", "ID", DataType: "number", Width: "70px"),
            new("name", "�sim"),
            new("email", "Email"),
            new("phone", "Telefon"),
            new("department", "Departman"),
            new("salary", "Maa�", DataType: "number"),
            new("experience", "Tecr�be", DataType: "number"),
            new("city", "�ehir"),
            new("position", "Pozisyon"),
            new("country", "�lke"),
            new("team", "Tak�m"),
            new("level", "Seviye"),
            new("manager", "Y�netici"),
            new("office", "Ofis"),
            new("startDate", "Ba�lang��"),
            new("status", "Durum")
        };

        var baseData = new[]
        {
            new { id = 1, name = "Ahmet Y�lmaz", email = "ahmet@example.com", phone = "0532 123 4567", department = "IT", salary = 15000, experience = 5, city = "Istanbul", position = "Yaz�l�m Geli�tirici", startDate = "2019-03-15", status = "Aktif" },
            new { id = 2, name = "Ay�e Demir", email = "ayse@example.com", phone = "0533 234 5678", department = "Sat��", salary = 12000, experience = 3, city = "Ankara", position = "Sat�� Temsilcisi", startDate = "2021-06-20", status = "Aktif" },
            new { id = 3, name = "Mehmet Kaya", email = "mehmet@example.com", phone = "0534 345 6789", department = "�nsan Kaynaklar�", salary = 18000, experience = 8, city = "Izmir", position = "�K M�d�r�", startDate = "2016-01-10", status = "Pasif" },
            new { id = 4, name = "Fatma �elik", email = "fatma@example.com", phone = "0535 456 7890", department = "Pazarlama", salary = 14000, experience = 4, city = "Bursa", position = "Pazarlama Uzman�", startDate = "2020-09-05", status = "Beklemede" },
            new { id = 5, name = "Ali �z", email = "ali@example.com", phone = "0536 567 8901", department = "IT", salary = 16500, experience = 6, city = "Istanbul", position = "Sistem Y�neticisi", startDate = "2018-11-12", status = "Aktif" },
            new { id = 6, name = "Zeynep �ahin", email = "zeynep@example.com", phone = "0537 678 9012", department = "Sat��", salary = 11000, experience = 2, city = "Antalya", position = "Sat�� Dan��man�", startDate = "2022-02-28", status = "Aktif" },
            new { id = 7, name = "Can Arslan", email = "can@example.com", phone = "0538 789 0123", department = "IT", salary = 17000, experience = 7, city = "Ankara", position = "K�demli Geli�tirici", startDate = "2017-05-18", status = "Pasif" },
            new { id = 8, name = "Elif Kurt", email = "elif@example.com", phone = "0539 890 1234", department = "Pazarlama", salary = 13500, experience = 4, city = "Istanbul", position = "Dijital Pazarlama", startDate = "2020-07-22", status = "Aktif" },
            new { id = 9, name = "Burak Ayd�n", email = "burak@example.com", phone = "0540 901 2345", department = "�nsan Kaynaklar�", salary = 12500, experience = 3, city = "Izmir", position = "�K Uzman�", startDate = "2021-04-14", status = "Beklemede" },
            new { id = 10, name = "Selin T�rk", email = "selin@example.com", phone = "0541 012 3456", department = "Sat��", salary = 13000, experience = 5, city = "Bursa", position = "B�lge M�d�r�", startDate = "2019-08-30", status = "Aktif" },
            new { id = 11, name = "Deniz Y�ld�z", email = "deniz@example.com", phone = "0542 111 2222", department = "IT", salary = 19000, experience = 10, city = "Istanbul", position = "IT M�d�r�", startDate = "2014-12-01", status = "Aktif" },
            new { id = 12, name = "Ece Kartal", email = "ece@example.com", phone = "0543 222 3333", department = "Pazarlama", salary = 12000, experience = 2, city = "Ankara", position = "Sosyal Medya Uzman�", startDate = "2022-05-17", status = "Pasif" },
            new { id = 13, name = "Emre Ko�", email = "emre@example.com", phone = "0544 333 4444", department = "IT", salary = 15500, experience = 5, city = "Izmir", position = "Veri Analisti", startDate = "2019-10-08", status = "Aktif" },
            new { id = 14, name = "Gizem Acar", email = "gizem@example.com", phone = "0545 444 5555", department = "Sat��", salary = 11500, experience = 3, city = "Antalya", position = "Sat�� Uzman�", startDate = "2021-03-25", status = "Aktif" },
            new { id = 15, name = "Hakan Demir", email = "hakan@example.com", phone = "0546 555 6666", department = "Pazarlama", salary = 16000, experience = 7, city = "Istanbul", position = "Marka M�d�r�", startDate = "2017-07-19", status = "Aktif" },
            new { id = 16, name = "�rem Yal��n", email = "irem@example.com", phone = "0547 666 7777", department = "�nsan Kaynaklar�", salary = 14500, experience = 6, city = "Bursa", position = "�K Koordinat�r�", startDate = "2018-09-11", status = "Aktif" },
            new { id = 17, name = "Kerem �zkan", email = "kerem@example.com", phone = "0548 777 8888", department = "IT", salary = 18500, experience = 9, city = "Ankara", position = "Yaz�l�m Mimar�", startDate = "2015-02-03", status = "Pasif" },
            new { id = 18, name = "Lale Kara", email = "lale@example.com", phone = "0549 888 9999", department = "Sat��", salary = 13500, experience = 4, city = "Istanbul", position = "Kurumsal Sat��", startDate = "2020-11-27", status = "Aktif" },
            new { id = 19, name = "Mert �etin", email = "mert@example.com", phone = "0530 999 0000", department = "Pazarlama", salary = 12500, experience = 3, city = "Izmir", position = "��erik �reticisi", startDate = "2021-08-16", status = "Beklemede" },
            new { id = 20, name = "Nil �en", email = "nil@example.com", phone = "0531 000 1111", department = "IT", salary = 17500, experience = 8, city = "Ankara", position = "DevOps M�hendisi", startDate = "2016-06-22", status = "Aktif" },
            new { id = 21, name = "O�uz Ta�", email = "oguz@example.com", phone = "0532 111 2222", department = "Sat��", salary = 14000, experience = 5, city = "Bursa", position = "Sat�� M�d�r�", startDate = "2019-04-09", status = "Aktif" },
            new { id = 22, name = "Pelin Ay", email = "pelin@example.com", phone = "0533 222 3333", department = "�nsan Kaynaklar�", salary = 13000, experience = 4, city = "Istanbul", position = "��e Al�m Uzman�", startDate = "2020-12-14", status = "Aktif" },
            new { id = 23, name = "R�za Ulu", email = "riza@example.com", phone = "0534 333 4444", department = "Pazarlama", salary = 15500, experience = 6, city = "Antalya", position = "Pazarlama M�d�r�", startDate = "2018-03-07", status = "Pasif" },
            new { id = 24, name = "Seda Ak�n", email = "seda@example.com", phone = "0535 444 5555", department = "IT", salary = 16000, experience = 6, city = "Izmir", position = "Test Uzman�", startDate = "2018-08-21", status = "Aktif" },
            new { id = 25, name = "Tolga Yurt", email = "tolga@example.com", phone = "0536 555 6666", department = "Sat��", salary = 12000, experience = 2, city = "Ankara", position = "Sat�� Eleman�", startDate = "2022-01-11", status = "Aktif" },
            new { id = 26, name = "Ufuk Ayd�n", email = "ufuk@example.com", phone = "0537 666 7777", department = "Pazarlama", salary = 14500, experience = 5, city = "Istanbul", position = "SEO Uzman�", startDate = "2019-09-26", status = "Aktif" },
            new { id = 27, name = "Vildan Er", email = "vildan@example.com", phone = "0538 777 8888", department = "�nsan Kaynaklar�", salary = 11500, experience = 2, city = "Bursa", position = "�K Asistan�", startDate = "2022-07-04", status = "Beklemede" },
            new { id = 28, name = "Ya�mur K�l��", email = "yagmur@example.com", phone = "0539 888 9999", department = "IT", salary = 20000, experience = 12, city = "Istanbul", position = "CTO", startDate = "2012-10-15", status = "Aktif" },
            new { id = 29, name = "Zafer G�ne�", email = "zafer@example.com", phone = "0540 999 0000", department = "Sat��", salary = 15000, experience = 7, city = "Ankara", position = "Sat�� Direkt�r�", startDate = "2017-11-30", status = "Aktif" },
            new { id = 30, name = "Asl� �zt�rk", email = "asli@example.com", phone = "0541 000 1111", department = "Pazarlama", salary = 13000, experience = 4, city = "Izmir", position = "Reklam Uzman�", startDate = "2020-05-13", status = "Pasif" },
            new { id = 31, name = "Baran �ak�r", email = "baran@example.com", phone = "0542 111 2222", department = "IT", salary = 17000, experience = 7, city = "Bursa", position = "G�venlik Uzman�", startDate = "2017-02-28", status = "Aktif" },
            new { id = 32, name = "Canan Tekin", email = "canan@example.com", phone = "0543 222 3333", department = "�nsan Kaynaklar�", salary = 16500, experience = 8, city = "Istanbul", position = "�K Direkt�r�", startDate = "2016-08-08", status = "Aktif" },
            new { id = 33, name = "Deniz Arslan", email = "deniz2@example.com", phone = "0544 333 4444", department = "Sat��", salary = 11000, experience = 1, city = "Antalya", position = "Stajyer", startDate = "2023-03-01", status = "Aktif" },
            new { id = 34, name = "Eda Polat", email = "eda@example.com", phone = "0545 444 5555", department = "Pazarlama", salary = 12500, experience = 3, city = "Ankara", position = "Grafik Tasar�mc�", startDate = "2021-09-20", status = "Beklemede" },
            new { id = 35, name = "Fikret Y�ld�r�m", email = "fikret@example.com", phone = "0546 555 6666", department = "IT", salary = 16500, experience = 6, city = "Izmir", position = "Network Uzman�", startDate = "2018-04-17", status = "Aktif" },
            new { id = 36, name = "G�l Ta�k�n", email = "gul@example.com", phone = "0547 666 7777", department = "Sat��", salary = 14500, experience = 6, city = "Istanbul", position = "�hracat M�d�r�", startDate = "2018-10-23", status = "Aktif" },
            new { id = 37, name = "Halil Kurt", email = "halil@example.com", phone = "0548 777 8888", department = "Pazarlama", salary = 18000, experience = 9, city = "Bursa", position = "CMO", startDate = "2015-12-05", status = "Aktif" },
            new { id = 38, name = "�pek �al��kan", email = "ipek@example.com", phone = "0549 888 9999", department = "�nsan Kaynaklar�", salary = 12000, experience = 3, city = "Ankara", position = "E�itim Uzman�", startDate = "2021-07-14", status = "Pasif" },
            new { id = 39, name = "Kaan Durmu�", email = "kaan@example.com", phone = "0530 999 0000", department = "IT", salary = 15000, experience = 5, city = "Izmir", position = "Mobil Geli�tirici", startDate = "2019-05-29", status = "Aktif" },
            new { id = 40, name = "Leyla Berk", email = "leyla@example.com", phone = "0531 000 1111", department = "Sat��", salary = 13500, experience = 4, city = "Istanbul", position = "E-ticaret Uzman�", startDate = "2020-08-11", status = "Aktif" },
            new { id = 41, name = "Mustafa Eren", email = "mustafa@example.com", phone = "0532 111 2222", department = "Pazarlama", salary = 14000, experience = 5, city = "Antalya", position = "Etkinlik Y�neticisi", startDate = "2019-06-18", status = "Beklemede" },
            new { id = 42, name = "Nalan Kaya", email = "nalan@example.com", phone = "0533 222 3333", department = "IT", salary = 19500, experience = 11, city = "Ankara", position = "Proje M�d�r�", startDate = "2013-09-10", status = "Aktif" },
            new { id = 43, name = "Onur Ayd�n", email = "onur@example.com", phone = "0534 333 4444", department = "Sat��", salary = 12500, experience = 3, city = "Bursa", position = "M��teri �li�kileri", startDate = "2021-11-22", status = "Aktif" },
            new { id = 44, name = "P�nar Y�ksel", email = "pinar@example.com", phone = "0535 444 5555", department = "�nsan Kaynaklar�", salary = 13500, experience = 4, city = "Istanbul", position = "Performans Uzman�", startDate = "2020-10-06", status = "Aktif" },
            new { id = 45, name = "Recep �im�ek", email = "recep@example.com", phone = "0536 555 6666", department = "Pazarlama", salary = 11500, experience = 2, city = "Izmir", position = "Video Edit�r�", startDate = "2022-04-19", status = "Pasif" },
            new { id = 46, name = "Selin �zer", email = "selin2@example.com", phone = "0537 666 7777", department = "IT", salary = 16000, experience = 6, city = "Ankara", position = "Veri Bilimci", startDate = "2018-07-12", status = "Aktif" },
            new { id = 47, name = "Taner Aslan", email = "taner@example.com", phone = "0538 777 8888", department = "Sat��", salary = 17000, experience = 9, city = "Istanbul", position = "Anahtar M��teri", startDate = "2015-05-25", status = "Aktif" },
            new { id = 48, name = "�mit Karaca", email = "umit@example.com", phone = "0539 888 9999", department = "Pazarlama", salary = 13000, experience = 3, city = "Bursa", position = "PR Uzman�", startDate = "2021-12-08", status = "Aktif" },
            new { id = 49, name = "Volkan �zdemir", email = "volkan@example.com", phone = "0540 999 0000", department = "�nsan Kaynaklar�", salary = 19000, experience = 10, city = "Antalya", position = "�K Genel M�d�r�", startDate = "2014-03-20", status = "Aktif" },
            new { id = 50, name = "Yasemin Tan", email = "yasemin@example.com", phone = "0541 000 1111", department = "IT", salary = 18000, experience = 8, city = "Izmir", position = "UX/UI Tasar�mc�", startDate = "2016-11-14", status = "Beklemede" },
            new { id = 51, name = "Zeki Bulut", email = "zeki@example.com", phone = "0542 111 2222", department = "Sat��", salary = 14500, experience = 5, city = "Ankara", position = "Teknik Sat��", startDate = "2019-02-07", status = "Aktif" },
            new { id = 52, name = "Aylin Erdo�an", email = "aylin@example.com", phone = "0543 222 3333", department = "Pazarlama", salary = 15000, experience = 6, city = "Istanbul", position = "Marka Stratejisti", startDate = "2018-06-30", status = "Aktif" },
            new { id = 53, name = "Bar�� Yaman", email = "baris@example.com", phone = "0544 333 4444", department = "IT", salary = 21000, experience = 13, city = "Bursa", position = "Ba�kan Yard�mc�s�", startDate = "2011-08-16", status = "Aktif" },
            new { id = 54, name = "Ceyda Ko�ak", email = "ceyda@example.com", phone = "0545 444 5555", department = "Sat��", salary = 13000, experience = 4, city = "Antalya", position = "Bayi Y�neticisi", startDate = "2020-03-24", status = "Pasif" },
            new { id = 55, name = "Do�an �ener", email = "dogan@example.com", phone = "0546 555 6666", department = "�nsan Kaynaklar�", salary = 15500, experience = 7, city = "Izmir", position = "Bordro Uzman�", startDate = "2017-09-02", status = "Aktif" }
        };

        var levels = new[] { "Junior", "Mid", "Senior" };

        Rows = baseData
            .Select((d, i) => new Dictionary<string, object?>
            {
                ["id"] = d.id,
                ["name"] = d.name,
                ["email"] = d.email,
                ["phone"] = d.phone,
                ["department"] = d.department,
                ["salary"] = d.salary,
                ["experience"] = d.experience,
                ["city"] = d.city,
                ["position"] = d.position,
                ["country"] = "T�rkiye",
                ["team"] = $"Tak�m {(i % 5) + 1}",
                ["level"] = levels[i % levels.Length],
                ["manager"] = $"Y�netici {(i % 8) + 1}",
                ["office"] = $"Ofis {(i % 4) + 1}",
                ["startDate"] = d.startDate,
                ["status"] = d.status
            })
            .ToList();
    }
}
