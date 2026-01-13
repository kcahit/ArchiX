'use strict';

// Grid header interactions: sorting, header filter dropdown, advanced search (slicer) panel.
// Works with `archix.grid.component.js` state via `window.__archixGridGetState` + `window.__archixGridApplyFilters`.

(function (window) {
    function getInternalState(tableId) {
        return window.__archixGridGetState ? window.__archixGridGetState(tableId) : null;
    }

    function ensureBootstrapCollapse() {
        return !!window.bootstrap?.Collapse;
    }

    // ---------- Advanced Search (Slicer) ----------
    function toggleAdvancedSearch(tableId) {
        if (!ensureBootstrapCollapse()) return;
        const el = document.getElementById(`${tableId}-advancedSearch`);
        if (!el) return;
        const inst = window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
        if (el.classList.contains('show')) inst.hide(); else inst.show();
    }

    function initSlicers(tableId) {
        const s = getInternalState(tableId);
        if (!s) return;

        s.fieldNames = s.fieldNames || Object.fromEntries((s.columns || []).map(c => [c.field, c.title || c.field]));
        s.activeSlicerColumns = s.activeSlicerColumns || [];
        s.slicerSelections = s.slicerSelections || {};

        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
    }

    function clearAdvancedFilters(tableId) {
        const s = getInternalState(tableId);
        if (!s) return;

        s.slicerSelections = {};
        createColumnCheckList(tableId);
        updateAllSlicers(tableId);
        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);
    }

    function toggleAllColumns(tableId) {
        const s = getInternalState(tableId);
        if (!s) return;

        const toggle = document.getElementById(`${tableId}-toggleAllColumns`);
        const all = (s.columns || []).map(c => c.field).filter(f => f !== 'id');

        if (toggle?.checked) {
            s.activeSlicerColumns = [...all];
        } else {
            s.activeSlicerColumns = [];
            s.slicerSelections = {};
        }

        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);
    }

    function createColumnCheckList(tableId) {
        const s = getInternalState(tableId);
        const host = document.getElementById(`${tableId}-columnCheckList`);
        if (!s || !host) return;

        const allColumns = (s.columns || []).map(c => c.field).filter(f => f !== 'id');
        host.innerHTML = '';

        allColumns.forEach(column => {
            const isChecked = (s.activeSlicerColumns || []).includes(column);
            const hasFilter = !!(s.slicerSelections?.[column]?.length);

            const div = document.createElement('div');
            div.className = `column-check${isChecked ? ' is-active' : ''}${hasFilter ? ' has-selection' : ''}`;
            div.innerHTML = `
                <label for="${tableId}-check-${column}">
                    <input type="checkbox" id="${tableId}-check-${column}" ${isChecked ? 'checked' : ''} />
                    ${escapeHtml(s.fieldNames?.[column] || column)}
                </label>
            `;

            const cb = div.querySelector('input');
            cb.addEventListener('change', () => toggleSlicerColumn(tableId, column));
            host.appendChild(div);
        });
    }

    function toggleSlicerColumn(tableId, column) {
        const s = getInternalState(tableId);
        if (!s) return;

        const idx = (s.activeSlicerColumns || []).indexOf(column);
        if (idx > -1) {
            s.activeSlicerColumns.splice(idx, 1);
            if (s.slicerSelections) delete s.slicerSelections[column];
        } else {
            s.activeSlicerColumns = s.activeSlicerColumns || [];
            s.activeSlicerColumns.push(column);
            const order = (s.columns || []).map(c => c.field);
            s.activeSlicerColumns.sort((a, b) => order.indexOf(a) - order.indexOf(b));
        }

        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);
    }

    function rebuildSlicers(tableId) {
        const s = getInternalState(tableId);
        const container = document.getElementById(`${tableId}-slicerContainer`);
        if (!s || !container) return;

        const cols = s.activeSlicerColumns || [];
        container.innerHTML = '';

        if (cols.length === 0) {
            container.innerHTML = `
                <div class="grid-no-slicer">
                    <i class="bi bi-arrow-left"></i> Soldaki listeden kolon seçin
                </div>
            `;
            return;
        }

        cols.forEach(column => {
            const hasFilter = !!(s.slicerSelections?.[column]?.length);
            const card = document.createElement('div');
            card.className = `grid-slicer-card${hasFilter ? ' has-filter' : ''}`;
            card.innerHTML = `
                <h6 title="${escapeHtml(s.fieldNames?.[column] || column)}">${escapeHtml(s.fieldNames?.[column] || column)}</h6>
                <div class="slicer-items" id="${tableId}-slicer-${column}"></div>
            `;
            container.appendChild(card);
            updateSlicerItems(tableId, column);
        });
    }

    function updateSlicerItems(tableId, column) {
        const s = getInternalState(tableId);
        const host = document.getElementById(`${tableId}-slicer-${column}`);
        if (!s || !host) return;

        const selectedValues = s.slicerSelections?.[column] || [];

        const availableData = computeFilteredData(tableId, { excludeSlicerColumn: column });
        const values = Array.from(new Set(availableData.map(r => r?.[column]).map(v => String(v ?? ''))))
            .sort((a, b) => a.localeCompare(b, 'tr-TR'));

        host.innerHTML = '';
        values.forEach(v => {
            const isSelected = selectedValues.includes(v);
            const item = document.createElement('div');
            item.className = `slicer-item${isSelected ? ' is-selected' : ''}`;
            item.textContent = v;
            item.title = v;
            item.addEventListener('click', () => toggleSlicerValue(tableId, column, v));
            host.appendChild(item);
        });
    }

    function toggleSlicerValue(tableId, column, value) {
        const s = getInternalState(tableId);
        if (!s) return;

        s.slicerSelections = s.slicerSelections || {};
        s.slicerSelections[column] = s.slicerSelections[column] || [];

        const v = String(value);
        const idx = s.slicerSelections[column].indexOf(v);
        if (idx > -1) s.slicerSelections[column].splice(idx, 1);
        else s.slicerSelections[column].push(v);

        if (s.slicerSelections[column].length === 0) delete s.slicerSelections[column];

        createColumnCheckList(tableId);
        updateAllSlicers(tableId);
        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);
    }

    function updateAllSlicers(tableId) {
        const s = getInternalState(tableId);
        if (!s) return;
        (s.activeSlicerColumns || []).forEach(c => updateSlicerItems(tableId, c));
        rebuildSlicers(tableId);
    }

    // ---------- Header Filter Dropdown (List + Text/Number) ----------
    function toggleFilter(tableId, field, ev) {
        ev?.stopPropagation?.();
        ev?.preventDefault?.();

        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!dd) return;

        document.querySelectorAll(`[id^='${tableId}-filter-']`).forEach(x => {
            if (x !== dd) x.classList.remove('show');
        });

        const isOpen = dd.classList.contains('show');
        if (isOpen) {
            dd.classList.remove('show');
            return;
        }

        buildFilterDropdown(tableId, field);
        dd.classList.add('show');

        const rect = dd.getBoundingClientRect();
        if (rect.right > window.innerWidth) {
            dd.style.left = 'auto';
            dd.style.right = '0';
        } else {
            dd.style.left = '0';
            dd.style.right = 'auto';
        }
    }

    function buildFilterDropdown(tableId, field) {
        const s = getInternalState(tableId);
        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!s || !dd) return;

        s.headerFilterMode = s.headerFilterMode || {};
        s.headerListFilters = s.headerListFilters || {};
        s.textFilters = s.textFilters || {};

        const mode = s.headerFilterMode[field] || 'list';

        const allValues = Array.from(new Set((s.data || []).map(r => r?.[field]).map(v => String(v ?? ''))))
            .sort((a, b) => a.localeCompare(b, 'tr-TR'));

        const isNumeric = allValues.length > 0 && allValues.every(v => v.trim() !== '' && !Number.isNaN(Number(v)));

        let html = `
            <div class="filter-type-selector">
                <button type="button" class="filter-type-btn ${mode === 'number' ? 'active' : ''}" data-mode="number">
                    <i class="bi bi-123"></i> ${isNumeric ? 'Sayı' : 'Metin'}
                </button>
                <button type="button" class="filter-type-btn ${mode === 'list' ? 'active' : ''}" data-mode="list">
                    <i class="bi bi-list-ul"></i> Liste
                </button>
            </div>
        `;

        if (mode === 'number') {
            const saved = s.textFilters[field] || { operator: (isNumeric ? 'equals' : 'contains'), value: '', value2: '' };
            const operators = isNumeric
                ? [
                    ['equals', 'Eşittir'],
                    ['notEquals', 'Eşit Değil'],
                    ['greaterThan', 'Büyüktür'],
                    ['greaterOrEqual', 'Büyük veya Eşit'],
                    ['lessThan', 'Küçüktür'],
                    ['lessOrEqual', 'Küçük veya Eşit'],
                    ['between', 'Arasında'],
                ]
                : [
                    ['contains', 'İçerir'],
                    ['notContains', 'İçermez'],
                    ['equals', 'Eşittir'],
                    ['notEquals', 'Eşit Değil'],
                    ['startsWith', 'İle Başlar'],
                    ['endsWith', 'İle Biter'],
                ];

            html += `
                <div class="text-filter-section">
                    <div class="text-filter-row">
                        <select class="text-filter-operator" id="${tableId}-text-operator-${cssSafe(field)}">
                            ${operators.map(([v, t]) => `<option value="${v}" ${saved.operator === v ? 'selected' : ''}>${t}</option>`).join('')}
                        </select>
                    </div>
                    <div id="${tableId}-filter-inputs-${cssSafe(field)}"></div>
                </div>
            `;

            dd.innerHTML = html + `
                <div class="filter-actions">
                    <button type="button" class="btn-apply-filter" data-apply="1"><i class="bi bi-check-circle"></i> Uygula</button>
                    <button type="button" class="btn-clear-filter" data-clear="1"><i class="bi bi-x-circle"></i> Temizle</button>
                </div>`;

            renderTextInputs(tableId, field, isNumeric, saved);

            dd.querySelector(`[data-mode="number"]`)?.addEventListener('click', (e) => { e.stopPropagation(); switchFilterMode(tableId, field, 'number'); });
            dd.querySelector(`[data-mode="list"]`)?.addEventListener('click', (e) => { e.stopPropagation(); switchFilterMode(tableId, field, 'list'); });

            dd.querySelector(`#${tableId}-text-operator-${cssSafe(field)}`)?.addEventListener('change', () => {
                const op = dd.querySelector(`#${tableId}-text-operator-${cssSafe(field)}`)?.value;
                const cur = s.textFilters[field] || { operator: op, value: '', value2: '' };
                cur.operator = op;
                s.textFilters[field] = cur;
                renderTextInputs(tableId, field, isNumeric, cur);
            });

            dd.querySelector('[data-apply]')?.addEventListener('click', () => applyHeaderFilter(tableId, field));
            dd.querySelector('[data-clear]')?.addEventListener('click', () => clearFilter(tableId, field));
            return;
        }

        const selected = s.headerListFilters[field] || null;
        html += `
            <input type="text" class="filter-search" placeholder="Ara..." id="${tableId}-filter-search-${cssSafe(field)}">
            <div class="filter-options" id="${tableId}-options-${cssSafe(field)}">
                <div class="filter-option" data-select-all="1">
                    <input type="checkbox" ${!selected || selected.length === 0 ? 'checked' : ''}>
                    <strong>(Tümünü Seç)</strong>
                </div>
                ${allValues.map(v => {
                    const isChecked = !selected || selected.includes(v);
                    const safeText = escapeHtml(v);
                    const encoded = b64Encode(v);
                    return `
                        <div class="filter-option" data-value-b64="${encoded}">
                            <input type="checkbox" ${isChecked ? 'checked' : ''}>
                            <span>${safeText}</span>
                        </div>`;
                }).join('')}
            </div>
        `;

        dd.innerHTML = html + `
            <div class="filter-actions">
                <button type="button" class="btn-apply-filter" data-apply="1"><i class="bi bi-check-circle"></i> Uygula</button>
                <button type="button" class="btn-clear-filter" data-clear="1"><i class="bi bi-x-circle"></i> Temizle</button>
            </div>`;

        dd.querySelector(`[data-mode="number"]`)?.addEventListener('click', (e) => { e.stopPropagation(); switchFilterMode(tableId, field, 'number'); });
        dd.querySelector(`[data-mode="list"]`)?.addEventListener('click', (e) => { e.stopPropagation(); switchFilterMode(tableId, field, 'list'); });

        dd.querySelector(`#${tableId}-filter-search-${cssSafe(field)}`)?.addEventListener('keyup', (e) => {
            const term = String(e.target.value ?? '').toLocaleLowerCase('tr-TR');
            dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option[data-value-b64]`).forEach(opt => {
                const text = opt.textContent?.toLocaleLowerCase('tr-TR') || '';
                opt.style.display = text.includes(term) ? 'flex' : 'none';
            });
        });

        dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option`).forEach(opt => {
            opt.addEventListener('click', (e) => {
                if (e.target?.type === 'checkbox') return;
                const cb = opt.querySelector('input[type="checkbox"]');
                if (cb) cb.checked = !cb.checked;
            });
        });

        dd.querySelector('[data-apply]')?.addEventListener('click', () => applyHeaderFilter(tableId, field));
        dd.querySelector('[data-clear]')?.addEventListener('click', () => clearFilter(tableId, field));
    }

    function applyHeaderFilter(tableId, field) {
        const s = getInternalState(tableId);
        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!s || !dd) return;

        const mode = s.headerFilterMode?.[field] || 'list';

        if (mode === 'number') {
            const op = dd.querySelector(`#${tableId}-text-operator-${cssSafe(field)}`)?.value;
            const v1 = dd.querySelector(`#${tableId}-text-value-${cssSafe(field)}`)?.value ?? '';
            const v2 = dd.querySelector(`#${tableId}-text-value2-${cssSafe(field)}`)?.value ?? '';

            if (op && String(v1).trim() !== '') {
                s.textFilters[field] = op === 'between'
                    ? { operator: op, value: String(v1), value2: String(v2) }
                    : { operator: op, value: String(v1) };
                if (s.headerListFilters) delete s.headerListFilters[field];
            } else {
                if (s.textFilters) delete s.textFilters[field];
            }
        } else {
            const selectedValues = [];
            dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option[data-value-b64] input[type="checkbox"]`).forEach(cb => {
                if (cb.checked) {
                    const b64 = cb.closest('.filter-option')?.getAttribute('data-value-b64');
                    const raw = b64Decode(b64);
                    if (raw !== null && raw !== undefined) selectedValues.push(String(raw));
                }
            });

            const all = Array.from(new Set((s.data || []).map(r => r?.[field]).map(v => String(v ?? ''))));
            if (selectedValues.length === 0 || selectedValues.length === all.length) {
                if (s.headerListFilters) delete s.headerListFilters[field];
            } else {
                s.headerListFilters[field] = selectedValues;
            }
            if (s.textFilters) delete s.textFilters[field];
        }

        updateFilterIcon(tableId, field);
        dd.classList.remove('show');

        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);

        if ((s.activeSlicerColumns || []).includes(field)) updateAllSlicers(tableId);
        createColumnCheckList(tableId);
    }

    // ...existing code (unchanged below)...

    function b64Encode(str) {
        try {
            return btoa(unescape(encodeURIComponent(String(str ?? ''))));
        } catch {
            return '';
        }
    }

    function b64Decode(b64) {
        try {
            if (!b64) return '';
            return decodeURIComponent(escape(atob(String(b64))));
        } catch {
            return '';
        }
    }

    // ---------- Helpers ----------
    function cssSafe(v) {
        return String(v).replace(/[^a-zA-Z0-9_-]/g, '_');
    }

    function escapeHtml(v) {
        return String(v ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/\"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function escapeAttr(v) {
        return escapeHtml(v).replace(/`/g, '&#096;');
    }

    // ...existing code...
})(window);
