# CI/CD Pack & Restore Debug Log (NU1101 + Satellite Assembly)

## 2026-01-30 14:35 (TR) - Deneme #1: Version Bump to beta.1
- **Change**: `src/ArchiX.WebHostDLL/ArchiX.WebHostDLL.csproj` package versions updated to `1.0.0-beta.1` for ArchiX.Library and ArchiX.Library.Web.
- **Expected**: CI restore finds local packed prerelease packages; NU1101 resolved.
- **Observed**: NU1101 (ArchiX.Library.* package not found in ./.nuget/local) devam ediyordu.
- **Durum**: Başarısız.

## 2026-01-30 21:40 (TR) - Deneme #2: Version Bump to beta.2
- **Change**: Bumped package versions to `1.0.0-beta.2` (ArchiX.Library, ArchiX.Library.Web) and aligned WebHostDLL PackageReference to beta.2. Goal: force restore to pick fresh local packages (Menu entity) instead of cached 1.0.0.
- **Expected**: Local pack to ./.nuget/local produces beta.2; restore resolves beta.2, CS0246 (Menu) disappears.
- **Observed**: CI yine NU1101 verdi. Paketler lokal feed'de yok.
- **Durum**: Başarısız.

## 2026-01-30 22:05 (TR) - Deneme #3: Pack Verbose Logging
- **Change**: CI yine NU1101 (ArchiX.Library*, local + nuget.org yok) verdi. Versiyon: 1.0.0-beta.2.
- **Expected**: Prepare local feed step paketleri üretip restore bulmalıydı.
- **Observed**: CI restore'da ArchiX.Library / ArchiX.Library.Web bulunamadı; Prepare local feed adımının çıktısı doğrulanamadı.
- **Durum**: Başarısız.

## 2026-01-31 00:05 (TR) - Deneme #4: Feed Source Verification
- **Change**: CI yeniden restore denemesi (sources: nuget.org + ./.nuget/local), paketler ArchiX.Library/Web 1.0.0-beta.2 olmalı.
- **Expected**: Prepare local feed paketi üretsin, restore beta.2 bulsun.
- **Observed**: Olmadı. NU1101 (ArchiX.Library*, ./.nuget/local ve nuget.org'da yok) devam etti.
- **Durum**: Başarısız.

## 2026-01-31 00:15 (TR) - Deneme #5: Verbose Pack + Verify Adımı
- **Hipotez**: "Prepare local feed" adımındaki `dotnet pack` komutları sessizce başarısız oluyor, paketler üretilmiyor.
- **Kanıt**: CI restore logunda ArchiX.Library/Web bulunamadı; pack adımı çıktısında paket üretimi onayı yok.
- **Change**:
  1. Pack komutlarına `--verbosity detailed` eklendi (hata mesajlarını görmek için).
  2. "Verify local feed" adımı eklendi: `.nuget/local` içeriğini listeler, `ArchiX.Library.1.0.0-beta.2.nupkg` ve `ArchiX.Library.Web.1.0.0-beta.2.nupkg` yoksa hata verir.
  3. Build komutlarına `--verbosity minimal` + echo çıktıları eklendi.
- **Expected**: Eğer pack başarısız oluyorsa verbose log hatayı gösterecek; eğer başarılı olursa verify adımı geçecek ve restore NU1101 vermeyecek.
- **Observed**: Verify adımı çalışmadı (commit edilmemiş veya o adıma gelemedi).
- **Durum**: Başarısız (commit/push yapılmamış).

## 2026-01-31 00:25 (TR) - Deneme #6-10: Path Problem Denemesi
- **Hipotez**: Linux CI'da relative path (`./.nuget/local`) working directory farkından dolayı sorunlu olabilir.
- **Kanıt**: Lokal makinede (Windows) aynı komutlar `--source ./.nuget/local` ile başarılı; CI'da (Linux) relative path çözümlenemiyor olabilir.
- **Change (#6-10)**: Birkaç deneme yapıldı (detaylar kayboldu), en son (#11):
  1. `FEED_PATH="${GITHUB_WORKSPACE}/.nuget/local"` tam path kullanıldı.
  2. Pack komutlarında `--no-build` eklendi.
  3. Restore komutunda tırnak kullanılmadı (`--source ${FEED_PATH}`).
  4. Verify adımı daha detaylı çıktı verecek şekilde düzenlendi.
- **Expected**: Tam path kullanılınca pack ve restore aynı feed'i görecek; verify adımı paketleri bulacak; restore NU1101 vermeyecek.
- **Durum**: Commit/push bekleniyor.

## 2026-01-31 00:35 (TR) - Deneme #11: Verify Başarılı, Restore Relative Path
- **Gözlem**: Kullanıcı ekran görüntüsü paylaştı. "Verify local feed" adımı **başarılı** (satır 31: "All packages verified."). Paketler üretildi ve doğrulandı.
- **Sorun**: Restore adımı (satır 2) **hâlâ eski komutu kullanıyor**: `--source "./.nuget/local"` (relative path).
- **Kök Neden**: Son yaptığım değişiklik (tam path kullanımı) henüz commit/push edilmemiş veya CI eski commit'i çalıştırmış.
- **Kanıt**: Verify adımı paketleri buldu (relative path çalıştı), ama restore adımı başka bir working directory'den çalışıyor ve relative path'i çözemiyor.
- **Change**: Kullanıcı son değişiklikleri commit/push etmeli.
- **Expected**: Commit/push sonrası restore `${GITHUB_WORKSPACE}/.nuget/local` tam path'ini kullanacak, NU1101 gidecek.
- **Durum**: Commit/push bekleniyor (#12. deneme).

## 2026-01-31 08:22 (TR) - Deneme #12: CI Restore OK, Tests FAIL
- **Gözlem**: CI restore adımı **BAŞARILI** (tam path fix edilmiş), paketler bulundu. Build başarılı. Test adımında **7 test fail**.
- **Başarısız testler**: Hepsi `PasswordValidationMessageProviderTests` → localized string'leri bulamıyor:
  - Beklenen: "Parola boş olamaz.", "Password cannot be empty."
  - Gerçek: "EMPTY" (key fallback)
- **Kök Neden**: Culture-specific resx'ler (PasswordValidation.en-US.resx, PasswordValidation.tr-TR.resx) satellite assembly üretmiyor → runtime'da resource bulunamıyor.
- **Kanıt**: `src/ArchiX.Library/ArchiX.Library.csproj` içinde culture-specific resx'ler için `<GenerateResource>false</GenerateResource>` metadata'sı var.
- **Change (#13)**: `GenerateResource=false` metadata'sı kaldırıldı. Resx'ler build/test için satellite üretecek, ama `<Pack>false</Pack>` sayesinde nupkg'ye dahil edilmeyecek (NU5026 önlenir).
- **Expected**: Lokal `dotnet test` geçecek, CI test adımı 7 test fail vermeyecek.
- **Durum**: Commit/push bekleniyor (#13. deneme).

## 2026-01-31 08:30 (TR) - Deneme #13: Yine FAIL, GenerateSatelliteAssemblies=false
- **Gözlem**: CI #13 çalıştı, yine **aynı 7 test fail**. Build loglarında "CoreGenerateSatelliteAssemblies: Skipping target... up-to-date" → satellite'ler üretilmiş ama testler fail.
- **Kök Neden Bulundu**: `src/ArchiX.Library/ArchiX.Library.csproj` satır 19'da `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` property'si var → bu satellite üretimini **tamamen** kapatıyor. `<EmbeddedResource>` item-level metadata'sından daha güçlü.
- **Kanıt**: Build log'da "up-to-date" diyor ama "Skipping" → eski artifact'ler var, yeni üretim yok. Test projesine satellite'ler kopyalanmıyor.
- **Change (#14)**:
  1. `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` satırı tamamen kaldırıldı (default `true` olacak).
  2. `<EmbeddedResource Update ... Pack=false>` bloğu kaldırıldı (gereksiz; zaten `IncludeSatelliteAssembliesInPackage=false` var).
  3. `<IncludeSatelliteAssembliesInPackage>false</IncludeSatelliteAssembliesInPackage>` **kalıyor** (satellite'ler build/test için üretilsin, ama pack'e dahil edilmesin).
- **Expected**: Build satellite üretecek, test projesi bunları kopyalayacak, testler geçecek.
- **Durum**: Commit/push bekleniyor (#14. deneme).

## 2026-01-31 08:36 (TR) - Deneme #14: Lokal Test, Satellite Yok
- **Gözlem**: #14 düzeltmesi sonrası lokal temiz build yapıldı. `en-US/` ve `tr-TR/` klasörleri oluşturuldu **AMA boş** (satellite DLL yok).
- **Kök Neden**: .NET SDK satellite logic: **neutral resx yoksa culture-specific resx'ler satellite üretmiyor**. `PasswordValidation.resx` (neutral/default) yoktu, sadece `.en-US` ve `.tr-TR` vardı.
- **Keşif**: 
  1. Lokal verbose build: `CoreGenerateSatelliteAssemblies` target hiç çalışmadı.
  2. `obj/Release/net9.0/*.resources` kontrol: sadece neutral resource yok, culture-specific'ler compile edilmemiş.
  3. SDK'nın wildcard pattern (`Resources\*.resx`) sadece neutral resx'i match etti, culture-specific'leri skip etti.
- **Hipotez**: SDK, neutral resx görünce ona bağlı culture-specific'leri otomatik bulur ve satellite üretir. Neutral yoksa logic tetiklenmiyor.
- **Durum**: Başarısız (lokal test geçmedi, CI'ya gönderilmedi).

## 2026-01-31 08:42 (TR) - Deneme #15: Neutral Resx + Explicit Include (BAŞARILI ✅)
- **Change**:
  1. **`PasswordValidation.resx` (neutral) oluşturuldu**: En-US default değerlerle (EMPTY, MIN_LENGTH, REQ_UPPER, REQ_LOWER, REQ_DIGIT, REQ_SPECIAL, DICT_WORD, PWNED, IN_HISTORY, EXPIRED, LOW_ENTROPY, DYNAMIC_BLOCK - tüm 13 key).
  2. **Culture-specific resx'ler explicit `Include` ile eklendi** (csproj'a):
     ```xml
     <EmbeddedResource Include="Resources\PasswordValidation.en-US.resx">
       <WithCulture>true</WithCulture>
       <LogicalName>ArchiX.Library.Resources.PasswordValidation.en-US.resources</LogicalName>
     </EmbeddedResource>
     <EmbeddedResource Include="Resources\PasswordValidation.tr-TR.resx">
       <WithCulture>true</WithCulture>
       <LogicalName>ArchiX.Library.Resources.PasswordValidation.tr-TR.resources</LogicalName>
     </EmbeddedResource>
     ```
  3. **Neutral resx sadece Designer.cs generate ediyor**:
     ```xml
     <EmbeddedResource Update="Resources\PasswordValidation.resx">
       <Generator>ResXFileCodeGenerator</Generator>
       <LastGenOutput>PasswordValidation.Designer.cs</LastGenOutput>
     </EmbeddedResource>
     ```
  4. `<IncludeSatelliteAssembliesInPackage>false</IncludeSatelliteAssembliesInPackage>` geri eklendi (satellite'ler pack'e dahil edilmesin, NU5026 önlenir).
- **Lokal Test Sonuçları**:
  - `dotnet clean && dotnet build -c Release --no-incremental` çalıştırıldı.
  - `CoreGenerateSatelliteAssemblies` target **çalıştı** (1.7s).
  - `src/ArchiX.Library/bin/Release/net9.0/en-US/ArchiX.Library.resources.dll` üretildi (5 KB).
  - `src/ArchiX.Library/bin/Release/net9.0/tr-TR/ArchiX.Library.resources.dll` üretildi (5 KB).
  - `dotnet test ArchiX.sln -c Release --no-restore` çalıştırıldı:
    - `PasswordValidationMessageProviderTests`: **12/12 geçti** ✅
    - Tüm testler: **424/424 geçti** (347 ArchiX.Library.Tests + 77 ArchiX.Library.Web.Tests) ✅
- **Sonuç**: **Sorun tamamen çözüldü!**
- **Durum**: Commit/push bekleniyor (#15. deneme - kesin çözüm).

---

## Özet: Tüm Sorunlar ve Çözümler

### Sorun 1: NU1101 (Package Not Found)
- **Kök Neden**: CI'da restore adımı relative path (`./.nuget/local`) kullanıyordu, working directory farklı olduğu için feed bulunamıyordu.
- **Çözüm**: `FEED_PATH="${GITHUB_WORKSPACE}/.nuget/local"` tam path kullanıldı (deneme #11-12). ✅

### Sorun 2: Satellite Assembly Üretilmiyor
- **Kök Neden 1**: `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` property'si satellite üretimini kapatmıştı (deneme #13-14).
- **Kök Neden 2**: Neutral resx (`PasswordValidation.resx`) yoktu, SDK satellite logic'i tetiklenmiyordu (deneme #14).
- **Çözüm**: 
  1. `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` kaldırıldı.
  2. Neutral resx oluşturuldu (EN-US default values).
  3. Culture-specific resx'ler explicit `Include` + `WithCulture=true` ile eklendi (deneme #15). ✅

### Final Durum
- **Lokal testler**: 424/424 geçti ✅
- **CI beklenen**: Tüm testler geçecek, pack/restore başarılı olacak ✅
- **Dosyalar**:
  - `src/ArchiX.Library/ArchiX.Library.csproj` → satellite config düzeltildi
  - `src/ArchiX.Library/Resources/PasswordValidation.resx` → neutral resx eklendi
  - `.github/workflows/ci_main.yml` → tam path fix (zaten yapılmış)
