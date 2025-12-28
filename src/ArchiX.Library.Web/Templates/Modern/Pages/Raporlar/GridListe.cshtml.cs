using System.Collections.Generic;
using System.Linq;
using ArchiX.Library.Web.ViewModels.Grid;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;

public class GridListeModel : PageModel
{
    public IReadOnlyList<GridColumnDefinition> Columns { get; private set; } = new List<GridColumnDefinition>();
    public IEnumerable<IDictionary<string, object?>> Rows { get; private set; } = Enumerable.Empty<IDictionary<string, object?>>();

    public void OnGet()
    {
        Columns = new List<GridColumnDefinition>
        {
            new("id", "ID", DataType: "number", Width: "70px"),
            new("name", "Ýsim"),
            new("email", "Email"),
            new("phone", "Telefon"),
            new("department", "Departman"),
            new("salary", "Maaþ", DataType: "number"),
            new("experience", "Tecrübe", DataType: "number"),
            new("city", "Þehir"),
            new("position", "Pozisyon"),
            new("startDate", "Baþlangýç"),
            new("status", "Durum")
        };

        var data = new[]
        {
            new { id = 1, name = "Ahmet Yýlmaz", email = "ahmet@example.com", phone = "0532 123 4567", department = "IT", salary = 15000, experience = 5, city = "Istanbul", position = "Yazýlým Geliþtirici", startDate = "2019-03-15", status = "Aktif" },
            new { id = 2, name = "Ayþe Demir", email = "ayse@example.com", phone = "0533 234 5678", department = "Satýþ", salary = 12000, experience = 3, city = "Ankara", position = "Satýþ Temsilcisi", startDate = "2021-06-20", status = "Aktif" },
            new { id = 3, name = "Mehmet Kaya", email = "mehmet@example.com", phone = "0534 345 6789", department = "Ýnsan Kaynaklarý", salary = 18000, experience = 8, city = "Izmir", position = "ÝK Müdürü", startDate = "2016-01-10", status = "Pasif" },
            new { id = 4, name = "Fatma Çelik", email = "fatma@example.com", phone = "0535 456 7890", department = "Pazarlama", salary = 14000, experience = 4, city = "Bursa", position = "Pazarlama Uzmaný", startDate = "2020-09-05", status = "Beklemede" },
            new { id = 5, name = "Ali Öz", email = "ali@example.com", phone = "0536 567 8901", department = "IT", salary = 16500, experience = 6, city = "Istanbul", position = "Sistem Yöneticisi", startDate = "2018-11-12", status = "Aktif" }
        };

        Rows = data.Select(d => new Dictionary<string, object?>
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
            ["startDate"] = d.startDate,
            ["status"] = d.status
        }).ToList();
    }
}
