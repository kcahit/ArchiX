## 2026-01-30 14:35 (TR)
- Change: Updated `src/ArchiX.WebHostDLL/ArchiX.WebHostDLL.csproj` package versions to `1.0.0-beta.1` for ArchiX.Library and ArchiX.Library.Web.
- Expected: CI restore finds local packed prerelease packages; NU1101 resolved.
- Observed: önceki koşulda NU1101 (ArchiX.Library.* package not found in ./.nuget/local) devam ediyordu. Bu giriş sonrası yeniden denenecek.

## 2026-01-30 21:40 (TR)
- Change: Bumped package versions to `1.0.0-beta.2` (`ArchiX.Library`, `ArchiX.Library.Web`) and aligned WebHostDLL PackageReference to beta.2. Goal: force restore to pick fresh local packages (Menu entity) instead of cached 1.0.0.
- Expected: Local pack to ./.nuget/local produces beta.2; restore resolves beta.2, CS0246 (Menu) disappears.
- Observed: Pending (pack/restore to be rerun).

## 2026-01-30 22:05 (TR)
- Change: CI yine NU1101 (ArchiX.Library*, local + nuget.org yok) verdi. Versiyon: 1.0.0-beta.2.
- Expected: Prepare local feed step paketleri üretip restore bulmalıydı.
- Observed: CI restore'da ArchiX.Library / ArchiX.Library.Web bulunamadı; Prepare local feed adımının çıktısı doğrulanamadı.

## 2026-01-31 00:05 (TR)
- Change: CI yeniden restore denemesi (sources: nuget.org + ./.nuget/local), paketler ArchiX.Library/Web 1.0.0-beta.2 olmalı.
- Expected: Prepare local feed paketi üretsin, restore beta.2 bulsun.
- Observed: Olmadı. NU1101 (ArchiX.Library*, ./.nuget/local ve nuget.org'da yok) devam etti.

## 2026-01-31 00:15 (TR) - Kök Neden Analizi
- **Hipotez**: "Prepare local feed" adımındaki `dotnet pack` komutları sessizce başarısız oluyor, paketler üretilmiyor.
- **Kanıt**: CI restore logunda ArchiX.Library/Web bulunamadı; pack adımı çıktısında paket üretimi onayı yok.
- **Çözüm Denemesi**:
  1. Pack komutlarına `--verbosity detailed` eklendi (hata mesajlarını görmek için).
  2. "Verify local feed" adımı eklendi: `.nuget/local` içeriğini listeler, `ArchiX.Library.1.0.0-beta.2.nupkg` ve `ArchiX.Library.Web.1.0.0-beta.2.nupkg` yoksa hata verir.
  3. Build komutlarına `--verbosity minimal` + echo çıktıları eklendi (hangi aşamada olduğunu görmek için).
- **Beklenen**: Eğer pack başarısız oluyorsa verbose log hatayı gösterecek; eğer başarılı olursa verify adımı geçecek ve restore NU1101 vermeyecek.
- **Durum**: Commit/push bekleniyor, sonraki CI run'ında log incelenecek.

## 2026-01-31 00:25 (TR) - Path Problemi Hipotezi
- **Gözlem**: Kullanıcı hâlâ aynı NU1101 hatası aldı. "Verify local feed" adımı logunda görünmedi (commit edilmemiş veya o adıma gelemedi).
- **Yeni Hipotez**: Linux CI'da relative path (`./.nuget/local`) working directory farkından dolayı sorunlu olabilir. Pack komutu bir dizinde, restore başka dizinde çalışıyor olabilir.
- **Kanıt**: Lokal makinede (Windows) aynı komutlar `--source ./.nuget/local` ile başarılı; CI'da (Linux) relative path çözümlenemiyor olabilir.
- **Çözüm Denemesi (#11)**:
  1. `FEED_PATH="${GITHUB_WORKSPACE}/.nuget/local"` tam path kullanıldı (her adımda tutarlı).
  2. Pack komutlarında `--no-build` eklendi (build zaten yapıldı, gereksiz MSBuild property karışımını önlemek için).
  3. Restore komutunda tırnak kullanılmadı (`--source ${FEED_PATH}` yerine `--source "${FEED_PATH}"`).
  4. Verify adımı daha detaylı çıktı verecek şekilde düzenlendi (`|| echo` fallback'leri eklendi).
- **Beklenen**: Tam path kullanılınca pack ve restore aynı feed'i görecek; verify adımı paketleri bulacak; restore NU1101 vermeyecek.
- **Durum**: Commit/push bekleniyor.

## 2026-01-31 00:35 (TR) - Verify Başarılı, Restore Hâlâ Relative Path
- **Gözlem**: Kullanıcı ekran görüntüsü paylaştı. "Verify local feed" adımı **başarılı** (satır 31: "All packages verified."). Paketler üretildi ve doğrulandı.
- **Sorun**: Restore adımı (satır 2) **hâlâ eski komutu kullanıyor**: `--source "./.nuget/local"` (relative path).
- **Kök Neden**: Son yaptığım değişiklik (tam path kullanımı) henüz commit/push edilmemiş veya CI eski commit'i çalıştırmış.
- **Kanıt**: Verify adımı paketleri buldu (relative path çalıştı), ama restore adımı başka bir working directory'den çalışıyor ve relative path'i çözemiyor.
- **Çözüm**: Kullanıcı son değişiklikleri commit/push etmeli. Yeni CI run'ında restore `${GITHUB_WORKSPACE}/.nuget/local` tam path'ini kullanacak.
- **Beklenen**: Commit/push sonrası restore NU1101 vermeyecek.
- **Durum**: Commit/push bekleniyor (#12. deneme).

## 2026-01-31 08:22 (TR) - CI Restore OK, Tests FAIL (Culture-Specific Resx)
- **Gözlem**: CI restore adımı başarılı (tam path fix edilmiş), paketler bulundu. Build başarılı. Test adımında 7 test fail.
- **Başarısız testler**: Hepsi `PasswordValidationMessageProviderTests` → localized string'leri bulamıyor (beklenen "Parola boş olamaz.", gerçek "EMPTY").
- **Kök Neden**: `<GenerateResource>false</GenerateResource>` ile culture-specific resx'ler (örn. `PasswordValidation.en-US.resx`) satellite assembly üretmiyor → runtime'da resource bulunamıyor.
- **Çözüm (#13)**: `src/ArchiX.Library/ArchiX.Library.csproj` içinde `GenerateResource=false` kaldırıldı. Resx'ler build/test için satellite üretecek, ama `<Pack>false</Pack>` sayesinde nupkg'ye dahil edilmeyecek (NU5026 önlenir).
- **Beklenen**: Lokal `dotnet test` geçecek, CI test adımı 7 test fail vermeyecek.
- **Durum**: Commit/push bekleniyor (#13. deneme).

## 2026-01-31 08:30 (TR) - CI #13 Yine FAIL, Kök Neden: GenerateSatelliteAssemblies=false
- **Gözlem**: CI #13 çalıştı, yine aynı 7 test fail. Build loglarında "CoreGenerateSatelliteAssemblies: Skipping target... up-to-date" → satellite'ler üretilmiş ama testler fail.
- **Kök Neden Bulundu**: `src/ArchiX.Library/ArchiX.Library.csproj` satır 19'da `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` property'si var → bu satellite üretimini **tamamen** kapatıyor. `<EmbeddedResource>` item-level `GenerateResource` metadata'sından daha güçlü.
- **Kanıt**: Build log'da "up-to-date" diyor ama "Skipping" → eski artifact'ler var, yeni üretim yok. Test projesine satellite'ler kopyalanmıyor.
- **Çözüm (#14)**:
  1. `<GenerateSatelliteAssemblies>false</GenerateSatelliteAssemblies>` satırı tamamen kaldırıldı (default `true` olacak).
  2. `<EmbeddedResource Update ... Pack=false>` bloğu kaldırıldı (gereksiz; zaten `IncludeSatelliteAssembliesInPackage=false` var).
  3. `<IncludeSatelliteAssembliesInPackage>false</IncludeSatelliteAssembliesInPackage>` **kalıyor** (satellite'ler build/test için üretilsin, ama pack'e dahil edilmesin).
- **Beklenen**: Build satellite üretecek, test projesi bunları kopyalayacak, testler geçecek. Pack NU5026 warning verebilir ama sorun yok (satellite'ler zaten pack'e dahil edilmiyor).
- **Durum**: Commit/push bekleniyor (#14. deneme).

## 2026-01-31 08:42 (TR) - ÇÖZ ÜM BULUNDU: Neutral Resx + Explicit Culture Include (#15 BAŞARILI)
- **Gözlem**: #14 düzeltmesi sonrası lokal test, satellite'ler yine üretilmedi (`en-US/`, `tr-TR/` klasörleri boş).
- **Kök Neden**: .NET SDK satellite logic: **neutral resx yoksa culture-specific resx'ler satellite üretmiyor**. `PasswordValidation.resx` (neutral/default) yoktu, sadece `.en-US` ve `.tr-TR` vardı.
- **Keşif**: Lokal temiz build + verbose log: `CoreGenerateSatelliteAssemblies` target hiç çalışmadı → culture-specific resx'ler `EmbeddedResource` item list'ine eklenmemiş (SDK'nın wildcard pattern sadece neutral resx'i match etti).
- **Çözüm (#15 - BAŞARILI)**:
  1. **`PasswordValidation.resx` (neutral) oluşturuldu**: En-US default değerlerle (EMPTY, MIN_LENGTH, vb. tüm key'ler).
  2. **Culture-specific resx'ler explicit `Include` ile eklendi**:
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
  3. **Neutral resx sadece Designer.cs generate ediyor** (`<Generator>ResXFileCodeGenerator</Generator>`), culture-specific'ler etmiyor.
  4. `<IncludeSatelliteAssembliesInPackage>false</IncludeSatelliteAssembliesInPackage>` geri eklendi (satellite'ler pack'e dahil edilmesin).
- **Sonuç**: 
  - `CoreGenerateSatelliteAssemblies` target çalıştı (1.7s).
  - `src/ArchiX.Library/bin/Release/net9.0/en-US/ArchiX.Library.resources.dll` üretildi (5 KB).
  - `src/ArchiX.Library/bin/Release/net9.0/tr-TR/ArchiX.Library.resources.dll` üretildi (5 KB).
  - **Lokal tüm testler geçti**: 347 + 77 = 424 test, 0 fail! ✅
- **Durum**: Commit/push bekleniyor (#15. deneme - kesin çözüm).
