# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
### Quick decision rules (critical)

- 3 deneme sonrası teşhis + dokümantasyon (kritik):
  - Aynı issue/task için 3 deneme yapılıp kullanıcı hâlâ "olmadı" diyorsa, kör denemeyi durdur.
  - Frontend (Razor Pages / CSS / JS) için:
    - Mutlaka runtime incelemeye geç: `F12` (Elements/Console/Network), computed styles, box model, `getBoundingClientRect()`.
    - Gerekirse sayfa kaynağını doğrula: `Ctrl+U` (View Source) ile hangi layout/script/css gerçekten render edilmiş kontrol et.
  - Backend için:
    - Mutlaka runtime teşhise geç: log/exception/stack trace, minimal repro, gerekiyorsa küçük test/diagnostic kodu.
  - 3. denemeden sonra mutlaka `docs/Debugging/` altında bu issue/task için tek bir markdown günlük dosyası oluştur ya da mevcutsa devam ettir.
    - Format örneği: `docs/Debugging/tabhost-sidebar-seam.md`.
    - İçerik: kronolojik denemeler, değişiklik (dosya + selector/function), beklenen etki, gözlenen sonuç ("olmadı"), varsa runtime ölçümler ve nihai kök neden.

- Debug logging (required):
  - Create `docs/Debugging/` if missing.
  - Keep **one** markdown log per issue/task under `docs/Debugging/`.
  - When the user says **"olmadı"** for the same issue/task, append a short entry to that same log.
  - Entry format: timestamp (local), what changed (file + selector/function), expected effect, observed result ("olmadı").
  - Continue with the next attempt without asking the user.

- Katman seçimi:
  - `src/ArchiX.Library` = core (host bağımsız).
  - `src/ArchiX.Library.Web` = web-specific library (core üstüne).
  - `src/ArchiX.WebHost` = host/test harness (gerçek uygulama simülasyonu).

- `CopyToHost` kuralı:
  - Bir dosya `src/ArchiX.WebHost` altında görünüyorsa **önce** bunun `CopyToHost` ile kopyalanıp kopyalanmadığını kontrol et.
  - Kopyalanıyorsa: değişikliği WebHost kopyasında değil, kaynağında yap (çoğunlukla `src/ArchiX.Library.Web/wwwroot/...`; aynı kural diğer kopyalanan `Pages/Templates` içerikleri için de geçerli).
  - Kısa kontrol: `src/ArchiX.WebHost/ArchiX.WebHost.csproj` içindeki `CleanCopiedFiles` listesinde silinen klasörlerin altı (örn. `Pages`, `Templates`, `wwwroot/js`, `wwwroot/images`, `wwwroot/css/...`) build/clean ile yeniden üretilebilir; bu alanlarda kalıcı edit yapma.
