'use strict';

(function (window) {
    const states = {};

    const escapeHtml = (value) => String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/\"/g, '&quot;')
        .replace(/'/g, '&#039;');

    function getState(tableId) { return states[tableId]; }

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

    function encodeReturnContext(obj) {
        try {
            const json = JSON.stringify(obj);
            return btoa(unescape(encodeURIComponent(json)));
        } catch (e) {
            return '';
        }
    }

    function getReturnContext(tableId) {
        const state = getState(tableId);

        const input = document.getElementById(`${tableId}-searchInput`);
        const search = (input?.value ?? '').toString();

        return encodeReturnContext({
            search: search,
            page: state?.currentPage ?? 1,
            itemsPerPage: state?.itemsPerPage ?? 10
        });
    }

    // Issue #36 / 1.2.3 + 1.2.6
    function editItem(tableId, id) {
        const returnContext = getReturnContext(tableId);

        const url = `/Raporlar/FormRecordDetail?id=${encodeURIComponent(id ?? '')}`
            + (returnContext ? `&returnContext=${encodeURIComponent(returnContext)}` : '');

        window.location.href = url;
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

    function updateRecordCount(tableId) {
        const state = getState(tableId); if (!state) return;
        const el = document.getElementById(`${tableId}-totalRecords`);
        if (el) el.textContent = state.filteredData.length;
    }

    function setShowingInfo(tableId) {
        const state = getState(tableId); if (!state) return;
        const el = document.getElementById(`${tableId}-showingInfo`);
        if (!el) return;

        const total = state.filteredData.length;
        if (total === 0) {
            el.textContent = 'Gösteriliyor: 0-0 / 0';
            return;
        }

        const start = (state.currentPage - 1) * state.itemsPerPage + 1;
        const end = Math.min(state.currentPage * state.itemsPerPage, total);
        el.textContent = `Gösteriliyor: ${start}-${end} / ${total}`;
    }

    function renderPagination(tableId) {
        const state = getState(tableId); if (!state) return;
        const ul = document.getElementById(`${tableId}-pagination`);
        if (!ul) return;

        const total = state.filteredData.length;
        const pageCount = Math.max(1, Math.ceil(total / state.itemsPerPage));
        state.currentPage = Math.min(Math.max(1, state.currentPage), pageCount);

        const mk = (label, page, disabled, active) => {
            const li = document.createElement('li');
            li.className = `page-item ${disabled ? 'disabled' : ''} ${active ? 'active' : ''}`;

            const a = document.createElement('a');
            a.className = 'page-link';
            a.href = '#';
            a.textContent = label;
            a.addEventListener('click', (e) => {
                e.preventDefault();
                if (disabled) return;
                state.currentPage = page;
                render(tableId);
            });

            li.appendChild(a);
            return li;
        };

        ul.innerHTML = '';
        ul.appendChild(mk('‹', state.currentPage - 1, state.currentPage === 1, false));

        const maxButtons = 7;
        let start = Math.max(1, state.currentPage - Math.floor(maxButtons / 2));
        let end = Math.min(pageCount, start + maxButtons - 1);
        start = Math.max(1, end - maxButtons + 1);

        for (let p = start; p <= end; p++) {
            ul.appendChild(mk(String(p), p, false, p === state.currentPage));
        }

        ul.appendChild(mk('›', state.currentPage + 1, state.currentPage === pageCount, false));
    }

    function renderActionsCell(tableId, row) {
        const state = getState(tableId);
        if (!state?.showActions) return '';

        const id = row?.id;
        const canEdit = !!state.isFormOpenEnabled;

        let html = '<td class="action-buttons">';
        html += `<button type="button" class="btn btn-sm btn-outline-primary" onclick="viewItem('${tableId}','${id}')" title="Görüntüle"><i class="bi bi-eye"></i></button>`;

        // REQUIREMENT: IsFormOpenEnabled=0 => do NOT render Edit/Değiştir.
        if (canEdit) {
            html += `<button type="button" class="btn btn-sm btn-outline-secondary" onclick="editItem('${tableId}','${id}')" title="Değiştir"><i class="bi bi-pencil"></i></button>`;
        }

        html += `<button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteItem('${tableId}','${id}')" title="Sil"><i class="bi bi-trash"></i></button>`;
        html += '</td>';
        return html;
    }

    function renderRowHtml(tableId, row) {
        const state = getState(tableId); if (!state) return '';

        const tds = state.columns.map(c => {
            const v = row?.[c.field];
            return `<td>${escapeHtml(v)}</td>`;
        }).join('');

        return `<tr>${tds}${renderActionsCell(tableId, row)}</tr>`;
    }

    function renderTableBody(tableId) {
        const state = getState(tableId); if (!state) return;
        const tbody = document.getElementById(`${tableId}-tableBody`);
        if (!tbody) return;

        const start = (state.currentPage - 1) * state.itemsPerPage;
        const rows = state.filteredData.slice(start, start + state.itemsPerPage);
        tbody.innerHTML = rows.map(r => renderRowHtml(tableId, r)).join('');
    }

    function render(tableId) {
        updateRecordCount(tableId);
        renderTableBody(tableId);
        renderPagination(tableId);
        setShowingInfo(tableId);
    }

    function applyAllFilters(tableId) {
        const state = getState(tableId); if (!state) return;

        const input = document.getElementById(`${tableId}-searchInput`);
        const term = (input?.value ?? '').toLocaleLowerCase('tr-TR');

        state.filteredData = state.data.filter(item => {
            if (!term) return true;
            return Object.values(item || {}).some(v => String(v ?? '').toLocaleLowerCase('tr-TR').includes(term));
        });

        state.currentPage = 1;
        render(tableId);
    }

    function bindSearch(tableId) {
        const input = document.getElementById(`${tableId}-searchInput`);
        if (!input) return;
        if (input.dataset.archixBound === '1') return;
        input.dataset.archixBound = '1';

        input.addEventListener('input', () => applyAllFilters(tableId));
    }

    function bindItemsPerPage(tableId) {
        const sel = document.getElementById(`${tableId}-itemsPerPageSelect`);
        const state = getState(tableId); if (!sel || !state) return;

        if (sel.dataset.archixBound === '1') return;
        sel.dataset.archixBound = '1';

        sel.addEventListener('change', () => {
            const v = parseInt(sel.value, 10);
            state.itemsPerPage = Number.isFinite(v) && v > 0 ? v : 10;
            state.currentPage = 1;
            render(tableId);
        });
    }

    function initGridTable(tableId, data, columns, showActions = false, isFormOpenEnabled = false) {
        if (!tableId || !Array.isArray(data) || !Array.isArray(columns)) return;

        states[tableId] = {
            data: data.map(row => ({ ...row })),
            filteredData: data.map(row => ({ ...row })),
            columns,
            showActions: !!showActions,
            isFormOpenEnabled: !!isFormOpenEnabled,
            currentPage: 1,
            itemsPerPage: 10,
        };

        bindSearch(tableId);
        bindItemsPerPage(tableId);
        render(tableId);
    }

    // Public API used by Razor component
    window.initGridTable = initGridTable;

    // Actions (used by generated buttons)
    window.viewItem = viewItem;
    window.editItem = editItem;
    window.deleteItem = deleteItem;

    // Hooks referenced by markup (safe defaults)
    window.resetAllFilters = window.resetAllFilters || function (tableId) {
        const input = document.getElementById(`${tableId}-searchInput`);
        if (input) input.value = '';
        applyAllFilters(tableId);
    };
})(window);