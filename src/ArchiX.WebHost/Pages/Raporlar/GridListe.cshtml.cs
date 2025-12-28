using System.Collections.Generic;
using System.Linq;
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
            new("startDate", "Ba�lang��"),
            new("status", "Durum")
        };

        var data = new[]
        {
            new { id = 1, name = "Ahmet Y�lmaz", email = "ahmet@example.com", phone = "0532 123 4567", department = "IT", salary = 15000, experience = 5, city = "Istanbul", position = "Yaz�l�m Geli�tirici", startDate = "2019-03-15", status = "Aktif" },
            new { id = 2, name = "Ay�e Demir", email = "ayse@example.com", phone = "0533 234 5678", department = "Sat��", salary = 12000, experience = 3, city = "Ankara", position = "Sat�� Temsilcisi", startDate = "2021-06-20", status = "Aktif" },
            new { id = 3, name = "Mehmet Kaya", email = "mehmet@example.com", phone = "0534 345 6789", department = "�nsan Kaynaklar�", salary = 18000, experience = 8, city = "Izmir", position = "�K M�d�r�", startDate = "2016-01-10", status = "Pasif" },
            new { id = 4, name = "Fatma �elik", email = "fatma@example.com", phone = "0535 456 7890", department = "Pazarlama", salary = 14000, experience = 4, city = "Bursa", position = "Pazarlama Uzman�", startDate = "2020-09-05", status = "Beklemede" },
            new { id = 5, name = "Ali �z", email = "ali@example.com", phone = "0536 567 8901", department = "IT", salary = 16500, experience = 6, city = "Istanbul", position = "Sistem Y�neticisi", startDate = "2018-11-12", status = "Aktif" }
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
