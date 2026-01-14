(() => {
  const state = {
    tabs: [],
    activeId: null,
    seqByTitleBase: new Map()
  };

  const config = {
    maxOpenTabs: 15,
    maxTabReachedMessage: 'Açýk tab sayýsý 15 limitine geldi. Lütfen açýk tablardan birini kapatýnýz.'
  };

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
      openTab({ url: href, title });
    });
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
