# TabHost content alignment (center -> top-left)

Issue: Tab alt alanı (tab pane content) ortalanıyor; sol-üste yakın olmalı.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> `#archix-tabhost-panes .archix-tab-content` için `width: 100%`, `max-width: none`, `margin: 0`, `padding: 0.75rem`.
- Expected: Tab içerik konteyneri full-width olup sol-üste hizalansın.
- Observed: olmadı.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> `#archix-tabhost-panes .archix-tab-content .grid-page > .container.grid-shell` için `.container` centering (max-width/margins) override.
- Expected: Dataset grid içeriği TabHost içinde full-width olup sol-üste yaklaşsın.
- Observed: olmadı.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> `#archix-tabhost-panes.tab-content { display: block }` (flex centering kırma) + tab içine inject edilen `nav/navbar`, `main.archix-shell-main`, `footer` bloklarını gizleme (double layout savunması).
- Expected: Tab content flex merkezlemeye takılmadan sol-üst akışta kalsın; varsa duplicate layout görünmesin.
- Observed: Sol üste yaklaştı ancak içerik kayboldu / görünmez oldu.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> Tab içindeki layout gizleme (nav/main/footer) kuralı geri alındı.
- Expected: Tab içerik alanı tekrar görünür olsun.
- Observed: (bekleniyor).

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` -> `openTab()` load sonrası HTML parse edilip `.archix-work-area` içeriği extract edilerek sadece gerçek sayfa gövdesi tab içine basıldı.
- Expected: `/Dashboard` gibi tam layout döndüren sayfalarda iç içe layout oluşmasın; tab içeriği doğru hizalansın ve görünür kalsın.
- Observed: (bekleniyor).

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` -> extract mantığı genelleştirildi: `.archix-work-area` yoksa `main.archix-shell-main` içi alınıp `#archix-tabhost/#sidebar/nav/footer` gibi duplicate bloklar çıkarılıyor.
- Expected: Admin/Definitions gibi farklı layout varyantlarında da tab içine sadece gerçek içerik basılsın.
- Observed: (bekleniyor).

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml` -> `#tab-main` eklendi (work-area id) ve `px-2` padding verildi.
- Change: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` -> içerik extract önceliği `#tab-main` (sonra `.archix-work-area`, sonra `main` fallback).
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> tabhost CSS sadeleştirildi, tek `.archix-tab-content` root (padding/overflow) bırakıldı.
- Expected: Tüm sayfalar tek ortak tab gövdesi altında aynı hizalama/scroll davranışıyla çalışsın; ortalanma kalksın.
- Observed: olmadı.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml` -> TabHost fetch istekleri (`X-ArchiX-Tab: 1`) için layout sadeleştirildi: navbar/sidebar/tabhost/scripts/footer render edilmez, sadece `#tab-main` + `RenderBody()` döner.
- Expected: `/Dashboard` ve diğer tab içi sayfalarda nested layout (nav/main/footer tekrar) tamamen bitsin; tab gövdesi tek bir div (tab-main) tarafından yönetilsin.
- Observed: (bekleniyor).

## 2026-01-20 00:00 (local)
- Change: (no code) runtime feedback toplandı.
- Expected: Tab içi sayfalarda `.archix-tab-content` içinde layout tekrarları bitmiş olmalı.
- Observed: olmadı; kullanıcı içeriklerin biraz yukarı çıktığını söylüyor ama emin değil. DevTools Issues: BrowserLink deprecated unload listener uyarısı, aynı form içinde duplicate id uyarıları, 'No label associated with a form field', 'Quirks Mode' uyarısı.

## 2026-01-20 00:00 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` -> Tabbed modda statik render hedefini gizleme selector'ı `.archix-work-area` yerine `#tab-main` yapıldı.
- Expected: TabHost init sırasında yanlışlıkla fazla alanın gizlenmesi engellensin; tab içi içerik/hizalama daha deterministik olsun.
- Observed: (bekleniyor).

## 2025-01-22 11:45 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/css/tabhost.css` -> `#archix-tabhost-panes .archix-tab-content .container` için Bootstrap centering override (`max-width: none`, `margin: 0`, `width: 100%`).
- Expected: Definitions/Application gibi `.container` wrapper'lı sayfalar da Dashboard gibi sol-üste hizalansın; tüm tab içerikleri ortak yapıya uysun.
- Observed: (test bekleniyor - build + refresh).

## 2025-01-22 11:50 (local)
- Change: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` -> `showAutoClosePrompt()` içinde duplicate toast kontrolü eklendi (aynı tabId için mevcut toast varsa yeni toast açılmıyor).
- Expected: "Otomatik Kapatma" mesajı her tab için tek seferde görünsün; 10+ duplicate toast sorunu bitsin.
- Observed: (test bekleniyor - build + refresh).
