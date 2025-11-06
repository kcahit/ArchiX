Karar: 9,0100, 9,0200, 9,0300 ayrý kalsýn. Gerekçe: 9.01=EF optimizasyon, 9.02=cache stratejisi, 9.03=iyileþtirme metrikleri. Hedef notlarla uyumlu. Ölç-iyileþtir döngüsü için baðýmsýz çýktýlar üretir.
9,0300 — Ýyileþtirme metrikleri
* Çýktý: EF sorgu süresi histogramý. Komut sayacý. Cache hit/miss. P50/P95/P99. Prometheus/OTLP export.
* Uygulama: Var olan OpenTelemetry’yi kullan. DbCommandInterceptor veya OTel EF enstrümantasyonu. Meter adlarý standartlara uysun.
* Test: “Metric yayýmlýyor mu” smoke testi. Basit P95 doðrulamasý.
* Neden önce: 9,0100 ve 9,0200 için baseline gerekir.
9,0100 — EF optimizasyonlarý
* Çýktý: Okumada varsayýlan AsNoTracking. Doðru Include. SplitQuery varsayýlaný. Projection. Sýcak yollar için CompiledQuery. Standart paging.
* Uygulama: AppDbContext’te UseQuerySplittingBehavior(SplitQuery). Uygun yerlerde AsNoTracking/AsNoTrackingWithIdentityResolution. Repository/Handler rehberi: projection-filtre kalýplarý. CompiledQuery yardýmcýlarý.
* Test: Önce/sonra karþýlaþtýrmasý. Takip edilen nesne sayýsý, SQL komut sayýsý ve süre düþer. N+1 testi.
9,0200 — Cache stratejileri
* Çýktý: CQRS Query tarafýnda cache decorator. Anahtar standardý. TTL ve invalidation politikasý. Cache metrikleri.
* Uygulama: IMemoryCache kaydý. QueryCacheBehavior<TReq,TRes> (key = {RequestType}:{hash(request)}). Seçenekler: TTL, max size. Invalidation: Komut sonrasý entity-type sürümü artýþý veya domain event ile tag invalidation.
* Test: Hit/miss senaryolarý. TTL dolumu. Komut sonrasý invalidation. Hit oraný ölçümü.
Sýralama
1. 9,0300’ý minimumda aç ve baseline al.
2. 9,0100 optimizasyonlarý uygula.
3. 9,0200 cache’i ekle.
4. 9,0300’ý tamama erdir: hedefler ve raporlama.
Kabul kriterleri
* 9,0100: P95 sorgu süresi ve SQL çaðrý sayýsý baseline’a göre anlamlý düþer. N+1 testleri geçer.
* 9,0200: P95 okuma süresi ve DB çaðrý sayýsý ek düþüþ gösterir. Hit oraný ? %70 (ayar-baðýmlý).
* 9,0300: Metrikler Prometheus’ta görünür. P95, hit/miss ve hata oraný panoda izlenir. Dokümantasyon tamdýr.
Kod dokunuþ noktalarý
* AppDbContext ve EF ayarlarý.
* Pipeline/Behaviors: QueryCacheBehavior.cs.
* ServiceCollectionExtensions: IMemoryCache ve davranýþ kayýtlarý.
* Testler: EF perf, QueryCacheBehavior, metrik smoke.













TREEVÝEW
+-- ArchiX.Library (PROJE)
|   +-- Infrastructure
|   |   +-- EFCore
|   |   |   +-- Repository.cs (Güncelleme - 9,0100)
|   |   |   +-- QueryableOptimizationExtensions.cs (Yeni - 9,0100)
|   |   |   +-- DbCommandMetricsInterceptor.cs (Yeni - 9,0300)
|   |
|   |   +-- Caching
|   |       +-- MemoryCacheService.cs (Güncelleme - 9,0300)
|   |       +-- RedisCacheService.cs (Güncelleme - 9,0300)
|   |
|   +-- Runtime
|       +-- Observability
|           +-- ObservabilityServiceCollectionExtensions.cs (Güncelleme - 9,0300)

+-- ArchiX.Library.Tests (PROJE)
|   +-- Test
|   |   +-- InfrastructureTests
|   |   |   +-- RepositoryEfOptimizationTests.cs (Yeni - 9,0100)
|   |
|   |   +-- RunTimeTests
|   |       +-- ObservabilityTests
|   |           +-- DbCommandMetricsInterceptorTests.cs (Yeni - 9,0300)
|   |           +-- CacheServiceMetricsEmissionTests.cs (Yeni - 9,0300)

+-- ArchiX.WebApplication (PROJE)
|   +-- Abstractions
|   |   +-- Caching
|   |       +-- ICacheableRequest.cs (Yeni - 9,0200)
|   |       +-- CacheOptions.cs (Yeni - 9,0200)
|   |
|   +-- Behaviors
|   |   +-- QueryCacheBehavior.cs (Yeni - 9,0200)
|   |
|   +-- Pipeline
|       +-- ServiceCollectionExtensions.cs (Güncelleme - 9,0200)
|       +-- CqrsRegistrationExtensions.cs (Güncelleme - 9,0200)

+-- ArchiX.WebApplication.Tests (PROJE)
|   +-- Behaviors
|   |   +-- QueryCacheBehavior
|   |       +-- QueryCacheBehaviorTests.cs (Yeni - 9,0200)
|   |
|   +-- Pipeline
|       +-- ServiceCollectionExtensions_CacheBehaviorTests.cs (Yeni - 9,0200)

+-- docs
|   +-- 9_Performance
|       +-- 9_0100_EF_Optimization.md (Yeni - 9,0100)
|       +-- 9_0200_Cache_Strategy.md (Yeni - 9,0200)
|       +-- 9_0300_Metrics.md (Yeni - 9,0300)

Notlar:
* 9,0100: EF sorgularýnda AsNoTracking/AsSplitQuery ve dâhil etme stratejisi için Repository.cs güncellemesi ve QueryableOptimizationExtensions.cs eklenecek.
* 9,0200: Sadece sorgular için araya giren CachingBehavior ve iþaretleyici arayüzleri eklenecek; CQRS kayýtlarý güncellenecek.
* 9,0300: Cache ve DB iþlemlerinde metrik yayýlýmý için MemoryCacheService.cs, RedisCacheService.cs ve Repository.cs içine ölçüm çaðrýlarý eklenecek; ölçüm ve basit benchmark testleri eklenecek.
Sýra noÝþin numarasýProje KoduKlasör yapýsýDosya adý19,0300ArchiX.LibraryRuntime/ObservabilityObservabilityServiceCollectionExtensions.cs (Güncelleme)29,0300ArchiX.LibraryInfrastructure/EFCoreDbCommandMetricsInterceptor.cs (Yeni)39,0300ArchiX.LibraryInfrastructure/CachingMemoryCacheService.cs (Güncelleme)49,0300ArchiX.LibraryInfrastructure/CachingRedisCacheService.cs (Güncelleme)59,0300ArchiX.Library.TestsTest/RunTimeTests/ObservabilityTestsDbCommandMetricsInterceptorTests.cs (Yeni)69,0300ArchiX.Library.TestsTest/RunTimeTests/ObservabilityTestsCacheServiceMetricsEmissionTests.cs (Yeni)79,0300docs9_Performance9_0300_Metrics.md (Yeni)89,0100ArchiX.LibraryInfrastructure/EFCoreRepository.cs (Güncelleme)99,0100ArchiX.LibraryInfrastructure/EFCoreQueryableOptimizationExtensions.cs (Yeni)109,0100ArchiX.Library.TestsTest/InfrastructureTestsRepositoryEfOptimizationTests.cs (Yeni)119,0100docs9_Performance9_0100_EF_Optimization.md (Yeni)129,0200ArchiX.WebApplicationAbstractions/CachingICacheableRequest.cs (Yeni)139,0200ArchiX.WebApplicationAbstractions/CachingCacheOptions.cs (Yeni)149,0200ArchiX.WebApplicationBehaviorsQueryCacheBehavior.cs (Yeni)159,0200ArchiX.WebApplicationPipelineServiceCollectionExtensions.cs (Güncelleme)169,0200ArchiX.WebApplicationPipelineCqrsRegistrationExtensions.cs (Güncelleme)179,0200ArchiX.WebApplication.TestsBehaviors/QueryCacheBehaviorQueryCacheBehaviorTests.cs (Yeni)189,0200ArchiX.WebApplication.TestsPipelineServiceCollectionExtensions_CacheBehaviorTests.cs (Yeni)199,0200docs9_Performance9_0200_Cache_Strategy.md (Yeni)
Eþleþme tablosu kod – test eþleþmesi
Sýra noÝþin numarasýProje KoduKlasör yapýsýDosya adýTest Sýra NO19,03ArchiX.LibraryRuntime/ObservabilityObservabilityServiceCollectionExtensions.cs (Güncelleme)5, 629,03ArchiX.LibraryInfrastructure/EFCoreDbCommandMetricsInterceptor.cs (Yeni)539,03ArchiX.LibraryInfrastructure/CachingMemoryCacheService.cs (Güncelleme)649,03ArchiX.LibraryInfrastructure/CachingRedisCacheService.cs (Güncelleme)689,01ArchiX.LibraryInfrastructure/EFCoreRepository.cs (Güncelleme)1099,01ArchiX.LibraryInfrastructure/EFCoreQueryableOptimizationExtensions.cs (Yeni)10129,02ArchiX.WebApplicationAbstractions/CachingICacheableRequest.cs (Yeni)17, 18139,02ArchiX.WebApplicationAbstractions/CachingCacheOptions.cs (Yeni)17, 18149,02ArchiX.WebApplicationBehaviorsQueryCacheBehavior.cs (Yeni)17, 18159,02ArchiX.WebApplicationPipelineServiceCollectionExtensions.cs (Güncelleme)18169,02ArchiX.WebApplicationPipelineCqrsRegistrationExtensions.cs (Güncelleme)18

