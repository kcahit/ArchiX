# ArchiX Packaging Guide

## Paketler
- `ArchiX.Library` (core, DbContext + servisler)
- `ArchiX.Library.Web` (RCL, static web assets)

## Pack Komutları (Debug veya Release)
```bash
dotnet pack src/ArchiX.Library/ArchiX.Library.csproj -c Debug -o ./.nuget/local
dotnet pack src/ArchiX.Library.Web/ArchiX.Library.Web.csproj -c Debug -o ./.nuget/local
```
> Uyarı: OpenTelemetry EF/Prometheus bağımlılıkları prerelease. Stable paket istiyorsan önce bu bağımlılıkları stable sürüme çek veya kendi paket versiyonunu `-beta` olarak işaretle.

## Local Feed Kullanımı
- Repo kökünde `nuget.config` var, `./.nuget/local` kaynağını ekler.
- Restore: `dotnet restore` (otomatik local + nuget.org).

## GitHub Packages (öneri)
- Workflow (tasarlanacak): build → test → pack (Release) → push `ArchiX.*.nupkg` → `https://nuget.pkg.github.com/<owner>/index.json`.
- Kimlik: PAT veya `GITHUB_TOKEN` (packages:write scope).

## Versiyonlama
- SemVer: MAJOR.MINOR.PATCH.
- Prerelease örnekleri: `1.0.0-beta.1` (mevcut prerelease bağımlılıklar için tercih edilir).

## Push Örnekleri
```bash
# local feed
dotnet nuget push .nuget/local/ArchiX.Library.1.0.0.nupkg -s ./.nuget/local
dotnet nuget push .nuget/local/ArchiX.Library.Web.1.0.0.nupkg -s ./.nuget/local

# github packages (örnek, tasarlanacak)
dotnet nuget push .nuget/local/ArchiX.Library.1.0.0.nupkg -s https://nuget.pkg.github.com/<owner>/index.json -k <TOKEN>
```
