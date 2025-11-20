1. Mimari Genel Çerçeve
* Hibrit model (Osman çekirdeði + otomasyon/entegrasyon)
* Katmanlar: Common, Domain, Infrastructure, Application, Web (UI/API)
* Modüler kütüphane yapýsý
* Çok proje/solution uyumu
2. Common Katmaný
* Result / Error modeli (ErrorCode, CorrelationId, TraceId)
* Paging, Filtering, Sorting soyutlamalarý
* Response/Request standartlarý
* Ortak yardýmcýlar (Guard, Clock/TimeProvider, Slug/Id helpers)
3. Domain Katmaný
* BaseEntity (Id, CreatedAt/By, UpdatedAt/By, Status)
* Soft-delete ve durum yönetimi
* Domain event/notification altyapýsý
4. Infrastructure Katmaný
* AppDbContext konfigürasyonu
* Repository + UnitOfWork
* Cache arabirimleri
* Loglama entegrasyonu
* Dýþ servis adapter þablonlarý
5. Application (CQRS)
* Komut/Sorgu handler yapýsý
* Validation pipeline
* Mapping profilleri
* Transaction davranýþlarý
* Yetkilendirme davranýþlarý
6. Hata Yönetimi – Hibrit Osman Çekirdeði
* Global Exception Middleware + Action Filter
* Hata sýnýflarý: Validation / Business / Security / System / External
* Çift zaman damgasý
* Yetkiye göre detay gösterimi
* Log korrelasyonu
* Hata tablosu ve izleme ekranlarý
7. Güvenlik
* JWT/Token tabanlý kimlik doðrulama
* Güvenli login (2FA opsiyonu)
* Rol/claim bazlý yetkilendirme
* Anti-forgery, rate limiting
* Veri maskeleme
8. Performans
* Server-side paging/filtering/sorting
* EF optimizasyonlarý
* Cache stratejileri
* Ýyileþtirme metrikleri
9. UI/GRID – Excel Benzeri Liste
* Kolon baþlýklarýnda sýralama
* Filtreler (baþlar, biter, arasýnda vb.)
* Çoklu seçim filtreleri
* Export: Excel, PDF, CSV, TXT
* Import: Excel, CSV
* Inline edit
10. Ekran Tasarýmlarý
* Layout
* Partial Views
* Kullanýcý tema tercihleri
* Login, admin, form, list, master-detail ekran tasarýmlarý
* Inline ekleme/silme/güncelleme
* Toplamlar/ara toplamlar, satýr bazlý kümülatif toplamlar
* Yetki/iþ kuralý kontrolü
11. Dosya/Veri Aktarýmlarý
* Export/Import servisleri
* Þablon bazlý Excel üretimi
* Import doðrulama raporu
12. Raporlama Tool
* List, Pie, Line, Column, Pivot Table
* Pivot konfigürasyonu
* Filtreleme standardý (proje genelinde)
* Import/Export modüler yapýsý
* Yetki ve güvenlik
* Þablon desteði
13. Fonksiyonlar
* Satýr bazlý (Row-Level)
* Grup toplamlarý (Aggregate)
* Pencere (Window/Analitik)
* Zaman & tarih fonksiyonlarý
* Finans/vergisel fonksiyonlar
* Ýstatistik fonksiyonlar
* Haritalama & dönüþüm
* Maskeleme
14. Test ve Kalite
* Unit test
* Integration test
* Kod kalite analizleri
15. CI/CD
* Build/test/coverage pipeline
* Migration/seed otomasyonu
* Log/metrics/tracing entegrasyonu
16. ArchiX / Otomasyon
* Katman iskeletleri
* Controller/ViewModel/View/Partial üretimi
* Repo/UoW üretimi
* Mapping/Validation üretimi
* Program.cs ekleme þablonu
