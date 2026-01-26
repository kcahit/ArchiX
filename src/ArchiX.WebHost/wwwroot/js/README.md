# ArchiX Reports JavaScript Modules

Bu klasörde rapor template'lerinde kullanýlan ortak JavaScript modülleri bulunur.

## Modüller

### 1. archix.reports.header.js
Grid ve Pivot rapor sayfalarý için ortak header component'i.

**Özellikler:**
- Sayfa baþlýðý ve ikonu
- Kayýt sayýsý badge'i
- Genel arama input'u
- Sýfýrla butonu
- Geliþmiþ arama/filtreler toggle butonu
- Export dropdown (Excel, PDF, CSV, TXT, JSON)

**Kullaným:**
```javascript
ArchiXReportsHeader.init({
    title: 'Pivot Rapor',
    icon: 'bi-grid-3x3-gap-fill',
    showExport: true,
    showAdvancedSearch: true,
    exportFormats: ['excel', 'pdf', 'csv'],
    onExport: function(format) {
        // Export iþlemi
    },
    onReset: function() {
        // Reset iþlemi
    }
});

// Kayýt sayýsýný güncelle
ArchiXReportsHeader.updateRecordCount(55);
```

**HTML Gereksinimi:**
```html
<div id="reportHeader">
    <!-- Header buraya render edilir -->
</div>
```

### 2. archix.reports.slicer.core.js
Power BI benzeri slicer/filtre motoru. Tüm rapor template'lerinde kullanýlýr.

**Özellikler:**
- Genel arama filtresi
- Tablo kolon filtreleri
- Metin/sayý filtreleri
- Power BI tarzý slicer'lar
- Dinamik filtre güncellemesi
- Filtrelerin birbirleriyle uyumlu çalýþmasý

**Kullaným:**
```javascript
ArchiXReportsSlicer.init(data, fieldNames, function(filteredData) {
    // Filtre deðiþtiðinde çaðrýlýr
    console.log('Filtered data count:', filteredData.length);
    refreshTable();
});

// Global functions
function clearAdvancedFilters() { ArchiXReportsSlicer.clearAdvancedFilters(); }
function toggleAllColumns() { ArchiXReportsSlicer.toggleAllColumns(); }
```

**HTML Gereksinimleri:**
```html
<!-- Genel arama -->
<input type="text" id="searchInput" placeholder="Genel arama...">

<!-- Kolon checkbox listesi -->
<div id="columnCheckList"></div>

<!-- Slicer container -->
<div id="slicerContainer"></div>

<!-- Toggle all checkbox -->
<input type="checkbox" id="toggleAllColumns" onchange="toggleAllColumns()">
```

## Kullaným Örnekleri

### Grid Template'de Kullaným
```html
<script src="~/js/archix.reports.header.js"></script>
<script src="~/js/archix.reports.slicer.core.js"></script>

<script>
    ArchiXReportsHeader.init({
        title: 'Raporlar',
        icon: 'bi-table',
        showExport: true,
        showAdvancedSearch: true
    });

    ArchiXReportsSlicer.init(data, fieldNames, function(filteredData) {
        ArchiXReportsHeader.updateRecordCount(filteredData.length);
        renderTable(filteredData);
    });
</script>
```

### Pivot Template'de Kullaným
```html
<script src="~/js/archix.reports.header.js"></script>
<script src="~/js/archix.reports.slicer.core.js"></script>

<script>
    ArchiXReportsHeader.init({
        title: 'Pivot Rapor',
        icon: 'bi-grid-3x3-gap-fill',
        showExport: false,
        showAdvancedSearch: true
    });

    ArchiXReportsSlicer.init(data, fieldNames, function(filteredData) {
        ArchiXReportsHeader.updateRecordCount(filteredData.length);
        refreshPivot();
    });
</script>
```

## Notlar

- Her iki modül de `window` object'ine global olarak eklenir
- Bootstrap 5 ve Bootstrap Icons gerektirir
- jQuery gerekmez (slicer core vanilla JS ile çalýþýr)
- Modüller birbirinden baðýmsýz çalýþabilir
