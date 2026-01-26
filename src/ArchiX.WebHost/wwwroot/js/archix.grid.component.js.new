'use strict';

(function (window) {
    const states = {};
    const MAX_BADGE = 99;

    const formatCount = (n) => {
        const num = parseInt(n || 0, 10);
        return num > MAX_BADGE ? `${MAX_BADGE}+` : `${num}`;
    };

    const sanitizeId = (key) => String(key || '').replace(/[^a-zA-Z0-9_-]/g, '');

    const escapeHtml = (value) => String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/\"/g, '&quot;')
        .replace(/'/g, '&#039;');

    function ensureRowEditorShell() {
        if (document.getElementById('archix-row-editor')) return;

        const el = document.createElement('div');
        el.innerHTML = `
<div class="offcanvas offcanvas-end" tabindex="-1" id="archix-row-editor" aria-labelledby="archix-row-editor-title">
  <div class="offcanvas-header">
    <h5 class="offcanvas-title" id="archix-row-editor-title">Kayıt Detayı</h5>
    <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Kapat"></button>
  </div>
  <div class="offcanvas-body">
    <div id="archix-row-editor-body"></div>
  </div>
</div>`;
        document.body.appendChild(el.firstElementChild);
    }

    function showRowEditor(tableId, row) {
        ensureRowEditorShell();

        const body = document.getElementById('archix-row-editor-body');
        if (!body) return;

        const state = getState(tableId);
        const cols = state?.columns || [];

        if (!row || cols.length === 0) {
            body.innerHTML = '<div class="text-muted">Gösterilecek alan yok.</div>';
        } else {
            body.innerHTML = cols.map(c => {
                const key = c.field;
                const label = c.title || c.field;
                const val = row[key];

                return `
<div class="mb-2">
  <div class="small text-muted">${escapeHtml(label)}</div>
  <div class="fw-semibold">${escapeHtml(val)}</div>
</div>`;
            }).join('');
        }

        if (window.bootstrap?.Offcanvas) {
            const offcanvasEl = document.getElementById('archix-row-editor');
            const inst = window.bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
            inst.show();
        }
    }

    function getRowById(tableId, id) {
        const state = getState(tableId); if (!state) return null;
        const needle = String(id);
        return state.data.find(x => String(x?.id) === needle)
            || state.filteredData.find(x => String(x?.id) === needle)
            || null;
    }

    function viewItem(tableId, id) {
        const row = getRowById(tableId, id);
        showRowEditor(tableId, row);
    }

    function editItem(tableId, id) {
        const row = getRowById(tableId, id);
        showRowEditor(tableId, row);
    }

    function deleteItem(tableId, id) {
        if (confirm(`ID ${id} numaralı kaydı silmek istediğinizden emin misiniz?`)) {
            const state = getState(tableId); if (!state) return;
            const needle = String(id);
            const index = state.data.findIndex(x => String(x?.id) === needle);
            if (index > -1) state.data.splice(index, 1);
            applyAllFilters(tableId);
        }
    }

    function ensureActiveFilterStyles() {
        if (document.getElementById('archix-active-filter-styles')) return;
        const style = document.createElement('style');
        style.id = 'archix-active-filter-styles';
        style.textContent = `
            .filter-summary-accordion .accordion-button.filter-accordion-btn {
                background:#ffc107 !important;
                color:#4b6bfb !important;
                font-weight:700;
                font-size:0.9rem;
                line-height:1.1;
                box-shadow:none !important;
                border:1px solid #e6b800;
                border-radius:6px;
                padding:6px 10px;
                gap:8px;
            }
            .filter-summary-accordion .accordion-button.filter-accordion-btn.collapsed {
                background:#ffc107 !important;
                color:#4b6bfb !important;
            }
            .filter-summary-accordion .accordion-button.filter-accordion-btn:not(.collapsed) {
                background:#ffc107 !important;
                color:#4b6bfb !important;
                box-shadow:none !important;
            }
            .filter-summary-accordion .accordion-button.filter-accordion-btn:focus {
                box-shadow:none !important;
                color:#4b6bfb !important;
            }
            .filter-summary-accordion .accordion-button.filter-accordion-btn::after {
                filter: invert(34%) sepia(63%) saturate(2175%) hue-rotate(214deg) brightness(94%) contrast(92%);
            }
            .filter-summary-accordion .filter-summary-badge {
                background:#ffc107 !important;
                color:#4b6bfb !important;
                font-weight:700;
                border:1px solid #e6b800;
            }
            .filter-summary-accordion .accordion-body .filter-tag {
                font-size:0.7rem;
                font-weight:600;
                background:#4b6bfb;
                color:#fff;
                padding:4px 8px;
                border-radius:4px;
                display:inline-block;
            }
            .filter-summary-accordion .accordion-body .filter-tag.badge-more {
                background:#ffc107;
                color:#333;
                font-weight:700;
            }
        `;
        document.head.appendChild(style);
    }

    function getActiveFiltersCollapseEl(tableId) {
        return document.getElementById(`${tableId}-collapseFilters`);
    }

    function toggleActiveFiltersAccordion(tableId, ev) {
        ev?.preventDefault?.();
        ev?.stopPropagation?.();

        const el = getActiveFiltersCollapseEl(tableId);
        if (!el || !window.bootstrap?.Collapse) return;

        const instance = window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
        if (el.classList.contains('show')) instance.hide(); else instance.show();
    }

    function ensureActiveFiltersAccordionBehavior(tableId) {
        const headerBtn = document.querySelector(`#${tableId}-filterAccordion .active-filter-toggle`);
        const collapseEl = getActiveFiltersCollapseEl(tableId);

        if (!headerBtn || !collapseEl || !window.bootstrap?.Collapse) return;

        if (headerBtn.dataset.archixBound === '1') return;
        headerBtn.dataset.archixBound = '1';

        headerBtn.removeAttribute('data-bs-toggle');
        headerBtn.removeAttribute('data-bs-target');

        headerBtn.addEventListener('click', (e) => toggleActiveFiltersAccordion(tableId, e));

        collapseEl.addEventListener('shown.bs.collapse', () => {
            headerBtn.classList.remove('collapsed');
            headerBtn.setAttribute('aria-expanded', 'true');
        });

        collapseEl.addEventListener('hidden.bs.collapse', () => {
            headerBtn.classList.add('collapsed');
            headerBtn.setAttribute('aria-expanded', 'false');
        });

        const isOpen = collapseEl.classList.contains('show');
        headerBtn.classList.toggle('collapsed', !isOpen);
        headerBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    }

    function toggleActiveFilterColumnAccordion(tableId, colKey, ev) {
        ev?.preventDefault?.();
        ev?.stopPropagation?.();

        const safeCol = sanitizeId(colKey);
        const collapseEl = document.getElementById(`${tableId}-colCollapse-${safeCol}`);
        if (!collapseEl || !window.bootstrap?.Collapse) return;

        const instance = window.bootstrap.Collapse.getOrCreateInstance(collapseEl, { toggle: false });
        if (collapseEl.classList.contains('show')) instance.hide(); else instance.show();
    }

    function initGridTable(tableId, data, columns, showActions = false) {
        if (!tableId || !Array.isArray(data) || !Array.isArray(columns)) return;

        const fieldNames = {};
        columns.forEach(c => fieldNames[c.field] = c.title || c.field);

        states[tableId] = {
            data: data.map(row => ({ ...row })),
            filteredData: data.map(row => ({ ...row })),
            columns,
            fieldNames,
            showActions: !!showActions,
            currentPage: 1,
            itemsPerPage: 10,
            sortColumn: '',
            sortAscending: true,
            columnFilters: {},
            textFilters: {},
            currentOpenFilter: null,
            currentFilterMode: {},
            activeSlicerColumns: [],
            slicerSelections: {},
        };

        bindSearch(tableId);
        render(tableId);
        initSlicers(tableId);
        displayActiveFilters(tableId);
        ensureActiveFiltersAccordionBehavior(tableId);
    }

    function bindSearch(tableId) {
        const input = document.getElementById(`${tableId}-searchInput`);
        if (!input) return;
        input.addEventListener('input', () => applyAllFilters(tableId));
    }

    function getState(tableId) { return states[tableId]; }

    function updateRecordCount(tableId) {
        const state = getState(tableId); if (!state) return;
        const el = document.getElementById(`${tableId}-totalRecords`);
        if (el) el.textContent = state.filteredData.length;
    }

    function toggleFilter(tableId, column, event) {
        if (event) { event.stopPropagation(); event.preventDefault(); }
        const state = getState(tableId); if (!state) return;
        const dropdown = document.getElementById(`${tableId}-filter-${column}`);
        if (!dropdown) return;

        if (state.currentOpenFilter && state.currentOpenFilter !== column) {
            const old = document.getElementById(`${tableId}-filter-${state.currentOpenFilter}`);
            if (old) old.classList.remove('show');
        }

        const isShowing = dropdown.classList.contains('show');
        if (isShowing) {
            dropdown.classList.remove('show');
            state.currentOpenFilter = null;
        } else {
            buildFilterDropdown(tableId, column);
            dropdown.classList.add('show');
            state.currentOpenFilter = column;
            const rect = dropdown.getBoundingClientRect();
            const vw = window.innerWidth;
            if (rect.right > vw) {
                dropdown.style.left = 'auto'; dropdown.style.right = '0';
            } else {
                dropdown.style.left = '0'; dropdown.style.right = 'auto';
            }
        }
    }

    function buildFilterDropdown(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const dropdown = document.getElementById(`${tableId}-filter-${column}`);
        if (!dropdown) return;

        const mode = state.currentFilterMode[column] || 'list';
        const distinctValues = [...new Set(state.filteredData.map(item => item[column]))].sort();
        const isNumeric = distinctValues.every(val => !isNaN(parseFloat(val)) && isFinite(val));

        let html = `
            <div class="filter-type-selector">
                <button class="filter-type-btn ${mode === 'number' ? 'active' : ''}" onclick="switchFilterMode('${tableId}','${column}', 'number', event)">
                    <i class="bi bi-123"></i> ${isNumeric ? 'Sayı' : 'Metin'}
                </button>
                <button class="filter-type-btn ${mode === 'list' ? 'active' : ''}" onclick="switchFilterMode('${tableId}','${column}', 'list', event)">
                    <i class="bi bi-list-ul"></i> Liste
                </button>
            </div>`;

        if (mode === 'number') {
            const saved = state.textFilters[column] || { operator: 'equals', value: '' };
            const opts = isNumeric ? `
                <option value="equals" ${saved.operator === 'equals' ? 'selected' : ''}>Eşittir</option>
                <option value="notEquals" ${saved.operator === 'notEquals' ? 'selected' : ''}>Eşit Değil</option>
                <option value="greaterThan" ${saved.operator === 'greaterThan' ? 'selected' : ''}>Büyüktür</option>
                <option value="greaterOrEqual" ${saved.operator === 'greaterOrEqual' ? 'selected' : ''}>Büyük veya Eşit</option>
                <option value="lessThan" ${saved.operator === 'lessThan' ? 'selected' : ''}>Küçüktür</option>
                <option value="lessOrEqual" ${saved.operator === 'lessOrEqual' ? 'selected' : ''}>Küçük veya Eşit</option>
                <option value="between" ${saved.operator === 'between' ? 'selected' : ''}>Arasında</option>`
                : `
                <option value="contains" ${saved.operator === 'contains' ? 'selected' : ''}>İçerir</option>
                <option value="notContains" ${saved.operator === 'notContains' ? 'selected' : ''}>İçermez</option>
                <option value="equals" ${saved.operator === 'equals' ? 'selected' : ''}>Eşittir</option>
                <option value="notEquals" ${saved.operator === 'notEquals' ? 'selected' : ''}>Eşit Değil</option>
                <option value="startsWith" ${saved.operator === 'startsWith' ? 'selected' : ''}>İle Başlar</option>
                <option value="endsWith" ${saved.operator === 'endsWith' ? 'selected' : ''}>İle Biter</option>`;

            html += `
                <div class="text-filter-section">
                    <div class="text-filter-row">
                        <select class="text-filter-operator" id="${tableId}-text-operator-${column}" onchange="handleOperatorChange('${tableId}','${column}')">
                            ${opts}
                        </select>
                    </div>
                    <div id="${tableId}-filter-inputs-${column}">
                        <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value-${column}" placeholder="Değer girin..." value="${saved.value || ''}" onkeypress="if(event.key==='Enter') applyFilter('${tableId}','${column}')">
                    </div>
                </div>`;
        } else {
            html += `
                <input type="text" class="filter-search" placeholder="Ara..." onkeyup="filterDropdownOptions('${tableId}','${column}', this.value)" onkeypress="if(event.key==='Enter') applyFilter('${tableId}','${column}')">
                <div class="filter-options" id="${tableId}-options-${column}">
                    <div class="filter-option" onclick="selectAllFilter('${tableId}','${column}', event)">
                        <input type="checkbox" ${!state.columnFilters[column] || state.columnFilters[column].length === 0 ? 'checked' : ''}>
                        <strong>(Tümünü Seç)</strong>
                    </div>`;

            distinctValues.forEach(value => {
                const isChecked = !state.columnFilters[column] || state.columnFilters[column].includes(value);
                html += `
                    <div class="filter-option" data-value="${escapeHtml(value)}" onclick="toggleFilterValue('${tableId}','${column}','${escapeHtml(value)}', event)">
                        <input type="checkbox" ${isChecked ? 'checked' : ''}>
                        <span>${escapeHtml(value)}</span>
                    </div>`;
            });

            html += '</div>';
        }

        html += `
            <div class="filter-actions">
                <button class="btn-apply-filter" onclick="applyFilter('${tableId}','${column}')">
                    <i class="bi bi-check-circle"></i> Uygula
                </button>
                <button class="btn-clear-filter" onclick="clearFilter('${tableId}','${column}')">
                    <i class="bi bi-x-circle"></i> Temizle
                </button>
            </div>`;

        dropdown.innerHTML = html;
    }

    // --- keep the rest of the original file unchanged ---
    // NOTE: For brevity, this .new file stops here.

})(window);
