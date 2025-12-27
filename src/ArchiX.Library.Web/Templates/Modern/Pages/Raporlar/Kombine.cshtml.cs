using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages.Raporlar
{
    public class KombineModel : PageModel
    {
        public List<EmployeeData> SampleData { get; set; } = new();

        public void OnGet()
        {
            // Sample data
            SampleData = new List<EmployeeData>
            {
                new() { Id = 1, Name = "Ahmet Yýlmaz", Email = "ahmet@example.com", Phone = "0532 123 4567", Department = "IT", Salary = 15000, Experience = 5, City = "Istanbul", Position = "Yazýlým Geliþtirici", StartDate = "2019-03-15", Status = "Aktif" },
                new() { Id = 2, Name = "Ayþe Demir", Email = "ayse@example.com", Phone = "0533 234 5678", Department = "Satýþ", Salary = 12000, Experience = 3, City = "Ankara", Position = "Satýþ Temsilcisi", StartDate = "2021-06-20", Status = "Aktif" },
                new() { Id = 3, Name = "Mehmet Kaya", Email = "mehmet@example.com", Phone = "0534 345 6789", Department = "Ýnsan Kaynaklarý", Salary = 18000, Experience = 8, City = "Izmir", Position = "ÝK Müdürü", StartDate = "2016-01-10", Status = "Pasif" },
                new() { Id = 4, Name = "Fatma Çelik", Email = "fatma@example.com", Phone = "0535 456 7890", Department = "Pazarlama", Salary = 14000, Experience = 4, City = "Bursa", Position = "Pazarlama Uzmaný", StartDate = "2020-09-05", Status = "Beklemede" },
                new() { Id = 5, Name = "Ali Öz", Email = "ali@example.com", Phone = "0536 567 8901", Department = "IT", Salary = 16500, Experience = 6, City = "Istanbul", Position = "Sistem Yöneticisi", StartDate = "2018-11-12", Status = "Aktif" },
                new() { Id = 6, Name = "Zeynep Þahin", Email = "zeynep@example.com", Phone = "0537 678 9012", Department = "Satýþ", Salary = 11000, Experience = 2, City = "Antalya", Position = "Satýþ Danýþmaný", StartDate = "2022-02-28", Status = "Aktif" },
                new() { Id = 7, Name = "Can Arslan", Email = "can@example.com", Phone = "0538 789 0123", Department = "IT", Salary = 17000, Experience = 7, City = "Ankara", Position = "Kýdemli Geliþtirici", StartDate = "2017-05-18", Status = "Pasif" },
                new() { Id = 8, Name = "Elif Kurt", Email = "elif@example.com", Phone = "0539 890 1234", Department = "Pazarlama", Salary = 13500, Experience = 4, City = "Istanbul", Position = "Dijital Pazarlama", StartDate = "2020-07-22", Status = "Aktif" },
                new() { Id = 9, Name = "Burak Aydýn", Email = "burak@example.com", Phone = "0540 901 2345", Department = "Ýnsan Kaynaklarý", Salary = 12500, Experience = 3, City = "Izmir", Position = "ÝK Uzmaný", StartDate = "2021-04-14", Status = "Beklemede" },
                new() { Id = 10, Name = "Selin Türk", Email = "selin@example.com", Phone = "0541 012 3456", Department = "Satýþ", Salary = 13000, Experience = 5, City = "Bursa", Position = "Bölge Müdürü", StartDate = "2019-08-30", Status = "Aktif" },
                new() { Id = 11, Name = "Deniz Yýldýz", Email = "deniz@example.com", Phone = "0542 111 2222", Department = "IT", Salary = 19000, Experience = 10, City = "Istanbul", Position = "IT Müdürü", StartDate = "2014-12-01", Status = "Aktif" },
                new() { Id = 12, Name = "Ece Kartal", Email = "ece@example.com", Phone = "0543 222 3333", Department = "Pazarlama", Salary = 12000, Experience = 2, City = "Ankara", Position = "Sosyal Medya Uzmaný", StartDate = "2022-05-17", Status = "Pasif" },
                new() { Id = 13, Name = "Emre Koç", Email = "emre@example.com", Phone = "0544 333 4444", Department = "IT", Salary = 15500, Experience = 5, City = "Izmir", Position = "Veri Analisti", StartDate = "2019-10-08", Status = "Aktif" },
                new() { Id = 14, Name = "Gizem Acar", Email = "gizem@example.com", Phone = "0545 444 5555", Department = "Satýþ", Salary = 11500, Experience = 3, City = "Antalya", Position = "Satýþ Uzmaný", StartDate = "2021-03-25", Status = "Aktif" },
                new() { Id = 15, Name = "Hakan Demir", Email = "hakan@example.com", Phone = "0546 555 6666", Department = "Pazarlama", Salary = 16000, Experience = 7, City = "Istanbul", Position = "Marka Müdürü", StartDate = "2017-07-19", Status = "Aktif" },
                new() { Id = 16, Name = "Ýrem Yalçýn", Email = "irem@example.com", Phone = "0547 666 7777", Department = "Ýnsan Kaynaklarý", Salary = 14500, Experience = 6, City = "Bursa", Position = "ÝK Koordinatörü", StartDate = "2018-09-11", Status = "Aktif" },
                new() { Id = 17, Name = "Kerem Özkan", Email = "kerem@example.com", Phone = "0548 777 8888", Department = "IT", Salary = 18500, Experience = 9, City = "Ankara", Position = "Yazýlým Mimarý", StartDate = "2015-02-03", Status = "Pasif" },
                new() { Id = 18, Name = "Lale Kara", Email = "lale@example.com", Phone = "0549 888 9999", Department = "Satýþ", Salary = 13500, Experience = 4, City = "Istanbul", Position = "Kurumsal Satýþ", StartDate = "2020-11-27", Status = "Aktif" },
                new() { Id = 19, Name = "Mert Çetin", Email = "mert@example.com", Phone = "0530 999 0000", Department = "Pazarlama", Salary = 12500, Experience = 3, City = "Izmir", Position = "Ýçerik Üreticisi", StartDate = "2021-08-16", Status = "Beklemede" },
                new() { Id = 20, Name = "Nil Þen", Email = "nil@example.com", Phone = "0531 000 1111", Department = "IT", Salary = 17500, Experience = 8, City = "Ankara", Position = "DevOps Mühendisi", StartDate = "2016-06-22", Status = "Aktif" },
                new() { Id = 21, Name = "Oðuz Taþ", Email = "oguz@example.com", Phone = "0532 111 2222", Department = "Satýþ", Salary = 14000, Experience = 5, City = "Bursa", Position = "Satýþ Müdürü", StartDate = "2019-04-09", Status = "Aktif" },
                new() { Id = 22, Name = "Pelin Ay", Email = "pelin@example.com", Phone = "0533 222 3333", Department = "Ýnsan Kaynaklarý", Salary = 13000, Experience = 4, City = "Istanbul", Position = "Ýþe Alým Uzmaný", StartDate = "2020-12-14", Status = "Aktif" },
                new() { Id = 23, Name = "Rýza Ulu", Email = "riza@example.com", Phone = "0534 333 4444", Department = "Pazarlama", Salary = 15500, Experience = 6, City = "Antalya", Position = "Pazarlama Müdürü", StartDate = "2018-03-07", Status = "Pasif" },
                new() { Id = 24, Name = "Seda Akýn", Email = "seda@example.com", Phone = "0535 444 5555", Department = "IT", Salary = 16000, Experience = 6, City = "Izmir", Position = "Test Uzmaný", StartDate = "2018-08-21", Status = "Aktif" },
                new() { Id = 25, Name = "Tolga Yurt", Email = "tolga@example.com", Phone = "0536 555 6666", Department = "Satýþ", Salary = 12000, Experience = 2, City = "Ankara", Position = "Satýþ Elemaný", StartDate = "2022-01-11", Status = "Aktif" }
            };
        }
    }

    public class EmployeeData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Salary { get; set; }
        public int Experience { get; set; }
        public string City { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
