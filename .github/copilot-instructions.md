# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
### Quick decision rules (critical)

- Katman seçimi:
  - `src/ArchiX.Library` = core (host bağımsız).
  - `src/ArchiX.Library.Web` = web-specific library (core üstüne).
  - `src/ArchiX.WebHost` = host/test harness (gerçek uygulama simülasyonu).

- `CopyToHost` kuralı:
  - Bir dosya `src/ArchiX.WebHost` altında görünüyorsa **önce** bunun `CopyToHost` ile kopyalanıp kopyalanmadığını kontrol et.
  - Kopyalanıyorsa: değişikliği WebHost kopyasında değil, kaynağında yap (çoğunlukla `src/ArchiX.Library.Web/wwwroot/...`; aynı kural diğer kopyalanan `Pages/Templates` içerikleri için de geçerli).
  - Kısa kontrol: `src/ArchiX.WebHost/ArchiX.WebHost.csproj` içindeki `CleanCopiedFiles` listesinde silinen klasörlerin altı (örn. `Pages`, `Templates`, `wwwroot/js`, `wwwroot/images`, `wwwroot/css/...`) build/clean ile yeniden üretilebilir; bu alanlarda kalıcı edit yapma.
