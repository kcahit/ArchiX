using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;

public class GridListeModel : PageModel
{
    private readonly IReportDatasetExecutor _executor;
    private readonly IReportDatasetOptionService _optionsSvc;

    public GridListeModel(IReportDatasetExecutor executor, IReportDatasetOptionService optionsSvc)
    {
        _executor = executor;
        _optionsSvc = optionsSvc;
    }

    public IReadOnlyList<GridColumnDefinition> Columns { get; private set; } = new List<GridColumnDefinition>();
    public IEnumerable<IDictionary<string, object?>> Rows { get; private set; } = [];

    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; private set; } = [];
    public int? SelectedReportDatasetId { get; private set; }

    public async Task OnGetAsync([FromQuery] int? reportDatasetId, CancellationToken ct)
    {
        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);

        if (!reportDatasetId.HasValue || reportDatasetId.Value <= 0)
        {
            LoadSampleData();
            return;
        }

        SelectedReportDatasetId = reportDatasetId.Value;

        // fail-closed: Approved olmayan dataset seçilirse sayfa boş/örnek data ile devam etmez.
        if (!DatasetOptions.Any(x => x.Id == reportDatasetId.Value))
            return;

        try
        {
            var result = await _executor.ExecuteAsync(new ReportDatasetExecutionRequest(reportDatasetId.Value), ct);

            Columns = result.Columns
                .Select(c => new GridColumnDefinition(c, c))
                .ToList();

            Rows = result.Rows
                .Select(r =>
                {
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    for (var i = 0; i < result.Columns.Count; i++)
                    {
                        dict[result.Columns[i]] = r[i];
                    }

                    return (IDictionary<string, object?>)dict;
                })
                .ToList();
        }
        catch
        {
            // GET tarafında hata fırlatmayalım; sayfa render edilsin.
        }
    }

    public async Task<IActionResult> OnPostRunAsync([FromForm] int reportDatasetId, CancellationToken ct)
    {
        // Unit tests instantiate the PageModel without HttpContext; Request will be null.
        var hasForm = Request?.HasFormContentType == true;

        if (reportDatasetId <= 0)
            return new BadRequestResult();

        // fail-closed: ApprovedOnly kontrolü (UI bypass edilirse bile)
        List<ReportDatasetOptionViewModel> opts = (await _optionsSvc.GetApprovedOptionsAsync(ct)).ToList();
        if (!opts.Any(x => x.Id == reportDatasetId))
            return new BadRequestResult();

        Dictionary<string, string?> parameters = hasForm
            ? ExtractParametersFromForm(Request!.Form)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            ReportDatasetExecutionResult result = await _executor.ExecuteAsync(
                new ReportDatasetExecutionRequest(reportDatasetId, Parameters: parameters),
                ct);

            Columns = result.Columns
                .Select(c => new GridColumnDefinition(c, c))
                .ToList();

            Rows = result.Rows
                .Select(r =>
                {
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    for (var i = 0; i < result.Columns.Count; i++)
                    {
                        dict[result.Columns[i]] = r[i];
                    }

                    return (IDictionary<string, object?>)dict;
                })
                .ToList();

            return new OkResult();
        }
        catch
        {
            return new BadRequestResult();
        }
    }

    private const string ParamPrefix = "p_";

    private static Dictionary<string, string?> ExtractParametersFromForm(Microsoft.AspNetCore.Http.IFormCollection form)
    {
        Dictionary<string, string?> dict = new(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in form)
        {
            var key = kv.Key ?? string.Empty;
            if (!key.StartsWith(ParamPrefix, StringComparison.Ordinal))
                continue;

            var normalized = key[ParamPrefix.Length..].Trim();
            if (normalized.Length == 0)
                continue;

            dict[normalized] = kv.Value.Count > 0 ? kv.Value[0] : null;
        }

        return dict;
    }

    private void LoadSampleData()
    {
        Columns = new List<GridColumnDefinition>
        {
            new("id", "ID", DataType: "number", Width: "70px"),
            new("name", "İsim"),
            new("email", "Email"),
            new("phone", "Telefon"),
            new("department", "Departman"),
            new("salary", "Maaş", DataType: "number"),
            new("experience", "Tecrübe", DataType: "number"),
            new("city", "Şehir"),
            new("position", "Pozisyon"),
            new("country", "Ülke"),
            new("team", "Takım"),
            new("level", "Seviye"),
            new("manager", "Yönetici"),
            new("office", "Ofis"),
            new("startDate", "Başlangıç"),
            new("status", "Durum")
        };

        var baseData = new[]
        {
            new { id = 1, name = "Ahmet Yılmaz", email = "ahmet@example.com", phone = "0532 123 4567", department = "IT", salary = 15000, experience = 5, city = "Istanbul", position = "Yazılım Geliştirici", startDate = "2019-03-15", status = "Aktif" },
            new { id = 2, name = "Ayşe Demir", email = "ayse@example.com", phone = "0533 234 5678", department = "Satış", salary = 12000, experience = 3, city = "Ankara", position = "Satış Temsilcisi", startDate = "2021-06-20", status = "Aktif" },
            new { id = 3, name = "Mehmet Kaya", email = "mehmet@example.com", phone = "0534 345 6789", department = "İnsan Kaynakları", salary = 18000, experience = 8, city = "Izmir", position = "İK Müdürü", startDate = "2016-01-10", status = "Pasif" },
            new { id = 4, name = "Fatma Çelik", email = "fatma@example.com", phone = "0535 456 7890", department = "Pazarlama", salary = 14000, experience = 4, city = "Bursa", position = "Pazarlama Uzmanı", startDate = "2020-09-05", status = "Beklemede" },
            new { id = 5, name = "Ali Öz", email = "ali@example.com", phone = "0536 567 8901", department = "IT", salary = 16500, experience = 6, city = "Istanbul", position = "Sistem Yöneticisi", startDate = "2018-11-12", status = "Aktif" },
            new { id = 6, name = "Zeynep Şahin", email = "zeynep@example.com", phone = "0537 678 9012", department = "Satış", salary = 11000, experience = 2, city = "Antalya", position = "Satış Danışmanı", startDate = "2022-02-28", status = "Aktif" },
            new { id = 7, name = "Can Arslan", email = "can@example.com", phone = "0538 789 0123", department = "IT", salary = 17000, experience = 7, city = "Ankara", position = "Kıdemli Geliştirici", startDate = "2017-05-18", status = "Pasif" },
            new { id = 8, name = "Elif Kurt", email = "elif@example.com", phone = "0539 890 1234", department = "Pazarlama", salary = 13500, experience = 4, city = "Istanbul", position = "Dijital Pazarlama", startDate = "2020-07-22", status = "Aktif" },
            new { id = 9, name = "Burak Aydın", email = "burak@example.com", phone = "0540 901 2345", department = "İnsan Kaynakları", salary = 12500, experience = 3, city = "Izmir", position = "İK Uzmanı", startDate = "2021-04-14", status = "Beklemede" },
            new { id = 10, name = "Selin Türk", email = "selin@example.com", phone = "0541 012 3456", department = "Satış", salary = 13000, experience = 5, city = "Bursa", position = "Bölge Müdürü", startDate = "2019-08-30", status = "Aktif" },
            new { id = 11, name = "Deniz Yıldız", email = "deniz@example.com", phone = "0542 111 2222", department = "IT", salary = 19000, experience = 10, city = "Istanbul", position = "IT Müdürü", startDate = "2014-12-01", status = "Aktif" },
            new { id = 12, name = "Ece Kartal", email = "ece@example.com", phone = "0543 222 3333", department = "Pazarlama", salary = 12000, experience = 2, city = "Ankara", position = "Sosyal Medya Uzmanı", startDate = "2022-05-17", status = "Pasif" },
            new { id = 13, name = "Emre Koç", email = "emre@example.com", phone = "0544 333 4444", department = "IT", salary = 15500, experience = 5, city = "Izmir", position = "Veri Analisti", startDate = "2019-10-08", status = "Aktif" },
            new { id = 14, name = "Gizem Acar", email = "gizem@example.com", phone = "0545 444 5555", department = "Satış", salary = 11500, experience = 3, city = "Antalya", position = "Satış Uzmanı", startDate = "2021-03-25", status = "Aktif" },
            new { id = 15, name = "Hakan Demir", email = "hakan@example.com", phone = "0546 555 6666", department = "Pazarlama", salary = 16000, experience = 7, city = "Istanbul", position = "Marka Müdürü", startDate = "2017-07-19", status = "Aktif" },
            new { id = 16, name = "İrem Yalçın", email = "irem@example.com", phone = "0547 666 7777", department = "İnsan Kaynakları", salary = 14500, experience = 6, city = "Bursa", position = "İK Koordinatörü", startDate = "2018-09-11", status = "Aktif" },
            new { id = 17, name = "Kerem Özkan", email = "kerem@example.com", phone = "0548 777 8888", department = "IT", salary = 18500, experience = 9, city = "Ankara", position = "Yazılım Mimarı", startDate = "2015-02-03", status = "Pasif" },
            new { id = 18, name = "Lale Kara", email = "lale@example.com", phone = "0549 888 9999", department = "Satış", salary = 13500, experience = 4, city = "Istanbul", position = "Kurumsal Satış", startDate = "2020-11-27", status = "Aktif" },
            new { id = 19, name = "Mert Çetin", email = "mert@example.com", phone = "0530 999 0000", department = "Pazarlama", salary = 12500, experience = 3, city = "Izmir", position = "İçerik Üreticisi", startDate = "2021-08-16", status = "Beklemede" },
            new { id = 20, name = "Nil Şen", email = "nil@example.com", phone = "0531 000 1111", department = "IT", salary = 17500, experience = 8, city = "Ankara", position = "DevOps Mühendisi", startDate = "2016-06-22", status = "Aktif" },
            new { id = 21, name = "Oğuz Taş", email = "oguz@example.com", phone = "0532 111 2222", department = "Satış", salary = 14000, experience = 5, city = "Bursa", position = "Satış Müdürü", startDate = "2019-04-09", status = "Aktif" },
            new { id = 22, name = "Pelin Ay", email = "pelin@example.com", phone = "0533 222 3333", department = "İnsan Kaynakları", salary = 13000, experience = 4, city = "Istanbul", position = "İşe Alım Uzmanı", startDate = "2020-12-14", status = "Aktif" },
            new { id = 23, name = "Rıza Ulu", email = "riza@example.com", phone = "0534 333 4444", department = "Pazarlama", salary = 15500, experience = 6, city = "Antalya", position = "Pazarlama Müdürü", startDate = "2018-03-07", status = "Pasif" },
            new { id = 24, name = "Seda Akın", email = "seda@example.com", phone = "0535 444 5555", department = "IT", salary = 16000, experience = 6, city = "Izmir", position = "Test Uzmanı", startDate = "2018-08-21", status = "Aktif" },
            new { id = 25, name = "Tolga Yurt", email = "tolga@example.com", phone = "0536 555 6666", department = "Satış", salary = 12000, experience = 2, city = "Ankara", position = "Satış Elemanı", startDate = "2022-01-11", status = "Aktif" },
            new { id = 26, name = "Ufuk Aydın", email = "ufuk@example.com", phone = "0537 666 7777", department = "Pazarlama", salary = 14500, experience = 5, city = "Istanbul", position = "SEO Uzmanı", startDate = "2019-09-26", status = "Aktif" },
            new { id = 27, name = "Vildan Er", email = "vildan@example.com", phone = "0538 777 8888", department = "İnsan Kaynakları", salary = 11500, experience = 2, city = "Bursa", position = "İK Asistanı", startDate = "2022-07-04", status = "Beklemede" },
            new { id = 28, name = "Yağmur Kılıç", email = "yagmur@example.com", phone = "0539 888 9999", department = "IT", salary = 20000, experience = 12, city = "Istanbul", position = "CTO", startDate = "2012-10-15", status = "Aktif" },
            new { id = 29, name = "Zafer Güneş", email = "zafer@example.com", phone = "0540 999 0000", department = "Satış", salary = 15000, experience = 7, city = "Ankara", position = "Satış Direktörü", startDate = "2017-11-30", status = "Aktif" },
            new { id = 30, name = "Aslı Öztürk", email = "asli@example.com", phone = "0541 000 1111", department = "Pazarlama", salary = 13000, experience = 4, city = "Izmir", position = "Reklam Uzmanı", startDate = "2020-05-13", status = "Pasif" },
            new { id = 31, name = "Baran Çakır", email = "baran@example.com", phone = "0542 111 2222", department = "IT", salary = 17000, experience = 7, city = "Bursa", position = "Güvenlik Uzmanı", startDate = "2017-02-28", status = "Aktif" },
            new { id = 32, name = "Canan Tekin", email = "canan@example.com", phone = "0543 222 3333", department = "İnsan Kaynakları", salary = 16500, experience = 8, city = "Istanbul", position = "İK Direktörü", startDate = "2016-08-08", status = "Aktif" },
            new { id = 33, name = "Deniz Arslan", email = "deniz2@example.com", phone = "0544 333 4444", department = "Satış", salary = 11000, experience = 1, city = "Antalya", position = "Stajyer", startDate = "2023-03-01", status = "Aktif" },
            new { id = 34, name = "Eda Polat", email = "eda@example.com", phone = "0545 444 5555", department = "Pazarlama", salary = 12500, experience = 3, city = "Ankara", position = "Grafik Tasarımcı", startDate = "2021-09-20", status = "Beklemede" },
            new { id = 35, name = "Fikret Yıldırım", email = "fikret@example.com", phone = "0546 555 6666", department = "IT", salary = 16500, experience = 6, city = "Izmir", position = "Network Uzmanı", startDate = "2018-04-17", status = "Aktif" },
            new { id = 36, name = "Gül Taşkın", email = "gul@example.com", phone = "0547 666 7777", department = "Satış", salary = 14500, experience = 6, city = "Istanbul", position = "İhracat Müdürü", startDate = "2018-10-23", status = "Aktif" },
            new { id = 37, name = "Halil Kurt", email = "halil@example.com", phone = "0548 777 8888", department = "Pazarlama", salary = 18000, experience = 9, city = "Bursa", position = "CMO", startDate = "2015-12-05", status = "Aktif" },
            new { id = 38, name = "İpek Çalışkan", email = "ipek@example.com", phone = "0549 888 9999", department = "İnsan Kaynakları", salary = 12000, experience = 3, city = "Ankara", position = "Eğitim Uzmanı", startDate = "2021-07-14", status = "Pasif" },
            new { id = 39, name = "Kaan Durmuş", email = "kaan@example.com", phone = "0530 999 0000", department = "IT", salary = 15000, experience = 5, city = "Izmir", position = "Mobil Geliştirici", startDate = "2019-05-29", status = "Aktif" },
            new { id = 40, name = "Leyla Berk", email = "leyla@example.com", phone = "0531 000 1111", department = "Satış", salary = 13500, experience = 4, city = "Istanbul", position = "E-ticaret Uzmanı", startDate = "2020-08-11", status = "Aktif" },
            new { id = 41, name = "Mustafa Eren", email = "mustafa@example.com", phone = "0532 111 2222", department = "Pazarlama", salary = 14000, experience = 5, city = "Antalya", position = "Etkinlik Yöneticisi", startDate = "2019-06-18", status = "Beklemede" },
            new { id = 42, name = "Nalan Kaya", email = "nalan@example.com", phone = "0533 222 3333", department = "IT", salary = 19500, experience = 11, city = "Ankara", position = "Proje Müdürü", startDate = "2013-09-10", status = "Aktif" },
            new { id = 43, name = "Onur Aydın", email = "onur@example.com", phone = "0534 333 4444", department = "Satış", salary = 12500, experience = 3, city = "Bursa", position = "Müşteri İlişkileri", startDate = "2021-11-22", status = "Aktif" },
            new { id = 44, name = "Pınar Yüksel", email = "pinar@example.com", phone = "0535 444 5555", department = "İnsan Kaynakları", salary = 13500, experience = 4, city = "Istanbul", position = "Performans Uzmanı", startDate = "2020-10-06", status = "Aktif" },
            new { id = 45, name = "Recep Şimşek", email = "recep@example.com", phone = "0536 555 6666", department = "Pazarlama", salary = 11500, experience = 2, city = "Izmir", position = "Video Editörü", startDate = "2022-04-19", status = "Pasif" },
            new { id = 46, name = "Selin Özer", email = "selin2@example.com", phone = "0537 666 7777", department = "IT", salary = 16000, experience = 6, city = "Ankara", position = "Veri Bilimci", startDate = "2018-07-12", status = "Aktif" },
            new { id = 47, name = "Taner Aslan", email = "taner@example.com", phone = "0538 777 8888", department = "Satış", salary = 17000, experience = 9, city = "Istanbul", position = "Anahtar Müşteri", startDate = "2015-05-25", status = "Aktif" },
            new { id = 48, name = "Ümit Karaca", email = "umit@example.com", phone = "0539 888 9999", department = "Pazarlama", salary = 13000, experience = 3, city = "Bursa", position = "PR Uzmanı", startDate = "2021-12-08", status = "Aktif" },
            new { id = 49, name = "Volkan Özdemir", email = "volkan@example.com", phone = "0540 999 0000", department = "İnsan Kaynakları", salary = 19000, experience = 10, city = "Antalya", position = "İK Genel Müdürü", startDate = "2014-03-20", status = "Aktif" },
            new { id = 50, name = "Yasemin Tan", email = "yasemin@example.com", phone = "0541 000 1111", department = "IT", salary = 18000, experience = 8, city = "Izmir", position = "UX/UI Tasarımcı", startDate = "2016-11-14", status = "Beklemede" },
            new { id = 51, name = "Zeki Bulut", email = "zeki@example.com", phone = "0542 111 2222", department = "Satış", salary = 14500, experience = 5, city = "Ankara", position = "Teknik Satış", startDate = "2019-02-07", status = "Aktif" },
            new { id = 52, name = "Aylin Erdoğan", email = "aylin@example.com", phone = "0543 222 3333", department = "Pazarlama", salary = 15000, experience = 6, city = "Istanbul", position = "Marka Stratejisti", startDate = "2018-06-30", status = "Aktif" },
            new { id = 53, name = "Barış Yaman", email = "baris@example.com", phone = "0544 333 4444", department = "IT", salary = 21000, experience = 13, city = "Bursa", position = "Başkan Yardımcısı", startDate = "2011-08-16", status = "Aktif" },
            new { id = 54, name = "Ceyda Koçak", email = "ceyda@example.com", phone = "0545 444 5555", department = "Satış", salary = 13000, experience = 4, city = "Antalya", position = "Bayi Yöneticisi", startDate = "2020-03-24", status = "Pasif" },
            new { id = 55, name = "Doğan Şener", email = "dogan@example.com", phone = "0546 555 6666", department = "İnsan Kaynakları", salary = 15500, experience = 7, city = "Izmir", position = "Bordro Uzmanı", startDate = "2017-09-02", status = "Aktif" }
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
                ["country"] = "Türkiye",
                ["team"] = $"Takım {(i % 5) + 1}",
                ["level"] = levels[i % levels.Length],
                ["manager"] = $"Yönetici {(i % 8) + 1}",
                ["office"] = $"Ofis {(i % 4) + 1}",
                ["startDate"] = d.startDate,
                ["status"] = d.status
            })
            .ToList();
    }
}
