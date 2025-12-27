// ArchiX Grid Report - Main Script
// Örnek veri (gerçek uygulamada API'den gelecek)
const data = [
    { id: 1, name: "Ahmet Yýlmaz", email: "ahmet@example.com", phone: "0532 123 4567", department: "IT", salary: 15000, experience: 5, city: "Istanbul", position: "Yazýlým Geliþtirici", startDate: "2019-03-15", status: "Aktif" },
    { id: 2, name: "Ayþe Demir", email: "ayse@example.com", phone: "0533 234 5678", department: "Satýþ", salary: 12000, experience: 3, city: "Ankara", position: "Satýþ Temsilcisi", startDate: "2021-06-20", status: "Aktif" },
    { id: 3, name: "Mehmet Kaya", email: "mehmet@example.com", phone: "0534 345 6789", department: "Ýnsan Kaynaklarý", salary: 18000, experience: 8, city: "Izmir", position: "ÝK Müdürü", startDate: "2016-01-10", status: "Pasif" },
    { id: 4, name: "Fatma Çelik", email: "fatma@example.com", phone: "0535 456 7890", department: "Pazarlama", salary: 14000, experience: 4, city: "Bursa", position: "Pazarlama Uzmaný", startDate: "2020-09-05", status: "Beklemede" },
    { id: 5, name: "Ali Öz", email: "ali@example.com", phone: "0536 567 8901", department: "IT", salary: 16500, experience: 6, city: "Istanbul", position: "Sistem Yöneticisi", startDate: "2018-11-12", status: "Aktif" },
    // ... diðer 50 kayýt (GridListe.cshtml'den kopyala)
];

let filteredData = [...data];
let currentPage = 1;
let itemsPerPage = 10;
let sortColumn = '';
let sortAscending = true;
let columnFilters = {};
let textFilters = {};
let currentOpenFilter = null;
let currentFilterMode = {};
let slicerSelections = {};
let activeSlicerColumns = [];

const fieldNames = {
    id: "ID",
    name: "Ýsim",
    email: "Email",
    phone: "Telefon",
    department: "Departman",
    salary: "Maaþ",
    experience: "Tecrübe",
    city: "Þehir",
    position: "Pozisyon",
    startDate: "Baþlangýç",
    status: "Durum"
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initSlicers();
    renderTable();
    
    // Search event
    document.getElementById('searchInput').addEventListener('input', function() {
        applyAllFilters();
        if (activeSlicerColumns.length > 0) {
            updateAllSlicers();
        }
    });
    
    // Click outside to close filters
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.filter-dropdown') && !e.target.closest('.filter-icon')) {
            document.querySelectorAll('.filter-dropdown').forEach(dropdown => {
                dropdown.classList.remove('show');
            });
            currentOpenFilter = null;
        }
    });
});

// Export all functions that need to be accessed from HTML onclick handlers
window.toggleFilter = toggleFilter;
window.sortTable = sortTable;
window.changePage = changePage;
window.changeItemsPerPage = changeItemsPerPage;
window.goToPage = goToPage;
window.resetAllFilters = resetAllFilters;
window.clearAdvancedFilters = clearAdvancedFilters;
window.toggleAllColumns = toggleAllColumns;
window.toggleSlicerColumn = toggleSlicerColumn;
window.switchFilterMode = switchFilterMode;
window.handleOperatorChange = handleOperatorChange;
window.filterDropdownOptions = filterDropdownOptions;
window.selectAllFilter = selectAllFilter;
window.toggleFilterValue = toggleFilterValue;
window.applyFilter = applyFilter;
window.clearFilter = clearFilter;
window.removeIndividualFilter = removeIndividualFilter;
window.toggleAllFilterAccordions = toggleAllFilterAccordions;
window.removeColumnFilter = removeColumnFilter;
window.viewItem = viewItem;
window.editItem = editItem;
window.deleteItem = deleteItem;
window.addNew = addNew;
window.exportData = exportData;
window.toggleSlicerValue = toggleSlicerValue;

// [TÜM FONKSÝYONLARI BURAYA KOPYALA - GridListe.cshtml'den]
// toggleFilter, buildFilterDropdown, sortTable, renderTable, vs...
