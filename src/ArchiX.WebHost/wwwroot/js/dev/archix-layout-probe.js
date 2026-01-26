(() => {
  'use strict';

  const targets = [
    { name: '#sidebar', sel: '#sidebar' },
    { name: '.main-content', sel: '.main-content' },
    { name: '#archix-tabhost', sel: '#archix-tabhost' },
    { name: '#archix-tabhost-tabs', sel: '#archix-tabhost-tabs' },
    { name: '#archix-tabhost-panes', sel: '#archix-tabhost-panes' },
    { name: '.archix-tab-content (first)', sel: '#archix-tabhost-panes .archix-tab-content' }
  ];

  function pick(elOrNull, name) {
    if (!elOrNull) {
      return { name, exists: false };
    }

    const el = elOrNull;
    const r = el.getBoundingClientRect();
    const cs = window.getComputedStyle(el);

    const toNum = (px) => {
      if (px == null) return null;
      const s = String(px);
      if (!s.endsWith('px')) return s;
      const v = Number.parseFloat(s);
      return Number.isFinite(v) ? v : s;
    };

    const edges = {
      marginLeft: toNum(cs.marginLeft),
      marginRight: toNum(cs.marginRight),
      paddingLeft: toNum(cs.paddingLeft),
      paddingRight: toNum(cs.paddingRight),
      borderLeftWidth: toNum(cs.borderLeftWidth),
      borderRightWidth: toNum(cs.borderRightWidth),
      boxSizing: cs.boxSizing
    };

    const rect = {
      left: r.left,
      right: r.right,
      width: r.width,
      top: r.top,
      bottom: r.bottom,
      height: r.height,
      leftFrac: r.left - Math.floor(r.left),
      rightFrac: r.right - Math.floor(r.right)
    };

    return {
      name,
      exists: true,
      rect,
      edges,
      className: el.className,
      id: el.id
    };
  }

  function dump() {
    const dpr = window.devicePixelRatio || 1;
    const viewport = { w: window.innerWidth, h: window.innerHeight };

    const rows = targets
      .map(t => ({ t, el: document.querySelector(t.sel) }))
      .map(x => pick(x.el, x.t.name));

    const sidebar = document.querySelector('#sidebar');
    const tabhost = document.querySelector('#archix-tabhost');

    let seam = null;
    if (sidebar && tabhost) {
      const s = sidebar.getBoundingClientRect();
      const t = tabhost.getBoundingClientRect();
      seam = {
        sidebarRight: s.right,
        tabhostLeft: t.left,
        gapPx: t.left - s.right,
        sidebarRightFrac: s.right - Math.floor(s.right),
        tabhostLeftFrac: t.left - Math.floor(t.left)
      };
    }

    console.groupCollapsed('[ArchiX layoutProbe] TabHost/Sidebar metrics');
    console.log({ dpr, viewport, seam, at: new Date().toISOString() });
    console.table(
      rows.map(r => ({
        name: r.name,
        exists: r.exists,
        left: r.rect?.left,
        right: r.rect?.right,
        width: r.rect?.width,
        leftFrac: r.rect?.leftFrac,
        rightFrac: r.rect?.rightFrac,
        marginLeft: r.edges?.marginLeft,
        paddingLeft: r.edges?.paddingLeft,
        borderLeftWidth: r.edges?.borderLeftWidth,
        boxSizing: r.edges?.boxSizing,
        id: r.id,
        className: r.className
      }))
    );
    console.log('Full rows:', rows);
    console.groupEnd();

    drawOverlay(rows);
  }

  function drawOverlay(rows) {
    const old = document.getElementById('__archix_layout_probe_overlay');
    if (old) old.remove();

    const overlay = document.createElement('div');
    overlay.id = '__archix_layout_probe_overlay';
    overlay.style.position = 'fixed';
    overlay.style.inset = '0';
    overlay.style.pointerEvents = 'none';
    overlay.style.zIndex = '2147483647';

    // Draw a vertical line at sidebar right edge and tabhost left edge to visualize the gap.
    const sidebar = document.querySelector('#sidebar');
    const tabhost = document.querySelector('#archix-tabhost');

    const line = (x, color, label) => {
      const l = document.createElement('div');
      l.style.position = 'absolute';
      l.style.top = '0';
      l.style.bottom = '0';
      l.style.left = `${x}px`;
      l.style.width = '1px';
      l.style.background = color;

      const tag = document.createElement('div');
      tag.textContent = label;
      tag.style.position = 'absolute';
      tag.style.left = `${x + 4}px`;
      tag.style.top = '4px';
      tag.style.padding = '2px 6px';
      tag.style.font = '12px/1.2 monospace';
      tag.style.background = 'rgba(0,0,0,0.7)';
      tag.style.color = 'white';
      tag.style.borderRadius = '4px';

      overlay.appendChild(l);
      overlay.appendChild(tag);
    };

    if (sidebar) {
      const s = sidebar.getBoundingClientRect();
      line(s.right, 'rgba(255,0,0,0.9)', `sidebar.right=${s.right.toFixed(2)}`);
    }

    if (tabhost) {
      const t = tabhost.getBoundingClientRect();
      line(t.left, 'rgba(0,255,0,0.9)', `tabhost.left=${t.left.toFixed(2)}`);
    }

    // Also draw element outlines (helpful for seeing which one is shifted)
    for (const r of rows) {
      if (!r.exists) continue;
      const el = document.querySelector(targets.find(t => t.name === r.name)?.sel || '');
      if (!el) continue;
      const b = el.getBoundingClientRect();
      const box = document.createElement('div');
      box.style.position = 'absolute';
      box.style.left = `${b.left}px`;
      box.style.top = `${b.top}px`;
      box.style.width = `${b.width}px`;
      box.style.height = `${b.height}px`;
      box.style.outline = '1px dashed rgba(0, 153, 255, 0.8)';
      overlay.appendChild(box);
    }

    document.body.appendChild(overlay);
  }

  function scheduleDump() {
    // 1) After initial layout
    requestAnimationFrame(() => requestAnimationFrame(dump));

    // 2) After a short delay (tabs may be injected async)
    setTimeout(dump, 500);
    setTimeout(dump, 1500);

    // 3) On resize
    window.addEventListener('resize', () => {
      setTimeout(dump, 0);
    });
  }

  scheduleDump();
})();
