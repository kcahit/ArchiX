Grup	Is Sýrasý	Bölüm	Alt Bölüm	Açýklama	Durum	Test Durum	Test Tarihi
10	1,0100	Mimari Genel Çerçeve	Katman yapýsý (Solution/Common/Domain/Infrastructure/Application/API)	"Hibrit model kuruldu; katmanlar mevcut."	Tamamlandý	Gerek Yok	
10	2,0100	Common Katmaný	Çekirdek (Result, Error, Correlation, Guard, Clock)	Çekirdek yapýlar hazýr.	Tamamlandý	eksik - Guard	
10	2,0200	Common Katmaný	Paging soyutlamasý	Sayfalama interface ve servisleri yazýlacak.	Tamamlandý		
10	2,0300	Common Katmaný	Filtering soyutlamasý	Baþlar, biter, arasýnda vb. filtre mantýðý eklenecek.	Tamamlandý		
10	2,0400	Common Katmaný	Sorting soyutlamasý	Kolon bazlý sýralama helper’ý yazýlacak.	Tamamlandý	tamamlandý	11.09.2025
10	2,0500	Common Katmaný	Çok Dillilik	ContextName/FieldName tasarýmýyla DB tabanlý çok dilli destek eklenecek.	Tamamlandý	tamamlandý	10.09.2025
10	3,0100	mimari Genel Çerçeve	BaseEntity	Id, CreatedAt, UpdatedAt alanlarý var.	Tamamlandý	Gerek yok	
10	3,0200	Domain Katmaný	Soft-delete	Status alanýyla iþaretleme + filtre.	Tamamlandý	tamamlandý	16.09.2025
10	3,0210	Domain Katmaný	QueryableSoftDeleteExtensions		Tamamlandý	tamamlandý	16.09.2025
10	3,0220	database	tablolarda tamamý eksi olarak insert olmuþ identity sorunu olmuþ		Tamamlandý	Gerek Yok	16.09.2025
10	3,0300	Domain Katmaný	Domain events	Entity event raise/handler altyapýsý.	Tamamlandý	tamamlandý	16.09.2025
10	4,0100	Infrastructure	DbContext/Repository/UoW	EF Core DbContext, Repository, UnitOfWork hazýr.	Tamamlandý	tamamlandý	
10	4,0200	Infrastructure	Cache arabirimleri	Memory/Redis interface yapýlacak. Cache Yönetimi 	Tamamlandý	tamamlandý	16.09.2025
10	4,0210	Infrastructure	Cache arabirimleri/ Redis için serileþtirme seçenekleri (JsonSerializerOptions vs.)	"Amaç: .NET nesnelerini Redis’e koyarken/metinden geri okurken nasýl yazýp okuyacaðýný belirlemek.

Uyumluluk: Farklý servisler ayný veriyi okuyacaksa, hepsi ayný biçimi anlamalý (ör. tarih biçimi, sayý noktasý/virgülü, camelCase/pascalCase).

Boyut/performans: Gereksiz alanlarý yazmazsan veriler daha küçük olur, að daha az yorulur, daha hýzlý çalýþýr.

Doðruluk: null alanlarý yazýp yazmamak, varsayýlan deðerleri atlamak gibi ayarlarla yanlýþ anlama riskini azaltýrsýn.

Geriye dönük uyum: Versiyon yükseltirken alan eklesen bile eski veriler patlamasýn diye toleranslý okuma yapabilirsin.

Kýsaca: “Ayný dili konuþalým, daha az yer kaplayalým, hýzlý ve hatasýz okuyalým.”"	Tamamlandý	tamamlandý	17.09.2025
10	4,0220	Infrastructure	Cache arabirimleri/ “Cache key policy” helper’larý (prefix, tenant, culture vb.)	"Amaç: Cache anahtarlarýný tutarlý ve çakýþmasýz üretmek.

Ayrýþtýrma: Çok kiracýlý (multi-tenant) sistemde tenantA:users:123 ile tenantB:users:123 karýþmaz.

Yerelleþtirme: Dil/kültüre göre farklý sonuçlarý ayrý tutarsýn: tr-TR:menu ve en-US:menu.

Versiyonlama: Þema/veri deðiþtiðinde v2:product:42 gibi anahtarlarla eski cache’i kýrarsýn (bayat veri okunmaz).

Bulunabilirlik: Ortak bir kural olunca anahtarlar tahmin edilebilir olur, yönetmesi kolaylaþýr (temizlik/izleme)."	Tamamlandý	tamamlandý	18.09.2025
10	4,0300	Infrastructure	Dýþ servis adapterleri	HttpClient wrapper þablonlarý.	Tamamlandý	tamamlandý	25.09.2025
10	5,0100	Test ve Kalite	Integration Tests	ArchiXTests.Api ile integration testler.	Tamamlandý	tamamlandý	25.08.2025
10	5,0200	Test ve Kalite	Unit Tests	Common/Domain için unit testler.	Tamamlandý	tamamlandý	25.08.2025
10	5,0300	Test ve Kalite	Kod kalite analizleri	Analyzer + StyleCop kurallarý.	Ýptal	iþler iptal edildi	
10	6,0100	CI/CD	Pipeline (ci/quick)	GitHub Actions minimal pipeline çalýþýyor.	Tamamlandý		
10	6,0200	CI/CD	Build/test coverage	dotnet test + coverlet. Bu iþ Git  de testleri çalýþtýcaktý ama gerek yok ben VS 2022 yapýyorum sürekli hata veriyordu	Ýptal	Ýptal	26.09.205
10	6,0300	CI/CD	Migration/seed otomasyonu	EF migration CI/CD’de. BU iþ Ýptal ve 6,3050 iþi bunun yerine yapýlacak	Ýptal		27.09.2025
10	6,0350	CI/CD	Migration/Seed otomasyonu (Git baðýmsýz, kütüphane içi)	"ArchiX bir uygulama prohjesine referans edildiðind db iþlemleri proje içinde metodlarla yapýlacak.ArchiX kütüphanesi, Create() ve Update() metodlarýyla sabit DB adý ve sabit kullanýcý adýyla veritabanýný oluþturur/günceller; sadece server ve parola parametreyle gelir. EF migrations kütüphanede gömülü, çalýþma zamanýnda uygulanýr. Git/CI gerekmez."	Tamamlandý	tamamlandý	01.10.2025
10	6,0400	CI/CD	Log/metrics/tracing entegrasyonu	Health + observability.(Not: kütüphane içi olacaðýna göre bu o þekilde düzenlenecek 6,035 iþine bak)	Tamamlandý	tamamlandý	04.10.2025
10	6,5000	Temizlik	Klasor/Ýsim standartlarý	6-06-Klasor-Isým-Duzetme_Isleri.docx	Ýptal		
10	6,6000	Temizlik	Loglarýn Temizlik	Bir çok gerekli gereksiz lod dosyaarý oluþmuþ. Bunlar bir stadtarta oturtulmalý	Tamamlandý	tamamlandý	05.10.2025
10	6,6490	Temizlik	Git Prod u merge et main e	hata ile son iþlemler prod da yapýlmýþ. Maine aktar. Sonra produ pasif et. Prod ilemlerinde aktif dersin en baþta	Tamamlandý	tamamlandý	18.10.2025
10	7,0100	Application (CQRS)	Command/Query Handler	IRequestHandler yapýsý.	Tamamlandý	Test Yok	
10	7,0200	Application (CQRS)	Validation Pipeline	FluentValidation middleware.	Tamamlandý	tamamlandý	21.10.2025
10	7,0300	Application (CQRS)	Mapping Profilleri	AutoMapper profile ekleme.	Tamamlandý	tamamlandý	21.10.2025
10	7,0400	Application (CQRS)	Transaction Davranýþlarý	UoW + transaction pipeline.	Tamamlandý	tamamlandý	27.10.2025
10	7,0500	Application (CQRS)	Yetkilendirme Davranýþlarý	Authorization behavior.	Tamamlandý	tamamlandý	28.10.2025
10	9,0100	Performans	EF Optimizasyonlarý	AsNoTracking, Include stratejisi.	Baþlanmadý		
10	9,0200	Performans	Cache Stratejileri	Query cache.	Baþlanmadý		
10	9,0300	Performans	Ýyileþtirme Metrikleri	Benchmark, perf counter.	Tamamlandý	tamamlandý	01.11.2025
10	11,0100	Hata Yönetimi	Global Exception Middleware	Exception yakalayýp JSON loglayan middleware.	Tamamlandý	eksik- 8 nolu iþ test yapýldý	
10	11,0200	Hata Yönetimi	layout & Tema	CorrelationId ve TraceId ekleyen middleware.	Tamamlandý		
10	11,0300	Hata Yönetimi	Exception Logger	Hata detaylarýný sýnýflandýran yardýmcý sýnýf.	Tamamlandý		
10	14,0100	Güvenlik	JWT Kimlik Doðrulama	Token üretimi + refresh. Ýkli veeri doðrulama token, sms, email seçnekler ve patametik olacak	Baþlanmadý		
10	14,0200	Güvenlik	2FA Opsiyonu	SMS/Authenticator integration.	Baþlanmadý		
10	14,0300	Güvenlik	Rol/Claim Bazlý Yetki	Policy-based auth.	Baþlanmadý		
10	14,0400	Güvenlik	Anti-forgery & Rate limiting	CSRF + throttling.	Baþlanmadý		
10	14,0500	Güvenlik	Veri Maskeleme	Sensitive data masking.	Baþlanmadý		
10	17,0100	Ekran Tasarýmlarý	Layout & Tema	Layout, tema tercihleri.	Baþlanmadý		
10	17,0550	Ekran Tasarýmlarý	Login Ekraný	Login, admin ekranlarý. Þifre deðiþtirme, zorunlu güncelle ekran ve parametrik kararlarý	Baþlanmadý		
10	20,0100	UI/GRID	Kolon sýralama	Grid baþlýðý sort.	Baþlanmadý		
10	20,0200	UI/GRID	Filtreler	Baþlar, biter, arasýnda.	Baþlanmadý		
10	20,0300	UI/GRID	Çoklu seçim filtreleri	Checkbox/dropdown filtre.	Baþlanmadý		
10	20,0400	UI/GRID	Export: Excel, PDF, CSV, TXT,JSON,XML	Export servisleri.	Baþlanmadý		
50	22,0200	Ekran Tasarýmlarý	DB veri tipine göre ekran kolon tipi tasarlama. 	Datetime için DatetimePicker eþleþme yada 250 den fazla olanlr için mültiarea vs	Baþlanmadý		
50	22,0300	Ekran Tasarýmlarý	Admin Paneli		Baþlanmadý		
50	23,0000	UI/GRID	Import: Excel, CSV	Import ve validasyon.	Baþlanmadý		
50	23,1000	UI/GRID	Inline Edit	Grid satýrýnda düzenleme.	Baþlanmadý		
50	24,0000	CI/CD	"Production projesinde  test yapýlacak. appsettings.json dosyasýnda  ""AllowDbOps"": true olacak"	gerçek bir prod projesi açýlacak. Gerçekten yeni bir database create ediyprmu bak.IsDevolopment True olacak.launchsettings.json deðiþecek	Baþlanmadý		
50	25,1000	Hata Yönetimi	Hata tablosu (DB logging)	Hatalarýn DB’ye yazýlacaðý tablo ve EF konfigürasyonu.	Baþlanmadý		
50	25,2000	Hata Yönetimi	Ýzleme ekranlarý (Admin UI)	Hata kayýtlarýnýn admin panelinde listelenmesi.	Baþlanmadý		
50	25,3000	Hata Yönetimi	Log arama uçlarý (API)	Hata kayýtlarý üzerinde API tabanlý arama/filtre uçlarý.	Baþlanmadý		
50	25,4000	Hata Yönetimi	E-posta bildirimleri	Kritik severity loglarda e-posta uyarýsý gönderme.	Baþlanmadý		
50	31,0100	Fonksiyonlar	Row-level Functions	Row-level hesaplamalar.	Baþlanmadý		
50	31,0200	Fonksiyonlar	Aggregate Functions	Grup toplamlarý.	Baþlanmadý		
50	31,0300	Fonksiyonlar	Window Functions	Analitik fonksiyonlar.	Baþlanmadý		
50	31,0400	Fonksiyonlar	Zaman & Tarih Fonksiyonlarý	DateTime yardýmcýlarý.	Baþlanmadý		
50	39,0100	ArchiX Otomasyon	Controller/ViewModel üretimi	T4/CLI üretim.	Baþlanmadý		
50	39,0200	ArchiX Otomasyon	Repo/UoW üretimi	Otomatik Repository/UoW.	Baþlanmadý		
50	39,0300	ArchiX Otomasyon	Mapping/Validation üretimi	Profile + Validation pipeline.	Baþlanmadý		
50	39,0400	ArchiX Otomasyon	Program.cs ekleme þablonu	Otomatik Program.cs injection.	Baþlanmadý		
50	100,0100	Import Servisleri	Import Servisleri	Excel, CSV import.	Baþlanmadý		
50	105,0100	Raporlama Tool	Pivot & Charts	List, pie, line, column.	Baþlanmadý		
50	105,0300	Raporlama Tool	Pivot Konfigürasyonu	Dinamik pivot setup.	Baþlanmadý		
50	110,0000	Fonksiyonlar	OCR	Perakende fiþini okuma vs	Baþlanmadý		
