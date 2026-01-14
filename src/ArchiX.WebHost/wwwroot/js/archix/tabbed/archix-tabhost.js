(() => {
  const state = {
    tabs: [],
    activeId: null,
    seqByTitleBase: new Map(),
    lastActivityAt: Date.now(),
    detailById: new Map() // { id, url, title, openedAt, lastActivatedAt, isDirty, warnedAt }
  };

  const config = {
    maxOpenTabs: 15,
    maxTabReachedMessage: 'Açýk tab sayýsý 15 limitine geldi. Lütfen açýk tablardan birini kapatýnýz.',
    tabAutoCloseMinutes: 10,
    autoCloseWarningSeconds: 30,
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

  function openNestedTab({ groupId, url, title }) {
    const h = ensureHost();
    if (!h) return;

    // Ensure group tab exists in root host
    let group = state.detailById.get(groupId);
    if (!group) {
      // Create group as a normal tab first
      const fakeTitle = title;
      // Create empty group tab
      const id = groupId;
      const tabLi = document.createElement('li');
      tabLi.className = 'nav-item';

      const tabA = document.createElement('a');
      tabA.className = 'nav-link';
      tabA.href = '#';
      tabA.setAttribute('role', 'tab');
      tabA.setAttribute('data-tab-id', id);
      tabA.innerHTML = `
        <span class="archix-tab-title">${escapeHtml(fakeTitle)}</span>
        <button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-tab-close" aria-label="Kapat" title="Kapat">&times;</button>
      `;

      tabLi.appendChild(tabA);

      const pane = document.createElement('div');
      pane.className = 'tab-pane fade';
      pane.setAttribute('role', 'tabpanel');
      pane.setAttribute('data-tab-id', id);
      pane.innerHTML = `<div class="p-2" data-archix-group-pane="1"></div>`;

      h.tabs.appendChild(tabLi);
      h.panes.appendChild(pane);

      state.tabs.push({ id, url: null, title: fakeTitle, isGroup: true });
      state.detailById.set(id, {
        id,
        url: null,
        title: fakeTitle,
        openedAt: Date.now(),
        lastActivatedAt: Date.now(),
        isDirty: false,
        warnedAt: null,
        isGroup: true
      });

      const inner = pane.querySelector('[data-archix-group-pane="1"]');
      if (inner) createNestedHost(inner, id);

      activateTab(id);
    } else {
      activateTab(groupId);
    }

    const groupPane = qs(`.tab-pane[data-tab-id="${CSS.escape(groupId)}"] [data-archix-nested-host="1"]`);
    if (!groupPane) {
      // fail-closed
      openTab({ url, title });
      return;
    }

    const nestedTabs = qs('[data-archix-nested-tabs="1"]', groupPane);
    const nestedPanes = qs('[data-archix-nested-panes="1"]', groupPane);
    if (!nestedTabs || !nestedPanes) {
      openTab({ url, title });
      return;
    }

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

    nestedTabs.addEventListener('click', e => {
      const target = e.target;
      if (!(target instanceof Element)) return;
      const closeBtn = target.closest('.archix-nested-close');
      const link = target.closest('.nav-link[data-nested-tab-id]');
      const id = (link && link.getAttribute('data-nested-tab-id')) || null;
      if (!id) return;
      e.preventDefault();

      if (closeBtn) {
        const li = link?.closest('li');
        if (li) li.remove();
        const p = qs(`.tab-pane[data-nested-tab-id="${CSS.escape(id)}"]`, nestedPanes);
        if (p) p.remove();
        const remaining = qsa('.nav-link[data-nested-tab-id]', nestedTabs);
        if (remaining.length > 0) {
          activateNested(remaining[remaining.length - 1].getAttribute('data-nested-tab-id'));
        }
        return;
      }

      activateNested(id);
    }, { once: true });

    activateNested(childId);

    loadContent(url).then(result => {
      if (!result.ok) {
        pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">Yükleme hatasý (${result.status}).</div></div>`;
        return;
      }
      const html = result.text;
      let content = html;
      const m = /<body[^>]*>([\s\S]*?)<\/body>/i.exec(html);
      if (m && m[1]) content = m[1];
      pane.innerHTML = `<div class="archix-tab-content">${content}</div>`;
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

  function normalizeTitleBase(title) {
    return (title || 'Tab').trim().replace(/\s+/g, ' ');
  }

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

        if (navigator.clipboard?.writeText) {
          navigator.clipboard.writeText(payload).then(
            () => showToast('Kopyalandý.'),
            () => showToast('Kopyalama baþarýsýz.')
          );
        } else {
          try {
            const ta = document.createElement('textarea');
            ta.value = payload;
            ta.style.position = 'fixed';
            ta.style.left = '-10000px';
            document.body.appendChild(ta);
            ta.select();
            document.execCommand('copy');
            ta.remove();
            showToast('Kopyalandý.');
          } catch {
            showToast('Kopyalama baþarýsýz.');
          }
        }
      }
    });
  }

  function showAutoClosePrompt(tabId) {
    const d = state.detailById.get(tabId);
    if (!d) return;

    const c = qs(selectors.toast);
    if (!c) return;

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
        <div class="mb-2">"${escapeHtml(d.title)}" sekmesi kapatýlacak.</div>
        <div class="mb-2 d-flex align-items-center gap-2">
          <label class="form-label m-0" for="archixDeferMinutes_${escapeHtml(tabId)}">Erteleme (dk)</label>
          <input class="form-control form-control-sm" style="width:90px" type="number" min="1" max="${config.tabAutoCloseMinutes}" value="${config.tabAutoCloseMinutes}" id="archixDeferMinutes_${escapeHtml(tabId)}" />
        </div>
        <div class="d-flex gap-2 flex-wrap">
          <button type="button" class="btn btn-sm btn-light" data-action="defer">Kapatmayý Ertele</button>
          ${hasDirty ? '<button type="button" class="btn btn-sm btn-danger" data-action="closeNoSave">Kaydetmeden Kapat</button>' : ''}
          <button type="button" class="btn btn-sm btn-primary" data-action="focus">Sayfayý Aç</button>
        </div>
      </div>`;

    c.appendChild(el);

    const doRemove = () => el.remove();
    el.addEventListener('hidden.bs.toast', doRemove);

    el.addEventListener('click', e => {
      const t = e.target;
      if (!(t instanceof Element)) return;
      const btn = t.closest('button[data-action]');
      if (!btn) return;

      const action = btn.getAttribute('data-action');
      const input = el.querySelector('input[type="number"]');
      let minutes = config.tabAutoCloseMinutes;
      if (input) {
        const v = Number.parseInt(input.value, 10);
        if (!Number.isNaN(v)) minutes = v;
      }
      if (minutes < 1) minutes = 1;
      if (minutes > config.tabAutoCloseMinutes) minutes = config.tabAutoCloseMinutes;

      if (action === 'defer') {
        // Reset idle window from now.
        state.lastActivityAt = Date.now() - Math.max(0, (config.tabAutoCloseMinutes - minutes)) * 60 * 1000;
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

  function nextUniqueTitle(titleBase) {
    const base = normalizeTitleBase(titleBase);
    const next = (state.seqByTitleBase.get(base) || 0) + 1;
    state.seqByTitleBase.set(base, next);
    if (next === 1) return base;
    return `${base}_${String(next - 1).padStart(3, '0')}`;
  }

  function ensureHost() {
    const host = qs(selectors.host);
    if (!host) return null;
    const tabs = qs(selectors.tabs, host);
    const panes = qs(selectors.panes, host);
    if (!tabs || !panes) return null;
    return { host, tabs, panes };
  }

  function newId() {
    return 't_' + Math.random().toString(36).slice(2, 10) + Date.now().toString(36);
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

    // bootstrap toast
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
    const res = await fetch(url, {
      method: 'GET',
      headers: {
        'X-ArchiX-Tab': '1',
        'X-Requested-With': 'XMLHttpRequest'
      },
      credentials: 'same-origin'
    });

    return {
      ok: res.ok,
      status: res.status,
      text: await res.text()
    };
  }

  function activateTab(id) {
    const h = ensureHost();
    if (!h) return;

    state.activeId = id;
    const d = state.detailById.get(id);
    if (d) d.lastActivatedAt = Date.now();

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

    if (state.tabs.length >= config.maxOpenTabs) {
      showToast(config.maxTabReachedMessage);
      return;
    }

    const id = newId();
    const uniqueTitle = nextUniqueTitle(title);

    const tabLi = document.createElement('li');
    tabLi.className = 'nav-item';

    const tabA = document.createElement('a');
    tabA.className = 'nav-link';
    tabA.href = '#';
    tabA.setAttribute('role', 'tab');
    tabA.setAttribute('data-tab-id', id);
    tabA.innerHTML = `
      <span class="archix-tab-title">${escapeHtml(uniqueTitle)}</span>
      <button type="button" class="btn btn-sm btn-link ms-2 p-0 archix-tab-close" aria-label="Kapat" title="Kapat">&times;</button>
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
      warnedAt: null
    });
    activateTab(id);

    try {
      const result = await loadContent(url);
      if (!result.ok) {
        pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">Yükleme hatasý (${result.status}).</div></div>`;
        return;
      }

      // Render response inside tab. Expect full HTML; we take body if present.
      const html = result.text;
      let content = html;
      const m = /<body[^>]*>([\s\S]*?)<\/body>/i.exec(html);
      if (m && m[1]) content = m[1];

      pane.innerHTML = `<div class="archix-tab-content">${content}</div>`;

      bindDirtyTrackingForPane(id, pane);
    } catch {
      pane.innerHTML = `<div class="p-3"><div class="alert alert-danger">Ýçerik yüklenemedi.</div></div>`;
    }
  }

  function closeTab(id) {
    const h = ensureHost();
    if (!h) return;

    const idx = state.tabs.findIndex(t => t.id === id);
    if (idx < 0) return;

    const tab = state.tabs[idx];
    state.tabs.splice(idx, 1);
    state.detailById.delete(id);

    const link = qs(`.nav-link[data-tab-id="${CSS.escape(id)}"]`, h.tabs);
    const li = link?.closest('li');
    if (li) li.remove();

    const pane = qs(`.tab-pane[data-tab-id="${CSS.escape(id)}"]`, h.panes);
    if (pane) pane.remove();

    // Activate previous tab if any
    if (state.tabs.length > 0) {
      const next = state.tabs[Math.min(idx - 1, state.tabs.length - 1)];
      activateTab(next.id);
    } else {
      // No tabs left: open Home/Dashboard
      openTab({ url: '/Dashboard', title: 'Home/Dashboard' });
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

      e.preventDefault();

      const title = (a.textContent || '').trim() || a.getAttribute('title') || href;

      // Nested tabs: only if enabled and sidebar provides hierarchy metadata.
      if (config.enableNestedTabs) {
        const menuPath = a.getAttribute('data-archix-menu');
        if (menuPath) {
          const group = menuPath.split('/')[0];
          const groupId = `g_${group}`;
          openNestedTab({ groupId, url: href, title });
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
    // Decision 6.5.1: pointerdown, pointermove, keydown, wheel, scroll
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
    // Scope: only inactive tabs (Decision 6.6.1)
    const idleMs = getInactiveIdleMs();
    const warnMs = config.tabAutoCloseMinutes * 60 * 1000 - config.autoCloseWarningSeconds * 1000;
    if (idleMs < warnMs) return;

    // Warn once per inactive tab per idle window.
    const now = Date.now();
    for (const t of getInactiveTabs()) {
      const d = state.detailById.get(t.id);
      if (!d) continue;
      if (d.warnedAt && (now - d.warnedAt) < 10_000) continue;
      d.warnedAt = now;
      showAutoClosePrompt(t.id);
    }
  }

  function bindDirtyTrackingForPane(tabId, pane) {
    // Decision 6.11.x: only record screens for now.
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

    interceptClicks();
    bindTabHostEvents();
    bindResponseCardActions();
    initIdleTracking();

    // Warning ticker: lightweight interval.
    window.setInterval(tickAutoCloseWarnings, 1000);

    // open default home tab
    openTab({ url: '/Dashboard', title: 'Home/Dashboard' });
  }

  window.ArchiX = window.ArchiX || {};
  window.ArchiX.TabHost = {
    init,
    openTab
  };

  document.addEventListener('DOMContentLoaded', init);
})();
