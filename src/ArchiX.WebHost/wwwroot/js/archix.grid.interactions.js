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

        // Required initial behavior:
        // - Column list is populated
        // - None selected by default
        // - Keep existing state if user already selected in this session
        if (!Array.isArray(s.activeSlicerColumns)) s.activeSlicerColumns = [];

        s.slicerSelections = s.slicerSelections || {};

        // Advanced Search must stay closed on load.
        if (ensureBootstrapCollapse()) {
            const el = document.getElementById(`${tableId}-advancedSearch`);
            if (el) {
                const inst = window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
                if (el.classList.contains('show')) inst.hide();
            }
        }

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

        const toggle = document.getElementById(`${tableId}-toggleAllColumns`);
        if (toggle) toggle.checked = allColumns.length > 0 && (s.activeSlicerColumns || []).length === allColumns.length;
    }

    function removeFromArray(arr, value) {
        const v = String(value);
        return (arr || []).filter(x => String(x) !== v);
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
                    <input type="checkbox" id="${tableId}-selectall-${cssSafe(field)}" ${!selected || selected.length === 0 ? 'checked' : ''}>
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
                syncSelectAllCheckbox(tableId, field);
            });
        });

        dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option[data-value-b64] input[type="checkbox"]`).forEach(cb => {
            cb.addEventListener('change', () => syncSelectAllCheckbox(tableId, field));
        });

        const selectAllCb = dd.querySelector(`#${tableId}-selectall-${cssSafe(field)}`);
        selectAllCb?.addEventListener('change', () => {
            const checked = !!selectAllCb.checked;
            dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option[data-value-b64] input[type="checkbox"]`).forEach(x => {
                x.checked = checked;
            });
        });

        dd.querySelector('[data-apply]')?.addEventListener('click', () => applyHeaderFilter(tableId, field));
        dd.querySelector('[data-clear]')?.addEventListener('click', () => clearFilter(tableId, field));

        syncSelectAllCheckbox(tableId, field);
    }

    function syncSelectAllCheckbox(tableId, field) {
        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!dd) return;

        const selectAll = dd.querySelector(`#${tableId}-selectall-${cssSafe(field)}`);
        if (!selectAll) return;

        const all = Array.from(dd.querySelectorAll(`#${tableId}-options-${cssSafe(field)} .filter-option[data-value-b64] input[type="checkbox"]`));
        if (all.length === 0) {
            selectAll.checked = true;
            return;
        }

        const checkedCount = all.filter(x => x.checked).length;
        selectAll.checked = checkedCount === all.length;
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

    // ---------- Filter pipeline ----------
    function applyAllFilters(tableId) {
        if (typeof window.__archixGridApplyFilters === 'function') {
            const s = getInternalState(tableId);
            if (!s) return;

            s.applyFilterPipeline = () => computeFilteredData(tableId);
            window.__archixGridApplyFilters(tableId);
        }
    }

    function computeFilteredData(tableId, options) {
        const s = getInternalState(tableId);
        if (!s) return [];

        const opts = options || {};
        const term = (document.getElementById(`${tableId}-searchInput`)?.value ?? '').toLocaleLowerCase('tr-TR');

        const listFilters = s.headerListFilters || {};
        const textFilters = s.textFilters || {};
        const slicerSelections = s.slicerSelections || {};

        return (s.data || []).filter(row => {
            if (term) {
                const ok = Object.values(row || {}).some(v => String(v ?? '').toLocaleLowerCase('tr-TR').includes(term));
                if (!ok) return false;
            }

            for (const [field, values] of Object.entries(listFilters)) {
                if (!values || values.length === 0) continue;
                const v = String(row?.[field] ?? '');
                if (!values.map(String).includes(v)) return false;
            }

            for (const [field, f] of Object.entries(textFilters)) {
                if (!f) continue;
                const valRaw = row?.[field];
                const itemValue = String(valRaw ?? '').toLocaleLowerCase('tr-TR');
                const f1 = String(f.value ?? '').toLocaleLowerCase('tr-TR');

                let ok = true;
                switch (f.operator) {
                    case 'contains': ok = itemValue.includes(f1); break;
                    case 'notContains': ok = !itemValue.includes(f1); break;
                    case 'equals': ok = itemValue === f1; break;
                    case 'notEquals': ok = itemValue !== f1; break;
                    case 'startsWith': ok = itemValue.startsWith(f1); break;
                    case 'endsWith': ok = itemValue.endsWith(f1); break;
                    case 'greaterThan': ok = Number(valRaw) > Number(f.value); break;
                    case 'greaterOrEqual': ok = Number(valRaw) >= Number(f.value); break;
                    case 'lessThan': ok = Number(valRaw) < Number(f.value); break;
                    case 'lessOrEqual': ok = Number(valRaw) <= Number(f.value); break;
                    case 'between': ok = Number(valRaw) >= Number(f.value) && Number(valRaw) <= Number(f.value2); break;
                }
                if (!ok) return false;
            }

            for (const [field, values] of Object.entries(slicerSelections)) {
                if (opts.excludeSlicerColumn && field === opts.excludeSlicerColumn) continue;
                if (!values || values.length === 0) continue;
                const v = String(row?.[field] ?? '');
                if (!values.map(String).includes(v)) return false;
            }

            return true;
        });
    }

    function refreshActiveFiltersPanel(tableId) {
        const s = getInternalState(tableId);
        const root = document.getElementById(`${tableId}-activeFilters`);
        if (!s || !root) return;

        const hasList = Object.keys(s.headerListFilters || {}).some(k => (s.headerListFilters[k] || []).length > 0);
        const hasText = Object.keys(s.textFilters || {}).length > 0;
        const hasSlicer = Object.keys(s.slicerSelections || {}).length > 0;

        const totalCount =
            Object.values(s.headerListFilters || {}).reduce((acc, arr) => acc + (arr?.length || 0), 0)
            + Object.keys(s.textFilters || {}).length
            + Object.values(s.slicerSelections || {}).reduce((acc, arr) => acc + (arr?.length || 0), 0);

        if (!hasList && !hasText && !hasSlicer) {
            root.style.display = 'none';
            return;
        }

        root.style.display = 'block';

        const countEl = document.getElementById(`${tableId}-filterCount`);
        if (countEl) countEl.textContent = String(totalCount);

        // Build per-column tag list (old behavior)
        const tagsHost = document.getElementById(`${tableId}-filterTags`);
        if (tagsHost && window.ArchiXActiveFilters?.renderActiveFilters) {
            const fieldNames = s.fieldNames || {};
            const filters = [];

            // list filters (header list)
            Object.entries(s.headerListFilters || {}).forEach(([field, values]) => {
                if (!values || values.length === 0) return;
                const html = values
                    .map(v => `<span class="filter-tag">${escapeHtml(v)} <i class="bi bi-x-circle" data-ax-remove="list" data-ax-field="${escapeAttr(field)}" data-ax-value="${escapeAttr(v)}"></i></span>`)
                    .join(' ');
                filters.push({
                    column: field,
                    title: fieldNames[field] || field,
                    badge: String(values.length),
                    html
                });
            });

            // text/number filters
            Object.entries(s.textFilters || {}).forEach(([field, f]) => {
                if (!f) return;
                const opText = {
                    contains: 'İçerir',
                    notContains: 'İçermez',
                    equals: 'Eşittir',
                    notEquals: 'Eşit Değil',
                    startsWith: 'İle Başlar',
                    endsWith: 'İle Biter',
                    greaterThan: '>',
                    greaterOrEqual: '>=',
                    lessThan: '<',
                    lessOrEqual: '<=',
                    between: 'Arasında'
                }[f.operator] || f.operator;

                const display = f.operator === 'between'
                    ? `${opText} "${escapeHtml(f.value)}"-"${escapeHtml(f.value2)}"`
                    : `${opText} "${escapeHtml(f.value)}"`;

                filters.push({
                    column: field,
                    title: fieldNames[field] || field,
                    badge: '1',
                    html: `<span class="filter-tag">${display} <i class="bi bi-x-circle" data-ax-remove="text" data-ax-field="${escapeAttr(field)}"></i></span>`
                });
            });

            // slicer selections
            Object.entries(s.slicerSelections || {}).forEach(([field, values]) => {
                if (!values || values.length === 0) return;
                const html = values
                    .map(v => `<span class="filter-tag">${escapeHtml(v)} <i class="bi bi-x-circle" data-ax-remove="slicer" data-ax-field="${escapeAttr(field)}" data-ax-value="${escapeAttr(v)}"></i></span>`)
                    .join(' ');
                filters.push({
                    column: `slicer-${field}`,
                    title: (fieldNames[field] || field) + ' (Slicer)',
                    badge: String(values.length),
                    html
                });
            });

            // Sort by original column order when possible
            const order = (s.columns || []).map(c => c.field);
            filters.sort((a, b) => order.indexOf(String(a.column).replace(/^slicer-/, '')) - order.indexOf(String(b.column).replace(/^slicer-/, '')));

            window.ArchiXActiveFilters.renderActiveFilters({
                tableId,
                filters,
                totalBadgeText: String(totalCount)
            });

            // Bind remove clicks (event delegation)
            tagsHost.onclick = (ev) => {
                const t = ev.target;
                if (!t?.getAttribute) return;
                const kind = t.getAttribute('data-ax-remove');
                if (!kind) return;
                ev.preventDefault();
                ev.stopPropagation();

                const field = t.getAttribute('data-ax-field');
                const value = t.getAttribute('data-ax-value');

                if (kind === 'list' && field) {
                    const cur = s.headerListFilters?.[field] || [];
                    const next = removeFromArray(cur, value);
                    if (next.length === 0) {
                        if (s.headerListFilters) delete s.headerListFilters[field];
                    } else {
                        s.headerListFilters[field] = next;
                    }
                    updateFilterIcon(tableId, field);
                }

                if (kind === 'text' && field) {
                    if (s.textFilters) delete s.textFilters[field];
                    updateFilterIcon(tableId, field);
                }

                if (kind === 'slicer' && field) {
                    const cur = s.slicerSelections?.[field] || [];
                    const next = removeFromArray(cur, value);
                    if (next.length === 0) {
                        if (s.slicerSelections) delete s.slicerSelections[field];
                    } else {
                        s.slicerSelections[field] = next;
                    }
                }

                createColumnCheckList(tableId);
                updateAllSlicers(tableId);
                applyAllFilters(tableId);
                refreshActiveFiltersPanel(tableId);
            };
        }

        // keep collapsed default; no auto open/close
    }

    document.addEventListener('click', (e) => {
        const t = e.target;
        if (t?.closest?.('.filter-dropdown') || t?.closest?.('.filter-icon')) return;
        document.querySelectorAll('.filter-dropdown.show').forEach(x => x.classList.remove('show'));
    });

    // ---------- Helpers ----------
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
    
    // Ensure escapeAttr is used for HTML attribute contexts when injecting data-* values
    function setDataAttribute(element, name, value) {
        element.setAttribute('data-' + escapeAttr(name), escapeAttr(value));
    }

    function renderTextInputs(tableId, field, isNumeric, saved) {
        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!dd) return;
        const host = dd.querySelector(`#${tableId}-filter-inputs-${cssSafe(field)}`);
        if (!host) return;

        const op = saved.operator;
        const t = isNumeric ? 'number' : 'text';

        if (op === 'between') {
            host.innerHTML = `
                <input type="${t}" class="text-filter-input mb-2" id="${tableId}-text-value-${cssSafe(field)}" placeholder="Başlangıç..." value="${escapeAttr(saved.value || '')}">
                <input type="${t}" class="text-filter-input" id="${tableId}-text-value2-${cssSafe(field)}" placeholder="Bitiş..." value="${escapeAttr(saved.value2 || '')}">
            `;
        } else {
            host.innerHTML = `
                <input type="${t}" class="text-filter-input" id="${tableId}-text-value-${cssSafe(field)}" placeholder="Değer girin..." value="${escapeAttr(saved.value || '')}">
            `;
        }
    }

    function switchFilterMode(tableId, field, mode) {
        const s = getInternalState(tableId);
        if (!s) return;
        s.headerFilterMode = s.headerFilterMode || {};
        s.headerFilterMode[field] = mode;
        buildFilterDropdown(tableId, field);
    }

    function updateFilterIcon(tableId, field) {
        const s = getInternalState(tableId);
        if (!s) return;

        const hasList = !!(s.headerListFilters?.[field]?.length);
        const hasText = !!(s.textFilters?.[field]);
        const hasFilter = hasList || hasText;

        const countEl = document.getElementById(`${tableId}-count-${field}`);
        const iconEl = countEl?.closest?.('.filter-icon');

        if (iconEl) {
            if (hasFilter) iconEl.classList.add('active');
            else iconEl.classList.remove('active');
        }

        if (countEl) {
            if (!hasFilter) {
                countEl.style.display = 'none';
                countEl.textContent = '0';
            } else {
                countEl.style.display = 'inline-block';
                countEl.textContent = hasList ? String(s.headerListFilters[field].length) : '1';
            }
        }
    }

    function clearFilter(tableId, field) {
        const s = getInternalState(tableId);
        const dd = document.getElementById(`${tableId}-filter-${field}`);
        if (!s) return;

        if (s.headerListFilters) delete s.headerListFilters[field];
        if (s.textFilters) delete s.textFilters[field];

        updateFilterIcon(tableId, field);
        dd?.classList.remove('show');

        applyAllFilters(tableId);
        refreshActiveFiltersPanel(tableId);

        updateAllSlicers(tableId);
        createColumnCheckList(tableId);
    }

    // Public API
    window.toggleAdvancedSearch = toggleAdvancedSearch;
    window.toggleFilter = toggleFilter;
    window.clearAdvancedFilters = clearAdvancedFilters;
    window.toggleAllColumns = toggleAllColumns;
    window.initSlicers = initSlicers;

    // Active Filters: "Hepsini Aç/Kapat"
    window.toggleAllFilterAccordions = function (tableId) {
        if (window.ArchiXActiveFilters?.toggleAll) window.ArchiXActiveFilters.toggleAll(tableId);
    };

    // Called from `archix.grid.component.js` after reset.
    // Keeps UI in sync (slicer cards, checklist colors, active filter panel)
    window.__archixGridAfterReset = window.__archixGridAfterReset || function (tableId) {
        const s = getInternalState(tableId);
        if (s) {
            (s.columns || []).forEach(c => {
                if (c?.field) updateFilterIcon(tableId, c.field);
            });
        }
        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        refreshActiveFiltersPanel(tableId);
    };

    document.addEventListener('DOMContentLoaded', () => {
        Object.keys(window.gridTables || {}).forEach(tableId => {
            initSlicers(tableId);
            refreshActiveFiltersPanel(tableId);
        });
    });
})(window);
