'use strict';

// Minimal, isolated Active Filters accordion helper for ArchiX grid.
// Purpose: avoid manual `.show` class toggling and rely on Bootstrap Collapse API.
// This file is intentionally small to debug the "opens but doesn't close" issue.

(function (window) {
    function sanitizeId(key) {
        return String(key || '').replace(/[^a-zA-Z0-9_-]/g, '');
    }

    // `filters` format example:
    // [{ column:'Name', title:'Ad', badge:'2', html:'<span class="filter-tag">...</span>' }]
    function renderActiveFilters({
        tableId,
        filters,
        totalBadgeText
    }) {
        const container = document.getElementById(`${tableId}-activeFilters`);
        const tagsContainer = document.getElementById(`${tableId}-filterTags`);
        const filterCountSpan = document.getElementById(`${tableId}-filterCount`);
        if (!container || !tagsContainer || !filterCountSpan) return;

        if (!filters || filters.length === 0) {
            container.style.display = 'none';
            filterCountSpan.textContent = '0';
            tagsContainer.innerHTML = '';
            return;
        }

        container.style.display = 'block';
        filterCountSpan.textContent = totalBadgeText || '';

        // Keep open state across rerender
        const openAccordions = new Set();
        tagsContainer.querySelectorAll('.accordion-collapse.show')
            .forEach(c => openAccordions.add(c.dataset.collapseId || c.id));

        tagsContainer.innerHTML = '';

        filters.forEach(f => {
            const colCollapseId = `${tableId}-colCollapse-${sanitizeId(f.column)}`;
            const isOpen = openAccordions.has(colCollapseId);

            const accordionDiv = document.createElement('div');
            accordionDiv.className = 'accordion mb-2 filter-summary-accordion';

            const accordionItem = document.createElement('div');
            accordionItem.className = 'accordion-item border-0';

            const header = document.createElement('h2');
            header.className = 'accordion-header';

            const btn = document.createElement('button');
            btn.className = `accordion-button filter-summary-trigger filter-accordion-btn ${isOpen ? '' : 'collapsed'}`;
            btn.type = 'button';
            btn.setAttribute('data-bs-toggle', 'collapse');
            btn.setAttribute('data-bs-target', `#${colCollapseId}`);
            btn.setAttribute('aria-controls', colCollapseId);
            btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');

            const titleSpan = document.createElement('span');
            titleSpan.className = 'filter-accordion-title';
            titleSpan.textContent = f.title || f.column;

            const badgeSpan = document.createElement('span');
            badgeSpan.className = 'badge filter-summary-badge ms-2';
            badgeSpan.textContent = f.badge || '';

            const collapseDiv = document.createElement('div');
            collapseDiv.id = colCollapseId;
            collapseDiv.dataset.collapseId = colCollapseId;
            collapseDiv.className = `accordion-collapse collapse ${isOpen ? 'show' : ''}`;

            const bodyDiv = document.createElement('div');
            bodyDiv.className = 'accordion-body filter-accordion-body';
            bodyDiv.style.fontSize = '0.7rem';
            bodyDiv.innerHTML = f.html || '';

            btn.appendChild(titleSpan);
            btn.appendChild(badgeSpan);

            // Keep aria/class in sync even if Bootstrap changes state
            collapseDiv.addEventListener('shown.bs.collapse', () => {
                btn.classList.remove('collapsed');
                btn.setAttribute('aria-expanded', 'true');
            });
            collapseDiv.addEventListener('hidden.bs.collapse', () => {
                btn.classList.add('collapsed');
                btn.setAttribute('aria-expanded', 'false');
            });

            header.appendChild(btn);
            collapseDiv.appendChild(bodyDiv);
            accordionItem.appendChild(header);
            accordionItem.appendChild(collapseDiv);
            accordionDiv.appendChild(accordionItem);
            tagsContainer.appendChild(accordionDiv);
        });
    }

    function toggleAll(tableId) {
        const container = document.getElementById(`${tableId}-filterTags`);
        const btn = document.getElementById(`${tableId}-toggleAllBtn`);
        if (!container || !btn || !window.bootstrap?.Collapse) return;

        const allCollapses = Array.from(container.querySelectorAll('.accordion-collapse'));
        const isAnyOpen = allCollapses.some(c => c.classList.contains('show'));

        allCollapses.forEach(el => {
            const instance = window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
            if (isAnyOpen) instance.hide(); else instance.show();
        });

        btn.innerHTML = isAnyOpen
            ? '<i class="bi bi-chevron-down"></i> Hepsini Aç'
            : '<i class="bi bi-chevron-up"></i> Hepsini Kapat';
    }

    window.ArchiXActiveFilters = {
        renderActiveFilters,
        toggleAll
    };
})(window);
