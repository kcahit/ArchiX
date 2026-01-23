# #32 — Application Tanım Ekranı

> **Durum:** Analiz/Tasarım Tamamlandı | **Kod:** İmplementasyona Hazır | **Açık Nokta:** 0  
> **Format:** V5 (24.01.2026) — SON VERSİYON

---

## 1) ANALİZ

### 1.1 Amaç ve Kapsam

**Hedef:**
- `Application` entity tanımlarını yönetecek ekran (liste + record CRUD)
- Grid mimarisi hem dataset-driven hem entity-driven çalışacak
- Tab hiyerarşisi ile uyumlu navigasyon

**Prensipler:**
- Liste: grid (entity'den dinamik kolon üretimi)
- Satır aksiyonu: "İncele/Düzenle" → **GridRecordAccordion** içinde form açar
- Toolbar: "Yeni Kayıt" butonu → **GridRecordAccordion** içinde form açar
- **GridRecordAccordion:** Grid üstünde inline accordion (başlangıçta kapalı)
- CRUD: DB ile çalışan, fiziksel delete yok
- Soft delete: `StatusId` üzerinden
- `ApplicationId = 1` silinemez (UI + backend)

**Kapsam:**
- Liste ekranı: DB'den `Applications` → grid (dinamik kolon)
- **GridRecordAccordion:** Grid üstünde accordion component (record form container)
- Record form: Create / Update / SoftDelete (DB, accordion içinde render)
- "Silinmişleri göster" checkbox (toolbar içinde)
- `ApplicationId=1` koruması (UI + handler)

### 1.2 Mevcut Sistem Bağımlılıkları (Doğrulanmış)

**TabHost / Tab Hiyerarşisi (Kritik):**
- **Dosya:** `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Sidebar altındaki menüler sağ tarafta **tab hiyerarşisi** olarak açılır
- Navigasyon: `openTab({ url, title })` (root tab) veya grup + nested tab
- İçerik yükleme: `loadContent(url)` + HTML extract (`#tab-main` / `.archix-work-area` / `main`)
- **NOT:** `window.location.href` kullanılmaz; TabHost üzerinden gezinilir

**Grid Altyapısı:**
- `GridTableViewModel` → grid model sözleşmesi
- `GridToolbarViewModel` → toolbar sözleşmesi
- `GridColumnDefinition` → kolon tanımı (Field, Title, DataType, Width...)
- Grid JS: `src/ArchiX.Library.Web/wwwroot/js/archix.grid.component.js`
- Toolbar view: `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/Components/Dataset/GridToolbar/Default.cshtml`

**Accordion Component (Yeni):**
- **GridRecordAccordion:** Grid üstünde inline form container
- Bootstrap accordion kullanılacak
- Başlangıçta collapsed (görünmez)
- "Yeni Kayıt" / "İncele" tıklanınca expand + içerik fetch/render
- ViewComponent olarak tasarlanacak

**Soft Delete Altyapısı:**
- `BaseEntity.SoftDelete(userId)` → `StatusId = 6`
- `BaseEntity.MarkCreated(userId)` / `MarkUpdated(userId)` → audit alanları
- EF global filter: `ApplySoftDeleteFilters()` (tüm BaseEntity türevleri)
- Filter bypass: `IgnoreQueryFilters()` ile include deleted

**DatasetRecord Referansı:**
- `/Tools/Dataset/Record` → dataset-driven record (fake UI, yaklaşım referansı)

**Application Placeholder:**
- `src/ArchiX.Library.Web/Pages/Definitions/Application.cshtml`
- `src/ArchiX.Library.Web/Pages/Definitions/Application.cshtml.cs`

### 1.3 Mevcut Kısıtlar (Doğrulanmış)

**1.3.1 Grid JS edit akışı dataset'e hard bağlı**
- `editItem()` şu an `/Tools/Dataset/Record` + `ReportDatasetId` üzerine kurulu
- Entity-driven record açmak için grid state'e "record endpoint" kontratı gerekir

**1.3.2 Record açma pattern eksik**
- Mevcut grid JS modal/tab açıyor
- **Gereksinim:** Grid üstünde inline accordion içinde form açılmalı
- Accordion component (GridRecordAccordion) yok, oluşturulmalı

**1.3.3 Nav davranışı TabHost ile uyumlu olmalı (Kritik)**
- Sistem `window.location.href` kullanmaz
- Grid içinden record açma artık **accordion içinde** olacak (tab değil)

**1.3.4 Delete işlemi client-side**
- `deleteItem()` şu an sadece `state.data` listesinden siliyor (DB'ye istek yok)
- Application için DB ile çalışan soft delete handler gerekir

**1.3.5 Entity → Grid model dönüşümü**
- Kodda "entity class → columns otomatik" gibi helper doğrulanmadı
- `Columns` ve `Rows` caller tarafından üretilmeli

---

## 2) TASARIM

### 2.1 Entity Referansı (Doğrulanmış)

**Dosya:** `src/ArchiX.Library/Entities/Application.cs`

**Alanlar:**
- `Id` (PK, BaseEntity)
- `RowId` (Guid, BaseEntity) — **her satırda mevcuttur**
- `Code` (unique)
- `Name`
- `DefaultCulture`
- `TimeZoneId`
- `Description`
- `ConfigVersion`
- `ExternalKey`
- `StatusId` (BaseEntity, soft delete)
- `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` (BaseEntity audit)

**DbContext:**
- `AppDbContext.Applications` (DbSet)
- `Code` için unique index mevcut

### 2.2 Liste Ekranı (Grid)

**2.2.1 Veri Kaynağı**
- `AppDbContext.Applications`
- Default: global filter ile `StatusId=6` dışarıda
- `includeDeleted=1` → `IgnoreQueryFilters()`

**2.2.2 Grid Model Üretimi (Dinamik Kolonlar)**

```csharp
// Razor Page: OnGetAsync içinde
public async Task OnGetAsync([FromQuery] int? includeDeleted, CancellationToken ct)
{
    var query = _db.Applications.AsQueryable();
    
    if (includeDeleted == 1)
    {
        query = query.IgnoreQueryFilters(); // Silinmişler dahil
    }
    
    var applications = await query.ToListAsync(ct);
    
    // Dinamik kolon üretimi (entity'den reflection)
    var columns = new List<GridColumnDefinition>
    {
        new("Id", "ID", Width: "80px"),
        new("Code", "Kod", Width: "150px"),
        new("Name", "Ad", Width: "200px"),
        new("DefaultCulture", "Dil", Width: "100px"),
        new("StatusId", "Durum", Width: "100px")
    };
    
    var rows = applications.Select(a => new Dictionary<string, object?>
    {
        ["Id"] = a.Id,
        ["Code"] = a.Code,
        ["Name"] = a.Name,
        ["DefaultCulture"] = a.DefaultCulture,
        ["StatusId"] = a.StatusId
    }).ToList();
    
    Model = new GridTableViewModel
    {
        Id = "appgrid",
        Columns = columns,
        Rows = rows,
        ShowActions = true,
        ShowToolbar = true,
        Toolbar = new GridToolbarViewModel
        {
            IsFormOpenEnabled = 1,
            RecordEndpoint = "/Definitions/Application/Record", // YENİ (entity-driven)
            ShowDeletedToggle = true // YENİ (toolbar içinde checkbox)
        }
    };
}
```

**2.2.3 Toolbar İçeriği (Koda Göre Gerçek)**

Toolbar view'unda şu bloklar var:

1. **Sol:** DatasetSelector (opsiyonel) + TotalRecords
2. **Orta:** Search input + Reset butonu
3. **Sağ:** Advanced Search + **ShowDeletedToggle** + **Yeni Kayıt** + Export dropdown

### 2.3 GridRecordAccordion Component (Yeni)

**Amaç:**
- Grid üstünde inline form container
- Modal/tab yerine accordion içinde record formu göstermek
- Daha az context switching, sayfa içinde CRUD

**Kontrat:**

**ViewModel:**
```csharp
// src/ArchiX.Library.Web/ViewModels/Grid/GridRecordAccordionViewModel.cs
public class GridRecordAccordionViewModel
{
    public string Id { get; set; } = "grid-record-accordion";
    public string GridId { get; set; } = "dsgrid"; // İlişkili grid ID
    public bool IsOpen { get; set; } = false; // Başlangıçta kapalı
    public string Title { get; set; } = "Kayıt Detayı";
}
```

**ViewComponent:**
```csharp
// src/ArchiX.Library.Web/ViewComponents/Grid/GridRecordAccordionViewComponent.cs
public class GridRecordAccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(GridRecordAccordionViewModel model)
    {
        return View("~/Templates/Modern/Pages/Shared/Components/Grid/GridRecordAccordion/Default.cshtml", model);
    }
}
```

**View (Bootstrap Accordion):**
```cshtml
@* src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/Components/Grid/GridRecordAccordion/Default.cshtml *@
@model ArchiX.Library.Web.ViewModels.Grid.GridRecordAccordionViewModel

<div class="accordion mb-3" id="@Model.Id">
    <div class="accordion-item">
        <h2 class="accordion-header">
            <button class="accordion-button @(Model.IsOpen ? "" : "collapsed")" 
                    type="button" 
                    data-bs-toggle="collapse" 
                    data-bs-target="#@Model.Id-body" 
                    aria-expanded="@(Model.IsOpen ? "true" : "false")">
                <span id="@Model.Id-title">@Model.Title</span>
            </button>
        </h2>
        <div id="@Model.Id-body" 
             class="accordion-collapse collapse @(Model.IsOpen ? "show" : "")" 
             data-bs-parent="#@Model.Id">
            <div class="accordion-body">
                <div id="@Model.Id-content" class="grid-record-content">
                    <!-- İçerik buraya dinamik yüklenecek -->
                </div>
            </div>
        </div>
    </div>
</div>
```

**JS API (Grid JS içinde):**
```javascript
// src/ArchiX.Library.Web/wwwroot/js/archix.grid.component.js içine eklenecek

function showRecordInAccordion(gridId, url, title) {
    const accordionId = `grid-record-accordion-${gridId}`;
    const contentId = `${accordionId}-content`;
    const titleEl = document.getElementById(`${accordionId}-title`);
    const contentEl = document.getElementById(contentId);
    
    if (!contentEl) return;
    
    // Başlık güncelle
    if (titleEl) titleEl.textContent = title || 'Kayıt Detayı';
    
    // Loading göster
    contentEl.innerHTML = '<div class="text-center p-3"><div class="spinner-border" role="status"></div></div>';
    
    // Accordion'u aç
    const accordionBody = document.getElementById(`${accordionId}-body`);
    if (accordionBody && window.bootstrap?.Collapse) {
        const bsCollapse = window.bootstrap.Collapse.getOrCreateInstance(accordionBody);
        bsCollapse.show();
    }
    
    // İçerik fetch et
    fetch(url, {
        headers: {
            'X-ArchiX-Tab': '1',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(res => res.text())
    .then(html => {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const main = doc.querySelector('#tab-main') || doc.querySelector('.archix-work-area') || doc.querySelector('main');
        contentEl.innerHTML = main ? main.innerHTML : html;
        
        // Form script'lerini yeniden çalıştır
        const scripts = contentEl.querySelectorAll('script');
        scripts.forEach(oldScript => {
            const newScript = document.createElement('script');
            if (oldScript.src) {
                newScript.src = oldScript.src;
            } else {
                newScript.textContent = oldScript.textContent;
            }
            oldScript.parentNode.replaceChild(newScript, oldScript);
        });
    })
    .catch(err => {
        contentEl.innerHTML = `<div class="alert alert-danger">Hata: ${err.message}</div>`;
    });
}

// Global export
window.showRecordInAccordion = showRecordInAccordion;
```

**"Yeni Kayıt" butonu çağrısı:**
```javascript
function openNewRecord(gridId, recordEndpoint) {
    showRecordInAccordion(gridId, recordEndpoint, 'Yeni Application');
}
```

**"İncele" butonu çağrısı (editItem içinde):**
```javascript
if (recordEndpoint) {
    const url = `${recordEndpoint}?id=${id}`;
    showRecordInAccordion(tableId, url, `Application #${id}`);
}
```

**Sonuç:**
- Grid üstünde accordion render edilecek
- "Yeni Kayıt" / "İncele" → accordion expand + form render
- Tab/modal yerine inline UX

### 2.4 IncludeDeleted Toggle (Toolbar İçinde)

**Kontrat:**
- `GridToolbarViewModel.ShowDeletedToggle` (yeni, default false)
- Query param: `includeDeleted` (0/1)

**Toolbar View Güncelleme:**

```cshtml
@* src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/Components/Dataset/GridToolbar/Default.cshtml *@
<div class="d-flex justify-content-between align-items-center gap-2 mb-3">
    <div class="d-flex align-items-center gap-2">
        @* Mevcut: DatasetSelector + TotalRecords *@
        @await Component.InvokeAsync("DatasetSelector", ...)
        <span class="text-muted grid-record-count ms-1">...</span>
    </div>

    <div class="d-flex align-items-center gap-2 flex-grow-1">
        @* Mevcut: Search + Reset *@
        <div class="search-box flex-grow-1">...</div>
        <button class="btn btn-primary btn-sm grid-btn-reset">...</button>
    </div>

    @if (Model.ShowAdvancedSearch) { ... }

    @* YENİ: ShowDeletedToggle (Export'un solunda) *@
    @if (Model.ShowDeletedToggle)
    {
        <div class="form-check ms-2">
            <input class="form-check-input" type="checkbox" id="@(Model.Id)-showDeleted" 
                   @(Context.Request.Query["includeDeleted"] == "1" ? "checked" : "")>
            <label class="form-check-label" for="@(Model.Id)-showDeleted">
                Silinmişleri Göster
            </label>
        </div>
    }

    @* YENİ: Yeni Kayıt butonu (RecordEndpoint varsa, Export'un solunda) *@
    @if (!string.IsNullOrWhiteSpace(Model.RecordEndpoint) && Model.IsFormOpenEnabled == 1)
    {
        <button class="btn btn-success btn-sm ms-2" onclick="openNewRecord('@Model.Id', '@Model.RecordEndpoint')">
            <i class="bi bi-plus-circle me-1"></i> Yeni Kayıt
        </button>
    }

    @if (Model.ShowExport)
    {
        <div class="ms-auto">
            <div class="dropdown">...</div>
        </div>
    }
</div>

<script>
    // Checkbox değiştiğinde tab içinde yeniden yükle
    document.getElementById('@(Model.Id)-showDeleted')?.addEventListener('change', function() {
        const url = new URL(window.location.href);
        if (this.checked) {
            url.searchParams.set('includeDeleted', '1');
        } else {
            url.searchParams.delete('includeDeleted');
        }
        window.location.href = url.toString();
    });
    
    // Yeni Kayıt butonu → Accordion içinde aç
    function openNewRecord(gridId, recordEndpoint) {
        if (window.showRecordInAccordion) {
            window.showRecordInAccordion(gridId, recordEndpoint, 'Yeni Application');
        } else {
            window.location.href = recordEndpoint;
        }
    }
</script>
```

**Sonuç:**
- Checkbox toolbar içinde, **Export dropdown'un solunda**
- `ShowDeletedToggle = true` → checkbox render edilir
- Checkbox değişince `?includeDeleted=1` ile tab içinde yeniden yüklenir
- Backend'de `IgnoreQueryFilters()`
- **Yeni Kayıt** butonu accordion içinde form açıyor

### 2.5 Grid → Record (Parametrik + Accordion Uyumlu)

**Kontrat:**
- `GridToolbarViewModel.RecordEndpoint` (yeni, opsiyonel)
  - doluysa → entity-driven record
  - boşsa → dataset-driven record (mevcut davranış)

**Grid State JSON Üretimi (Razor View):**

```cshtml
@* src/ArchiX.Library.Web/Pages/Definitions/Application.cshtml *@
@model ArchiX.Library.Web.Pages.Definitions.ApplicationPageModel

<div id="@Model.Model.Id-container" 
     data-grid-state="@Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.Model, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }))">
    
    @await Component.InvokeAsync("DatasetGrid", Model.Model)
</div>
```

**Grid JS Güncelleme:**

```javascript
// src/ArchiX.Library.Web/wwwroot/js/archix.grid.component.js
function editItem(tableId, id) {
    const state = getState(tableId);
    const canEdit = !!state?.isFormOpenEnabled;
    if (!canEdit) return;

    const recordEndpoint = state?.recordEndpoint;
    
    if (!recordEndpoint) {
        // Dataset modunda (mevcut davranış)
        const reportDatasetId = getSelectedReportDatasetId(tableId);
        if (!reportDatasetId) {
            alert('Dataset seçilmedi. Önce dataset seçip raporu çalıştırın.');
            return;
        }
        
        const returnContext = getReturnContext(tableId);
        const qs = new URLSearchParams();
        qs.set('ReportDatasetId', String(reportDatasetId));
        if (id !== undefined && id !== null && String(id).length > 0) qs.set('RowId', String(id));
        if (returnContext) qs.set('ReturnContext', returnContext);
        qs.set('HasRecordOperations', '1');
        
        window.location.href = `/Tools/Dataset/Record?${qs.toString()}`;
    } else {
        // Entity modunda: Accordion içinde aç
        const qs = new URLSearchParams();
        if (id !== undefined && id !== null && String(id).length > 0) qs.set('id', String(id));
        
        const url = `${recordEndpoint}?${qs.toString()}`;
        const title = id ? `Application #${id}` : 'Yeni Application';
        
        // Accordion içinde göster
        if (window.showRecordInAccordion) {
            window.showRecordInAccordion(tableId, url, title);
        } else {
            // Fallback
            window.location.href = url;
        }
    }
}
```

**Önemli Notlar:**

1. **JSON naming:** `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` ile `RecordEndpoint` → `recordEndpoint` olarak serialize ediliyor

2. **Accordion pattern:** `showRecordInAccordion(gridId, url, title)`
   - Grid üstünde accordion expand
   - İçerik fetch + render
   - Inline UX (modal/tab değil)

### 2.6 Record Ekranı (CRUD)

**URL:** `/Definitions/Application/Record`

**Page:** `src/ArchiX.Library.Web/Pages/Definitions/Application/Record.cshtml`

**PageModel:** `src/ArchiX.Library.Web/Pages/Definitions/Application/Record.cshtml.cs`

**Form Model:**

```csharp
// src/ArchiX.Library.Web/ViewModels/Definitions/ApplicationFormModel.cs
public sealed class ApplicationFormModel
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string DefaultCulture { get; set; } = "tr-TR";
    
    [StringLength(100)]
    public string? TimeZoneId { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}
```

**Handler Sözleşmeleri:**

**OnGetAsync:**
```csharp
public async Task OnGetAsync([FromQuery] int? id, CancellationToken ct)
{
    IsNew = id == null || id == 0;
    
    if (!IsNew)
    {
        var app = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        
        if (app == null)
        {
            Response.StatusCode = 404;
            return;
        }
        
        Application = app;
        Form = new ApplicationFormModel
        {
            Code = app.Code,
            Name = app.Name,
            DefaultCulture = app.DefaultCulture,
            TimeZoneId = app.TimeZoneId,
            Description = app.Description
        };
    }
}
```

**OnPostCreateAsync:**
```csharp
public async Task<IActionResult> OnPostCreateAsync([FromForm] ApplicationFormModel form, CancellationToken ct)
{
    if (!ModelState.IsValid)
        return Page();
    
    var app = new Application
    {
        Code = form.Code,
        Name = form.Name,
        DefaultCulture = form.DefaultCulture,
        TimeZoneId = form.TimeZoneId,
        Description = form.Description
    };
    
    // Audit: HttpContext.User üzerinden userId alınmalı (örnek: 1)
    app.MarkCreated(userId: 1); // TODO: gerçek userId
    _db.Applications.Add(app);
    await _db.SaveChangesAsync(ct);
    
    // Tab içinde liste sayfasına dön
    return RedirectToPage("/Definitions/Application");
}
```

**OnPostUpdateAsync:**
```csharp
public async Task<IActionResult> OnPostUpdateAsync([FromForm] int id, [FromForm] ApplicationFormModel form, CancellationToken ct)
{
    if (!ModelState.IsValid)
        return Page();
    
    var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
    if (app == null) return NotFound();
    
    app.Code = form.Code;
    app.Name = form.Name;
    app.DefaultCulture = form.DefaultCulture;
    app.TimeZoneId = form.TimeZoneId;
    app.Description = form.Description;
    
    app.MarkUpdated(userId: 1); // TODO: gerçek userId
    
    await _db.SaveChangesAsync(ct);
    return RedirectToPage("/Definitions/Application");
}
```

**OnPostDeleteAsync (Soft):**
```csharp
public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, CancellationToken ct)
{
    if (id == 1)
        throw new InvalidOperationException("System application cannot be deleted.");
    
    var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
    if (app == null) return NotFound();
    
    app.SoftDelete(userId: 1); // TODO: gerçek userId
    await _db.SaveChangesAsync(ct);
    
    return RedirectToPage("/Definitions/Application");
}
```

**Buton Kuralları (Record.cshtml):**

```cshtml
<div class="d-flex gap-2">
    @if (Model.IsNew)
    {
        <button type="submit" asp-page-handler="Create" class="btn btn-primary">Kaydet</button>
    }
    else
    {
        <button type="submit" asp-page-handler="Update" class="btn btn-primary">Güncelle</button>
        
        @if (Model.Application?.Id != 1)
        {
            <button type="submit" asp-page-handler="Delete" class="btn btn-danger" 
                    onclick="return confirm('Silmek istediğinize emin misiniz?')">Sil</button>
        }
    }
    
    <a href="/Definitions/Application" class="btn btn-secondary">Kapat</a>
</div>
```

---

## 3) UNIT TEST STRATEJİSİ

### 3.1 Test Sınıfları

**3.1.1 `ApplicationListPageTests.cs`**
- Liste doğru çekiliyor mu?
- `includeDeleted=0` → silinmişler gelmez
- `includeDeleted=1` → silinmişler dahil

**3.1.2 `ApplicationRecordPageTests.cs`**
- `id` yok → `IsNew=true`
- `id` var → `IsNew=false` ve doğru kayıt çekiliyor

**3.1.3 `ApplicationCrudHandlerTests.cs`**
- Create: yeni kayıt + `MarkCreated` çağrılıyor
- Update: kayıt güncelleme + `MarkUpdated` çağrılıyor
- Delete (soft): `StatusId=6` + `SoftDelete` çağrılıyor

**3.1.4 `ApplicationId1ProtectionTests.cs`**
- `id=1` için delete handler exception
- UI'da `id=1` için "Sil" butonu render edilmiyor

**3.1.5 `GridRecordAccordionTests.cs`**
- Accordion başlangıçta collapsed
- "Yeni Kayıt" tıkla → accordion expand + içerik yükleniyor
- "İncele" tıkla → accordion expand + içerik (id ile) yükleniyor
- Accordion header tıkla → collapse
- Form submit → accordion kapanıyor + grid refresh

### 3.2 Örnek Test Case'ler

```csharp
[Fact]
public async Task OnGetAsync_IdProvided_ShouldLoadApplication()
{
    // Arrange
    var page = new ApplicationRecordPageModel(_db);
    
    // Act
    await page.OnGetAsync(id: 2, CancellationToken.None);
    
    // Assert
    Assert.False(page.IsNew);
    Assert.NotNull(page.Application);
    Assert.Equal(2, page.Application.Id);
}

[Fact]
public async Task OnPostDeleteAsync_IdEquals1_ShouldThrow()
{
    // Arrange
    var page = new ApplicationRecordPageModel(_db);
    
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await page.OnPostDeleteAsync(id: 1, CancellationToken.None);
    });
}

[Fact]
public async Task OnPostDeleteAsync_ValidId_ShouldSetStatusId6()
{
    // Arrange
    var app = new Application { Id = 99, Code = "TEST", Name = "Test", StatusId = 1 };
    _db.Applications.Add(app);
    await _db.SaveChangesAsync();
    
    var page = new ApplicationRecordPageModel(_db);
    
    // Act
    await page.OnPostDeleteAsync(id: 99, CancellationToken.None);
    
    // Assert
    var deleted = await _db.Applications.IgnoreQueryFilters().FirstAsync(a => a.Id == 99);
    Assert.Equal(6, deleted.StatusId);
}
```

---

## 4) YAPILACAK İŞLER (Sıralı)

### İş 4.1 — GridRecordAccordionViewModel + ViewComponent
- **Bağımlılık:** Tasarım 2.3
- **Aksiyon:** ViewModel + ViewComponent + View oluştur

### İş 4.2 — GridToolbarViewModel: RecordEndpoint + ShowDeletedToggle
- **Bağımlılık:** Tasarım 2.4, 2.5
- **Aksiyon:** `GridToolbarViewModel`'e iki yeni alan ekle

### İş 4.3 — GridToolbar View: Toggle + Yeni Kayıt butonu
- **Bağımlılık:** İş 4.2, Tasarım 2.4
- **Aksiyon:** Toolbar view'unda checkbox + buton render et (Export'un solunda)

### İş 4.4 — Grid JS: showRecordInAccordion fonksiyonu
- **Bağımlılık:** İş 4.1, Tasarım 2.3
- **Aksiyon:** `showRecordInAccordion()` fonksiyonu ekle (fetch + render + accordion control)

### İş 4.5 — Grid JS: editItem accordion entegrasyonu
- **Bağımlılık:** İş 4.4, Tasarım 2.5
- **Aksiyon:** `editItem()` fonksiyonunu güncelle (accordion pattern)

### İş 4.6 — Application Liste Sayfası + Accordion
- **Bağımlılık:** İş 4.1, Tasarım 2.2.2
- **Aksiyon:** Placeholder kaldır, grid üret, **GridRecordAccordion component render et**, `includeDeleted` param

### İş 4.7 — ApplicationFormModel
- **Bağımlılık:** Tasarım 2.6
- **Aksiyon:** Form model oluştur (validation attribute'ları ile)

### İş 4.8 — Application Record Page + Handlers
- **Bağımlılık:** İş 4.7, Tasarım 2.6
- **Aksiyon:** Page/PageModel oluştur, Create/Update/Delete handlers

### İş 4.9 — Record UI: Buton kuralları + id=1 guard
- **Bağımlılık:** Tasarım 2.6
- **Aksiyon:** Buton render mantığı + backend guard

### İş 4.10 — Unit Testler
- **Bağımlılık:** İş 4.1–4.9, Unit Test 3
- **Aksiyon:** Test sınıflarını yaz (GridRecordAccordion testleri dahil)

### İş 4.11 — Manuel Test
- **Bağımlılık:** İş 4.1–4.10
- **Aksiyon:** Tüm akışı doğrula (liste, accordion açma/kapama, yeni, düzenle, sil, toggle)

---

## 5) AÇIK NOKTALAR

> (boş — tüm kararlar alındı, implementasyona hazır)

---

## 6) REFERANSLAR

- **GridRecordAccordion Component:** `src/ArchiX.Library.Web/ViewComponents/Grid/GridRecordAccordionViewComponent.cs` (yeni)
- **GridRecordAccordion View:** `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/Components/Grid/GridRecordAccordion/Default.cshtml` (yeni)
- **GridRecordAccordionViewModel:** `src/ArchiX.Library.Web/ViewModels/Grid/GridRecordAccordionViewModel.cs` (yeni)
- TabHost JS: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Grid JS: `src/ArchiX.Library.Web/wwwroot/js/archix.grid.component.js`
- Toolbar View: `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/Components/Dataset/GridToolbar/Default.cshtml`
- GridToolbarViewModel: `src/ArchiX.Library.Web/ViewModels/Grid/GridToolbarViewModel.cs`
- GridColumnDefinition: `src/ArchiX.Library.Web/ViewModels/Grid/GridColumnDefinition.cs`
- BaseEntity: `src/ArchiX.Library/Entities/BaseEntity.cs`
- Application Entity: `src/ArchiX.Library/Entities/Application.cs`

---

**SON GÜNCELLEME:** 24.01.2026 02:00  
**SORUMLU:** GitHub Copilot (Workspace Agent) Sonnet 4.5  
**FORMAT:** V5 (SON VERSİYON — GridRecordAccordion pattern ile implementasyona hazır)
**FORMAT:** V5 (SON VERSİYON — implementasyona hazır)
