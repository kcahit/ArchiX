// Copied from `src/ArchiX.WebHost/wwwroot/js/archix/tabbed/archix-tabhost.js` and extended.
// Source of truth must be `src/ArchiX.Library.Web` because WebHost `wwwroot` is wiped during build.

(() => {
  const state = {
    tabs: [],
    activeId: null,
    seqByTitleBase: new Map(),
    lastActivityAt: Date.now(),
    detailById: new Map() // { id, url, title, openedAt, lastActivatedAt, isDirty, warnedAt }
  };

  const pinned = {
    homeUrl: '/Dashboard',
    homeTitle: 'Dashboard'
  };

  function getNavigationMode() {
    // Source-of-truth is UI/TabbedOptions (seeded in DB) which defaults to Tabbed.
    // Client can override by setting window.ArchiX.UiOptions.navigationMode.
    return window.ArchiX?.UiOptions?.navigationMode || 'Tabbed';
  }

  function isPinnedUrl(url) {
    if (!url) return false;
    return String(url).toLowerCase() === pinned.homeUrl.toLowerCase();
  }

  function isPinnedTabId(id) {
    const d = state.detailById.get(id);
    return !!d && isPinnedUrl(d.url);
  }

  function isGroupTabId(id) {
    const d = state.detailById.get(id);
    return !!d && !!d.isGroup;
  }

  function normalizeGroupKey(groupKey) {
    return String(groupKey || '').trim().replace(/^\/+|\/+$/g, '');
  }

  // IMPORTANT: group tab ids must be unique per full group path.
  // Using only the last segment (or a short key) causes id collisions across nesting levels,
  // which breaks 3+ level hierarchies.
  function groupTabDomIdForKey(groupKey) {
    const k = normalizeGroupKey(groupKey);
    return `g_${k.replaceAll('/', '__')}`;
  }

  function closeGroupTabAndChildren(groupId) {
    const h = ensureHost();
    if (!h) return;

    // Remove tab record
    const idx = state.tabs.findIndex(t => t.id === groupId);
    if (idx >= 0) state.tabs.splice(idx, 1);
    state.detailById.delete(groupId);

    // Remove tab element
    const link = qs(`.nav-link[data-tab-id="${CSS.escape(groupId)}"]`, h.tabs);
    const li = link?.closest('li');
    if (li) li.remove();

    // Remove pane (contains nested host and all children)
    const pane = qs(`.tab-pane[data-tab-id="${CSS.escape(groupId)}"]`, h.panes);
    if (pane) pane.remove();

    // Activate previous tab if any
    if (state.tabs.length > 0) {
      const next = state.tabs[Math.min(idx - 1, state.tabs.length - 1)];
      activateTab(next.id);
    } else {
      openTab({ url: pinned.homeUrl, title: pinned.homeTitle });
    }
  }

  function copyTextCompat(payload) {
    if (navigator.clipboard?.writeText) {
      return navigator.clipboard.writeText(payload);
    }

    return Promise.reject(new Error('Clipboard API is not available.'));
  }

  function getNavGroupTitleFromSidebar(groupKey) {
    const sel = `#sidebar a[data-bs-toggle="collapse"][data-archix-group="${CSS.escape(groupKey)}"]`;
    const el = qs(sel);
    if (!el) return null;
    const t = (el.textContent || '').trim();
    return t.length > 0 ? t : null;
  }

  const config = {
    maxOpenTabs: 15,
    maxTabReachedMessage: 'Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.',
    tabAutoCloseSeconds: 15,  // 15 saniye (test için, prod: 600 = 10 dakika)
    autoCloseWarningSeconds: 5,  // 5 saniye uyarı (test için, prod: 30)
    tabRequestTimeoutMs: 30000,
    enableNestedTabs: true
  };

  function createNestedHost(parentPane, groupId) {
    const host = document.createElement('div');
    host.className = 'mt-2';
    host.setAttribute('data-archix-nested-host', '1');
    host.setAttribute('data-group-id', groupId);
    host.innerHTML = `
      <ul class="nav nav-tabs" role="tablist" data-archix-nested-tabs="1"></ul>
      <div class="tab-content border border-top-0" data-archix-nested-panes="1"></div>
    `;
    parentPane.appendChild(host);
    return host;
  }

  function isInSidebar(el) {
    return !!(el && el.closest && el.closest('#sidebar'));
  }

  function getSidebarLinkTitle(a, href) {
    const t = (a?.textContent || '').trim();
    return t || a?.getAttribute('title') || href;
  }

  function getSidebarGroupChainFromDom(a) {
    const chain = [];
    if (!(a instanceof Element)) return chain;

    const collapses = [];
    let cur = a.parentElement;
    while (cur) {
      if (cur instanceof Element && cur.classList && cur.classList.contains('collapse')) {
        collapses.push(cur);
      }
      cur = cur.parentElement;
      if (cur && cur.id === 'sidebar') break;
    }

    collapses.reverse();

    for (const c of collapses) {
      const id = c.getAttribute('id');
      if (!id) continue;
      const header = document.querySelector(`#sidebar a[data-bs-toggle="collapse"][href="#${CSS.escape(id)}"]`);
      if (!header) continue;
      const g = header.getAttribute('data-archix-group');
      if (!g) continue;
      if (!chain.includes(g)) chain.push(g);
    }

    return chain;
  }

  function getGroupChainForLink(a) {
    const byDom = getSidebarGroupChainFromDom(a);
    if (byDom.length > 0) return byDom;

    const menuPath = a.getAttribute('data-archix-menu');
    if (!menuPath) return [];

    const parts = String(menuPath).split('/').filter(Boolean);
    const chain = [];

    for (let i = 0; i < parts.length - 1; i++) {
      chain.push(parts.slice(0, i + 1).join('/'));
    }

    return chain;
  }

  function ensureRootGroupTab(groupKey) {
    const h = ensureHost();
    if (!h) return null;

    const groupId = groupTabDomIdForKey(groupKey);
    if (state.detailById.get(groupId)) return groupId;

    const normalizedKey = normalizeGroupKey(groupKey);
    const title = getNavGroupTitleFromSidebar(normalizedKey) || normalizedKey;

    const tabLi = document.createElement('li');
    tabLi.className = 'nav-item';

    const tabA = document.createElement('a');
    tabA.className = 'nav-link';
    tabA.href = '#';
    tabA.setAttribute('role', 'tab');
    tabA.setAttribute('data-tab-id', groupId);
    tabA.innerHTML = `
      <span class="archix-tab-title">${escapeHtml(title)}</span>
      <button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-tab-close" aria-label="Kapat" title="Kapat">&times;</button>
    `;

    tabLi.appendChild(tabA);

    const pane = document.createElement('div');
    pane.className = 'tab-pane fade';
    pane.setAttribute('role', 'tabpanel');
    pane.setAttribute('data-tab-id', groupId);
    pane.innerHTML = `<div class="p-2" data-archix-group-pane="1"></div>`;

    h.tabs.appendChild(tabLi);
    h.panes.appendChild(pane);

    state.tabs.push({ id: groupId, url: null, title, isGroup: true, groupKey: normalizedKey });
    state.detailById.set(groupId, {
      id: groupId,
      url: null,
      title,
      openedAt: Date.now(),
      lastActivatedAt: Date.now(),
      isDirty: false,
      warnedAt: null,
      isGroup: true,
      groupKey: normalizedKey
    });

    const inner = pane.querySelector('[data-archix-group-pane="1"]');
    if (inner) createNestedHost(inner, groupId);

    return groupId;
  }

  function ensureNestedGroupTab(parentGroupId, groupKey) {
    const parentPaneHost = qs(`.tab-pane[data-tab-id="${CSS.escape(parentGroupId)}"] [data-archix-nested-host="1"]`);
    if (!parentPaneHost) return null;

    const nestedTabs = qs('[data-archix-nested-tabs="1"]', parentPaneHost);
    const nestedPanes = qs('[data-archix-nested-panes="1"]', parentPaneHost);
    if (!nestedTabs || !nestedPanes) return null;

    const groupId = groupTabDomIdForKey(groupKey);
    const existing = qs(`.nav-link[data-tab-id="${CSS.escape(groupId)}"]`, nestedTabs);
    if (existing) return groupId;

    const normalizedKey = normalizeGroupKey(groupKey);
    const title = getNavGroupTitleFromSidebar(normalizedKey) || normalizedKey;

    const li = document.createElement('li');
    li.className = 'nav-item';

    const a = document.createElement('a');
    a.className = 'nav-link';
    a.href = '#';
    a.setAttribute('role', 'tab');
    a.setAttribute('data-tab-id', groupId);
    a.innerHTML = `
      <span class="archix-tab-title">${escapeHtml(title)}</span>
      <button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-nested-close" aria-label="Kapat" title="Kapat">&times;</button>
    `;

    li.appendChild(a);
    nestedTabs.appendChild(li);

    const pane = document.createElement('div');
    pane.className = 'tab-pane fade';
    pane.setAttribute('role', 'tabpanel');
    pane.setAttribute('data-tab-id', groupId);
    pane.innerHTML = `<div class="p-2" data-archix-group-pane="1"></div>`;

    nestedPanes.appendChild(pane);

    const inner = pane.querySelector('[data-archix-group-pane="1"]');
    if (inner) createNestedHost(inner, groupId);

    // Track group in global state so deeper leaves can resolve the correct pane/host.
    if (!state.detailById.get(groupId)) {
      state.detailById.set(groupId, {
        id: groupId,
        url: null,
        title,
        openedAt: Date.now(),
        lastActivatedAt: Date.now(),
        isDirty: false,
        warnedAt: null,
        isGroup: true,
        groupKey: normalizedKey
      });
    }

    if (!nestedTabs.hasAttribute('data-archix-group-bound')) {
      nestedTabs.setAttribute('data-archix-group-bound', '1');
      nestedTabs.addEventListener('click', e => {
        const target = e.target;
        if (!(target instanceof Element)) return;
        const closeBtn = target.closest('.archix-nested-close');
        const link = target.closest('.nav-link[data-tab-id]');
        const id = (link && link.getAttribute('data-tab-id')) || null;
        if (!id) return;
        e.preventDefault();

        if (closeBtn) {
          const li2 = link?.closest('li');
          if (li2) li2.remove();
          const p2 = qs(`.tab-pane[data-tab-id="${CSS.escape(id)}"]`, nestedPanes);
          if (p2) p2.remove();
          return;
        }

        qsa('.nav-link[data-tab-id]', nestedTabs).forEach(a2 => {
          const isActive = a2.getAttribute('data-tab-id') === id;
          a2.classList.toggle('active', isActive);
          a2.setAttribute('aria-selected', isActive ? 'true' : 'false');
        });
        qsa('.tab-pane[data-tab-id]', nestedPanes).forEach(p2 => {
          const isActive = p2.getAttribute('data-tab-id') === id;
          p2.classList.toggle('show', isActive);
          p2.classList.toggle('active', isActive);
        });
      });
    }

    return groupId;
  }

  function activateNestedGroup(parentGroupId, groupId) {
    const parentPaneHost = qs(`.tab-pane[data-tab-id="${CSS.escape(parentGroupId)}"] [data-archix-nested-host="1"]`);
    if (!parentPaneHost) return;

    const nestedTabs = qs('[data-archix-nested-tabs="1"]', parentPaneHost);
    const nestedPanes = qs('[data-archix-nested-panes="1"]', parentPaneHost);
    if (!nestedTabs || !nestedPanes) return;

    qsa('.nav-link[data-tab-id]', nestedTabs).forEach(a => {
      const isActive = a.getAttribute('data-tab-id') === groupId;
      a.classList.toggle('active', isActive);
      a.setAttribute('aria-selected', isActive ? 'true' : 'false');
    });
    qsa('.tab-pane[data-tab-id]', nestedPanes).forEach(p => {
      const isActive = p.getAttribute('data-tab-id') === groupId;
      p.classList.toggle('show', isActive);
      p.classList.toggle('active', isActive);
    });
  }

  function openByGroupChain({ groups, url, title }) {
    const rootGroupKey = groups[0];
    const rootGroupId = ensureRootGroupTab(rootGroupKey);
    if (!rootGroupId) {
      openTab({ url, title });
      return;
    }

    activateTab(rootGroupId);

    let currentGroupId = rootGroupId;
    for (let i = 1; i < groups.length; i++) {
      const gk = groups[i];
      const childGroupId = ensureNestedGroupTab(currentGroupId, gk);
      if (!childGroupId) break;
      activateNestedGroup(currentGroupId, childGroupId);
      currentGroupId = childGroupId;
    }

    // Open leaf tab under the deepest group tab in the chain.
    openNestedTab({ groupId: currentGroupId, url, title });
  }

  function newId() {
    return 't_' + Math.random().toString(36).slice(2, 10) + Date.now().toString(36);
  }

  function normalizeTitleBase(title) {
    return (title || 'Tab').trim().replace(/\s+/g, ' ');
  }

  function nextUniqueTitle(titleBase) {
    const base = normalizeTitleBase(titleBase);
    const next = (state.seqByTitleBase.get(base) || 0) + 1;
    state.seqByTitleBase.set(base, next);
    if (next === 1) return base;
    return `${base}_${String(next - 1).padStart(3, '0')}`;
  }

  function openNestedTab({ groupId, url, title }) {
    const h = ensureHost();
    if (!h) return;

    // Stabilize sidebar state before nested tab DOM mutations.
    try {
      window.ArchiX?.Sidebar?.restoreState?.();
    } catch { }

    // Ensure group tab exists in root host.
    // IMPORTANT: only the first level group should be a root tab.
    // Nested groups must live inside their parent group's nested host.
    const group = state.detailById.get(groupId);
    if (!group) {
      // If we got here without the group being created via group-chain logic, fail-closed.
      openTab({ url, title });
      return;
    }

    // Do not force nested groups into root tabs.
    // Root activation is handled by `openByGroupChain`. Here we only render the leaf into the group's nested host.
    if (groupId === state.activeId) {
      // ok
    }

    // Resolve the exact pane node belonging to this groupId.
    // We search within the TabHost root only (not whole document), and prefer the *deepest* match.
    const candidates = qsa(`.tab-pane[data-tab-id="${CSS.escape(groupId)}"]`, h.host);
    const paneNode = candidates.length > 0 ? candidates[candidates.length - 1] : null;

    const groupPaneHost = paneNode ? qs('[data-archix-nested-host="1"]', paneNode) : null;
    if (!groupPaneHost) return; 

    const nestedTabs = qs('[data-archix-nested-tabs="1"]', groupPaneHost);
    const nestedPanes = qs('[data-archix-nested-panes="1"]', groupPaneHost);
    if (!nestedTabs || !nestedPanes) return; 

    const childId = newId();
    const childTitle = nextUniqueTitle(title);

    const tabLi = document.createElement('li');
    tabLi.className = 'nav-item';
    const tabA = document.createElement('a');
    tabA.className = 'nav-link';
    tabA.href = '#';
    tabA.setAttribute('role', 'tab');
    tabA.setAttribute('data-nested-tab-id', childId);
    tabA.innerHTML = `
      <span class="archix-tab-title">${escapeHtml(childTitle)}</span>
      <button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-nested-close" aria-label="Kapat" title="Kapat">&times;</button>
    `;
    tabLi.appendChild(tabA);

    const pane = document.createElement('div');
    pane.className = 'tab-pane fade';
    pane.setAttribute('role', 'tabpanel');
    pane.setAttribute('data-nested-tab-id', childId);
    pane.innerHTML = `<div class="p-3 text-muted">Yükleniyor...</div>`;

    nestedTabs.appendChild(tabLi);
    nestedPanes.appendChild(pane);

    const activateNested = (id) => {
      qsa('.nav-link[data-nested-tab-id]', nestedTabs).forEach(a => {
        const isActive = a.getAttribute('data-nested-tab-id') === id;
        a.classList.toggle('active', isActive);
        a.setAttribute('aria-selected', isActive ? 'true' : 'false');
      });
      qsa('.tab-pane[data-nested-tab-id]', nestedPanes).forEach(p => {
        const isActive = p.getAttribute('data-nested-tab-id') === id;
        p.classList.toggle('show', isActive);
        p.classList.toggle('active', isActive);
      });
    };

    if (!nestedTabs.hasAttribute('data-archix-bound')) {
      nestedTabs.setAttribute('data-archix-bound', '1');
      nestedTabs.addEventListener('click', e => {
        const target = e.target;
        if (!(target instanceof Element)) return;
        const closeBtn = target.closest('.archix-nested-close');
        const link = target.closest('.nav-link[data-nested-tab-id]');
        const id = (link && link.getAttribute('data-nested-tab-id')) || null;
        if (!id) return;
        e.preventDefault();

        // Ensure sidebar state doesn't change as a side effect of opening a tab.
        try {
          window.ArchiX?.Sidebar?.restoreState?.();
        } catch { }

        if (closeBtn) {
          const li = link?.closest('li');
          if (li) li.remove();
          const p = qs(`.tab-pane[data-nested-tab-id="${CSS.escape(id)}"]`, nestedPanes);
          if (p) p.remove();
          const remaining = qsa('.nav-link[data-nested-tab-id]', nestedTabs);
          if (remaining.length > 0) {
            activateNested(remaining[remaining.length - 1].getAttribute('data-nested-tab-id'));
          } else {
            closeTab(groupId);
          }
          return;
        }

        activateNested(id);
      });
    }

    activateNested(childId);

    loadContent(url).then(async result => {
      if (!result.ok) {
        pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">Yükleme hatası (${result.status}).</div></div>`;
        return;
      }
      const html = await result.text;
      let content = html;
      const m = /<body[^>]*>([\s\S]*?)<\/body>/i.exec(html);
      if (m && m[1]) content = m[1];

      // Keep nested tab content consistent with root tabs: extract only the main body, not the full layout.
      try {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');

        const tabMain = doc.querySelector('#tab-main');
        if (tabMain) {
          content = tabMain.innerHTML;
          if (window.ArchiX?.Debug) {
            console.log('[ArchiX Debug] Nested extract from #tab-main', { url, htmlLength: content.length });
          }
        } else {
          const workArea = doc.querySelector('.archix-work-area');
          if (workArea) {
            content = workArea.innerHTML;
            if (window.ArchiX?.Debug) {
              console.log('[ArchiX Debug] Nested extract from .archix-work-area', { url, htmlLength: content.length });
            }
          } else {
            const shellMain = doc.querySelector('main.archix-shell-main') ?? doc.querySelector('main[role="main"]');
            if (shellMain) {
              const clone = shellMain.cloneNode(true);
              if (clone instanceof Element) {
                clone.querySelector('#archix-tabhost')?.remove();
                clone.querySelector('#sidebar')?.remove();
                clone.querySelector('nav.navbar')?.remove();
                clone.querySelector('footer')?.remove();
                content = clone.innerHTML;
                if (window.ArchiX?.Debug) {
                  console.log('[ArchiX Debug] Nested extract from main (duplicate removed)', { url, htmlLength: content.length });
                }
              }
            }
          }
        }
      } catch { }

      pane.innerHTML = `<div class="archix-tab-content">${content}</div>`;

      // Re-execute inline scripts (nested tab) with Chart.js cleanup
      try {
        const canvases = pane.querySelectorAll('canvas');
        canvases.forEach(canvas => {
          const chartId = canvas.getAttribute('id');
          if (chartId && window.Chart && window.Chart.getChart) {
            const existing = window.Chart.getChart(chartId);
            if (existing) {
              existing.destroy();
              if (window.ArchiX?.Debug) {
                console.log('[ArchiX Debug] Nested: Destroyed existing chart:', chartId);
              }
            }
          }
        });
      } catch (err) {
        if (window.ArchiX?.Debug) {
          console.error('[ArchiX Debug] Nested chart destroy error:', err);
        }
      }

      try {
        const scripts = pane.querySelectorAll('script');
        scripts.forEach(oldScript => {
          const newScript = document.createElement('script');
          if (oldScript.src) {
            newScript.src = oldScript.src;
          } else {
            newScript.textContent = oldScript.textContent;
          }
          oldScript.parentNode.replaceChild(newScript, oldScript);
        });
      } catch (err) {
        if (window.ArchiX?.Debug) {
          console.error('[ArchiX Debug] Nested script re-execute error:', err);
        }
      }

      try { window.ArchiX?.Sidebar?.restoreState?.(); } catch { }

      bindDirtyTrackingForPane(childId, pane);
    });
  }

  const selectors = {
    host: '#archix-tabhost',
    tabs: '#archix-tabhost-tabs',
    panes: '#archix-tabhost-panes',
    toast: '#toastContainer'
  };

  function qs(sel, root = document) { return root.querySelector(sel); }
  function qsa(sel, root = document) { return Array.from(root.querySelectorAll(sel)); }

  function bindResponseCardActions() {
    document.addEventListener('click', e => {
      const t = e.target;
      if (!(t instanceof Element)) return;
      const btn = t.closest('[data-archix-action]');
      if (!btn) return;

      const action = btn.getAttribute('data-archix-action');
      if (!action) return;

      if (action === 'close-tab') {
        e.preventDefault();
        if (state.activeId) closeTab(state.activeId);
        return;
      }

      if (action === 'copy-trace') {
        e.preventDefault();
        const trace = btn.getAttribute('data-archix-trace') || '';
        const msg = btn.getAttribute('data-archix-message') || '';
        const payload = `TraceId: ${trace}\nMesaj: ${msg}`;

        copyTextCompat(payload).then(
          () => showToast('Kopyalandı.'),
          () => showToast('Kopyalama desteklenmiyor. Tarayıcı izinlerini kontrol edin.')
        );
      }
    });
  }

  function showAutoClosePrompt(tabId) {
    const d = state.detailById.get(tabId);
    if (!d) return;

    const c = qs(selectors.toast);
    if (!c) return;

    // Prevent duplicate toasts for the same tab
    const existing = c.querySelector(`.toast[data-tab-id="${CSS.escape(tabId)}"]`);
    if (existing) return;

    const el = document.createElement('div');
    el.className = 'toast text-bg-warning border-0';
    el.setAttribute('role', 'alert');
    el.setAttribute('aria-live', 'assertive');
    el.setAttribute('aria-atomic', 'true');
    el.setAttribute('data-archix-autoclose', '1');
    el.setAttribute('data-tab-id', tabId);

    const hasDirty = !!d.isDirty;
    el.innerHTML = `
      <div class="toast-header text-bg-warning border-0">
        <strong class="me-auto">Otomatik Kapatma</strong>
        <small>Tab: ${escapeHtml(d.title)}</small>
        <button type="button" class="btn-close ms-2" data-bs-dismiss="toast" aria-label="Close"></button>
      </div>
      <div class="toast-body">
        <div class="mb-2">"${escapeHtml(d.title)}" sekmesi kapatılacak.</div>
        <div class="mb-2 d-flex align-items-center gap-2">
          <label class="form-label m-0" for="archixDeferSeconds_${escapeHtml(tabId)}">Erteleme (sn)</label>
          <input class="form-control form-control-sm" style="width:90px" type="number" min="1" max="${config.tabAutoCloseSeconds}" value="${config.tabAutoCloseSeconds}" id="archixDeferSeconds_${escapeHtml(tabId)}" />
        </div>
        <div class="d-flex gap-2 flex-wrap">
          <button type="button" class="btn btn-sm btn-light" data-action="defer">Kapatmayı Ertele</button>
          ${hasDirty ? '<button type="button" class="btn btn-sm btn-danger" data-action="closeNoSave">Kaydetmeden Kapat</button>' : ''}
          <button type="button" class="btn btn-sm btn-primary" data-action="focus">Sayfayı Aç</button>
        </div>
      </div>`;

    c.appendChild(el);

    const doRemove = () => el.remove();
    el.addEventListener('hidden.bs.toast', doRemove);

    // Auto-close tab after warning timeout if user doesn't interact
    const autoCloseTimer = setTimeout(() => {
      closeTab(tabId);
      if (window.bootstrap?.Toast) {
        const toast = window.bootstrap.Toast.getOrCreateInstance(el);
        toast.hide();
      } else {
        doRemove();
      }
    }, config.autoCloseWarningSeconds * 1000);

    el.addEventListener('click', e => {
      const t = e.target;
      if (!(t instanceof Element)) return;
      const btn = t.closest('button[data-action]');
      if (!btn) return;

      // Cancel auto-close timer when user interacts
      clearTimeout(autoCloseTimer);

      const action = btn.getAttribute('data-action');
      const input = el.querySelector('input[type="number"]');
      let seconds = config.tabAutoCloseSeconds;
      if (input) {
        const v = Number.parseInt(input.value, 10);
        if (!Number.isNaN(v)) seconds = v;
      }
      if (seconds < 1) seconds = 1;
      if (seconds > config.tabAutoCloseSeconds) seconds = config.tabAutoCloseSeconds;

      if (action === 'defer') {
        state.lastActivityAt = Date.now() - Math.max(0, (config.tabAutoCloseSeconds - seconds)) * 1000;
      }

      if (action === 'focus') {
        activateTab(tabId);
      }

      if (action === 'closeNoSave') {
        closeTab(tabId);
      }

      if (window.bootstrap?.Toast) {
        const toast = window.bootstrap.Toast.getOrCreateInstance(el);
        toast.hide();
      } else {
        doRemove();
      }
    });

    if (window.bootstrap?.Toast) {
      const toast = new window.bootstrap.Toast(el, { autohide: false });
      toast.show();
    }
  }

  function showToast(message) {
    const c = qs(selectors.toast);
    if (!c) return;
    const el = document.createElement('div');
    el.className = 'toast align-items-center text-bg-warning border-0';
    el.setAttribute('role', 'alert');
    el.setAttribute('aria-live', 'assertive');
    el.setAttribute('aria-atomic', 'true');
    el.innerHTML = `
      <div class="d-flex">
        <div class="toast-body">${escapeHtml(message)}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
      </div>`;
    c.appendChild(el);

    if (window.bootstrap?.Toast) {
      const t = new window.bootstrap.Toast(el, { delay: 4000 });
      el.addEventListener('hidden.bs.toast', () => el.remove());
      t.show();
    } else {
      setTimeout(() => el.remove(), 4000);
    }
  }

  function escapeHtml(s) {
    return String(s)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  async function loadContent(url) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), config.tabRequestTimeoutMs);

    try {
      const res = await fetch(url, {
        method: 'GET',
        headers: {
          'X-ArchiX-Tab': '1',
          'X-Requested-With': 'XMLHttpRequest',
          'Accept': 'text/html'
        },
        credentials: 'same-origin',
        signal: controller.signal
      });

      clearTimeout(timeoutId);

      return {
        ok: res.ok,
        status: res.status,
        text: await res.text()
      };
    } catch (err) {
      clearTimeout(timeoutId);
      if (err.name === 'AbortError') {
        return {
          ok: false,
          status: 408,
          text: `<div class="alert alert-warning">İstek zaman aşımına uğradı (${config.tabRequestTimeoutMs / 1000} saniye).</div>`
        };
      }
      throw err;
    }
  }

  function ensureHost() {
    const host = qs(selectors.host);
    if (!host) return null;
    const tabs = qs(selectors.tabs, host);
    const panes = qs(selectors.panes, host);
    if (!tabs || !panes) return null;
    return { host, tabs, panes };
  }

  function activateTab(id) {
    const h = ensureHost();
    if (!h) return;

    state.activeId = id;
    const d = state.detailById.get(id);
    if (d) {
      d.lastActivatedAt = Date.now();
      d.warnedAt = null;  // Clear warning when tab is activated
    }

    qsa('.nav-link[data-tab-id]', h.tabs).forEach(a => {
      const isActive = a.getAttribute('data-tab-id') === id;
      a.classList.toggle('active', isActive);
      a.setAttribute('aria-selected', isActive ? 'true' : 'false');
    });

    qsa('.tab-pane[data-tab-id]', h.panes).forEach(p => {
      const isActive = p.getAttribute('data-tab-id') === id;
      p.classList.toggle('show', isActive);
      p.classList.toggle('active', isActive);
    });
  }

  async function openTab({ url, title }) {
    const h = ensureHost();
    if (!h) return;

    try {
      window.ArchiX?.Sidebar?.restoreState?.();
    } catch { }

    if (state.tabs.length >= config.maxOpenTabs) {
      showToast(config.maxTabReachedMessage);
      return;
    }

    const id = newId();
    const isPinned = isPinnedUrl(url);
    const uniqueTitle = isPinned ? pinned.homeTitle : nextUniqueTitle(title);

    const tabLi = document.createElement('li');
    tabLi.className = 'nav-item';

    const tabA = document.createElement('a');
    tabA.className = 'nav-link';
    tabA.href = '#';
    tabA.setAttribute('role', 'tab');
    tabA.setAttribute('data-tab-id', id);
    tabA.innerHTML = `
      <span class="archix-tab-title">${escapeHtml(uniqueTitle)}</span>
      ${isPinned ? '' : '<button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-tab-close" aria-label="Kapat" title="Kapat">&times;</button>'}
    `;

    tabLi.appendChild(tabA);

    const pane = document.createElement('div');
    pane.className = 'tab-pane fade';
    pane.setAttribute('role', 'tabpanel');
    pane.setAttribute('data-tab-id', id);
    pane.innerHTML = `<div class="p-3 text-muted">Yükleniyor...</div>`;

    h.tabs.appendChild(tabLi);
    h.panes.appendChild(pane);

    state.tabs.push({ id, url, title: uniqueTitle });
    state.detailById.set(id, {
      id,
      url,
      title: uniqueTitle,
      openedAt: Date.now(),
      lastActivatedAt: Date.now(),
      isDirty: false,
      warnedAt: null,
      isPinned
    });

    activateTab(id);

    try {
      const result = await loadContent(url);
      if (!result.ok) {
        pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">Yükleme hatası (${result.status}).</div></div>`;
        return;
      }

      const html = result.text;
      let content = html;
      const m = /<body[^>]*>([\s\S]*?)<\/body>/i.exec(html);
      if (m && m[1]) content = m[1];

      // If the fetched page is a full shell/layout, avoid injecting the whole layout into the tab.
      // Prefer extracting only the page main content (works for Dashboard, Admin, Definitions, etc.).
      try {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');

        // 1) Preferred: explicit host container
        const tabMain = doc.querySelector('#tab-main');
        if (tabMain) {
          content = tabMain.innerHTML;
          if (window.ArchiX?.Debug) {
            console.log('[ArchiX Debug] Extract from #tab-main', { url, htmlLength: content.length });
          }
        } else {
          // 2) Fallback: template work area
          const workArea = doc.querySelector('.archix-work-area');
          if (workArea) {
            content = workArea.innerHTML;
            if (window.ArchiX?.Debug) {
              console.log('[ArchiX Debug] Extract from .archix-work-area', { url, htmlLength: content.length });
            }
          } else {
          // 3) Fallback: use the main shell content, but strip obvious duplicates
          const shellMain = doc.querySelector('main.archix-shell-main') ?? doc.querySelector('main[role="main"]');
          if (shellMain) {
            const clone = shellMain.cloneNode(true);
            if (clone instanceof Element) {
              clone.querySelector('#archix-tabhost')?.remove();
              clone.querySelector('#sidebar')?.remove();
              clone.querySelector('nav.navbar')?.remove();
              clone.querySelector('footer')?.remove();
              content = clone.innerHTML;
              if (window.ArchiX?.Debug) {
                console.log('[ArchiX Debug] Extract from main (duplicate removed)', { url, htmlLength: content.length });
              }
            }
          }
          }
        }
      } catch { }

      pane.innerHTML = `<div class="archix-tab-content">${content}</div>`;

      // Re-execute inline scripts (e.g., Chart.js init in Dashboard)
      // IMPORTANT: Destroy existing Chart.js instances before re-creating
      try {
        const canvases = pane.querySelectorAll('canvas');
        canvases.forEach(canvas => {
          const chartId = canvas.getAttribute('id');
          if (chartId && window.Chart && window.Chart.getChart) {
            const existing = window.Chart.getChart(chartId);
            if (existing) {
              existing.destroy();
              if (window.ArchiX?.Debug) {
                console.log('[ArchiX Debug] Destroyed existing chart:', chartId);
              }
            }
          }
        });
      } catch (err) {
        if (window.ArchiX?.Debug) {
          console.error('[ArchiX Debug] Chart destroy error:', err);
        }
      }

      try {
        const scripts = pane.querySelectorAll('script');
        scripts.forEach(oldScript => {
          const newScript = document.createElement('script');
          if (oldScript.src) {
            newScript.src = oldScript.src;
          } else {
            newScript.textContent = oldScript.textContent;
          }
          oldScript.parentNode.replaceChild(newScript, oldScript);
        });
      } catch (err) {
        if (window.ArchiX?.Debug) {
          console.error('[ArchiX Debug] Script re-execute error:', err);
        }
      }

      try {
        window.ArchiX?.Sidebar?.restoreState?.();
      } catch { }

      bindDirtyTrackingForPane(id, pane);
    } catch {
      pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">İçerik yüklenemedi.</div></div>`;
    }
  }

  function closeTab(id) {
    const h = ensureHost();
    if (!h) return;

    if (isPinnedTabId(id)) return;

    if (isGroupTabId(id)) {
      closeGroupTabAndChildren(id);
      return;
    }

    const idx = state.tabs.findIndex(t => t.id === id);
    if (idx < 0) return;

    state.tabs.splice(idx, 1);
    state.detailById.delete(id);

    const link = qs(`.nav-link[data-tab-id="${CSS.escape(id)}"]`, h.tabs);
    const li = link?.closest('li');
    if (li) li.remove();

    const pane = qs(`.tab-pane[data-tab-id="${CSS.escape(id)}"]`, h.panes);
    if (pane) pane.remove();

    if (state.tabs.length > 0) {
      const next = state.tabs[Math.min(idx - 1, state.tabs.length - 1)];
      activateTab(next.id);
    } else {
      openTab({ url: pinned.homeUrl, title: pinned.homeTitle });
    }
  }

  function interceptClicks() {
    document.addEventListener('click', e => {
      const a = e.target instanceof Element ? e.target.closest('a') : null;
      if (!a) return;

      // ignore non-left click / modifier
      if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

      const href = a.getAttribute('href');
      if (!href) return;

      // ignore bootstrap collapse toggles
      if (a.hasAttribute('data-bs-toggle')) return;

      // ignore external
      if (/^https?:\/\//i.test(href)) return;

      // ignore anchors
      if (href.startsWith('#')) return;

      // ignore explicit new tab
      if (a.getAttribute('target') === '_blank') return;

      // only intercept app links
      if (!href.startsWith('/')) return;

      if (getNavigationMode() !== 'Tabbed') return;

      e.preventDefault();

      const title = getSidebarLinkTitle(a, href);

      // Sidebar: build nested hierarchy based on sidebar DOM (or data-archix-menu fallback)
      if (config.enableNestedTabs && isInSidebar(a)) {
        const groups = getGroupChainForLink(a);
        if (groups.length > 0) {
          openByGroupChain({ groups, url: href, title });
          return;
        }
      }

      openTab({ url: href, title });
    });
  }

  function touchActivity() {
    state.lastActivityAt = Date.now();
  }

  function initIdleTracking() {
    const opts = { passive: true, capture: true };
    window.addEventListener('pointerdown', touchActivity, opts);
    window.addEventListener('pointermove', touchActivity, opts);
    window.addEventListener('keydown', touchActivity, opts);
    window.addEventListener('wheel', touchActivity, opts);
    window.addEventListener('scroll', touchActivity, opts);
  }

  function getInactiveTabs() {
    return state.tabs.filter(t => t.id !== state.activeId);
  }

  function getInactiveIdleMs() {
    return Date.now() - state.lastActivityAt;
  }

  function tickAutoCloseWarnings() {
    const idleMs = getInactiveIdleMs();
    const warnMs = config.tabAutoCloseSeconds * 1000 - config.autoCloseWarningSeconds * 1000;
    
    // DEBUG
    console.log('[TabHost Debug] Tick:', {
      idleMs,
      warnMs,
      inactiveTabs: getInactiveTabs().length,
      activeId: state.activeId,
      navigationMode: getNavigationMode()
    });
    
    if (idleMs < warnMs) return;

    const now = Date.now();
    for (const t of getInactiveTabs()) {
      const d = state.detailById.get(t.id);
      if (!d) continue;
      console.log('[TabHost Debug] Showing warning for tab:', t.id, t.title);
      d.warnedAt = now;
      showAutoClosePrompt(t.id);
    }
  }

  function bindDirtyTrackingForPane(tabId, pane) {
    const d = state.detailById.get(tabId);
    if (!d) return;
    if (!/\/Tools\/Dataset\/Record/i.test(d.url)) return;

    const markDirty = () => {
      const dd = state.detailById.get(tabId);
      if (dd) dd.isDirty = true;
    };

    pane.addEventListener('input', e => {
      const t = e.target;
      if (!(t instanceof Element)) return;
      if (t.matches('input,select,textarea')) markDirty();
    }, true);

    pane.addEventListener('change', e => {
      const t = e.target;
      if (!(t instanceof Element)) return;
      if (t.matches('input,select,textarea')) markDirty();
    }, true);
  }

  function bindTabHostEvents() {
    const h = ensureHost();
    if (!h) return;

    h.tabs.addEventListener('click', e => {
      const target = e.target;
      if (!(target instanceof Element)) return;
      const closeBtn = target.closest('.archix-tab-close');
      const link = target.closest('.nav-link[data-tab-id]');
      const id = (link && link.getAttribute('data-tab-id')) || null;
      if (!id) return;

      if (closeBtn) {
        e.preventDefault();
        closeTab(id);
        return;
      }

      if (link) {
        e.preventDefault();
        activateTab(id);
      }
    });
  }

  function init() {
    const h = ensureHost();
    if (!h) return;

    // Only take over the page when Tabbed navigation is explicitly enabled.
    if (getNavigationMode() !== 'Tabbed') return;

    interceptClicks();
    bindTabHostEvents();
    bindResponseCardActions();
    initIdleTracking();

    window.setInterval(tickAutoCloseWarnings, 1000);

    // IMPORTANT: For the initial Dashboard tab, use the statically rendered #tab-main content
    // instead of fetching it again. This avoids duplicate requests and preserves the initial page state.
    const root = h.host.parentElement;
    const tabMain = root ? root.querySelector('#tab-main') : null;
    
    if (tabMain && tabMain.innerHTML.trim().length > 0) {
      // Move static content to the first tab (Dashboard)
      const id = newId();
      const uniqueTitle = pinned.homeTitle;

      const tabLi = document.createElement('li');
      tabLi.className = 'nav-item';

      const tabA = document.createElement('a');
      tabA.className = 'nav-link';
      tabA.href = '#';
      tabA.setAttribute('role', 'tab');
      tabA.setAttribute('data-tab-id', id);
      tabA.innerHTML = `<span class="archix-tab-title">${escapeHtml(uniqueTitle)}</span>`;
      tabLi.appendChild(tabA);

      const pane = document.createElement('div');
      pane.className = 'tab-pane fade';
      pane.setAttribute('role', 'tabpanel');
      pane.setAttribute('data-tab-id', id);
      pane.innerHTML = `<div class="archix-tab-content">${tabMain.innerHTML}</div>`;

      h.tabs.appendChild(tabLi);
      h.panes.appendChild(pane);

      state.tabs.push({ id, url: pinned.homeUrl, title: uniqueTitle });
      state.detailById.set(id, {
        id,
        url: pinned.homeUrl,
        title: uniqueTitle,
        openedAt: Date.now(),
        lastActivatedAt: Date.now(),
        isDirty: false,
        warnedAt: null,
        isPinned: true
      });

      activateTab(id);

      // Hide the static container after moving content
      tabMain.style.display = 'none';

      // Call Dashboard chart init function if available
      try {
        if (typeof window.initDashboardCharts === 'function') {
          window.initDashboardCharts();
          if (window.ArchiX?.Debug) {
            console.log('[ArchiX Debug] Dashboard charts initialized via window.initDashboardCharts()');
          }
        }
      } catch (err) {
        if (window.ArchiX?.Debug) {
          console.error('[ArchiX Debug] Dashboard chart init error:', err);
        }
      }

      bindDirtyTrackingForPane(id, pane);
    } else {
      // Fallback: fetch Dashboard if static content is missing
      if (tabMain) tabMain.style.display = 'none';
      openTab({ url: pinned.homeUrl, title: pinned.homeTitle });
    }
  }

  window.ArchiX = window.ArchiX || {};
  window.ArchiX.TabHost = {
    init,
    openTab
  };

  document.addEventListener('DOMContentLoaded', init);
})();
