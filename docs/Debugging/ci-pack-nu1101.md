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
