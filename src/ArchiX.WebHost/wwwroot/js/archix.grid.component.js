'use strict';

(function (window) {
    const states = {};

    function initGridTable(tableId, data, columns) {
        if (!tableId || !Array.isArray(data) || !Array.isArray(columns)) return;
        const fieldNames = {};
        columns.forEach(c => fieldNames[c.field] = c.title || c.field);

        states[tableId] = {
            data: data.map(row => ({ ...row })),
            filteredData: data.map(row => ({ ...row })),
            columns,
            fieldNames,
            currentPage: 1,
            itemsPerPage: 10,
            sortColumn: '',
            sortAscending: true,
            columnFilters: {},
            textFilters: {},
            currentOpenFilter: null,
            currentFilterMode: {},
        };

        bindSearch(tableId);
        render(tableId);
    }

    function bindSearch(tableId) {
        const input = document.getElementById(`${tableId}-searchInput`);
        if (!input) return;
        input.addEventListener('input', () => {
            applyAllFilters(tableId);
        });
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
                    <i class="bi bi-123"></i> ${isNumeric ? 'Sayý' : 'Metin'}
                </button>
                <button class="filter-type-btn ${mode === 'list' ? 'active' : ''}" onclick="switchFilterMode('${tableId}','${column}', 'list', event)">
                    <i class="bi bi-list-ul"></i> Liste
                </button>
            </div>`;

        if (mode === 'number') {
            const saved = state.textFilters[column] || { operator: 'equals', value: '' };
            const opts = isNumeric ? `
                <option value="equals" ${saved.operator === 'equals' ? 'selected' : ''}>Eþittir</option>
                <option value="notEquals" ${saved.operator === 'notEquals' ? 'selected' : ''}>Eþit Deðil</option>
                <option value="greaterThan" ${saved.operator === 'greaterThan' ? 'selected' : ''}>Büyüktür</option>
                <option value="greaterOrEqual" ${saved.operator === 'greaterOrEqual' ? 'selected' : ''}>Büyük veya Eþit</option>
                <option value="lessThan" ${saved.operator === 'lessThan' ? 'selected' : ''}>Küçüktür</option>
                <option value="lessOrEqual" ${saved.operator === 'lessOrEqual' ? 'selected' : ''}>Küçük veya Eþit</option>
                <option value="between" ${saved.operator === 'between' ? 'selected' : ''}>Arasýnda</option>`
                : `
                <option value="contains" ${saved.operator === 'contains' ? 'selected' : ''}>Ýçerir</option>
                <option value="notContains" ${saved.operator === 'notContains' ? 'selected' : ''}>Ýçermez</option>
                <option value="equals" ${saved.operator === 'equals' ? 'selected' : ''}>Eþittir</option>
                <option value="notEquals" ${saved.operator === 'notEquals' ? 'selected' : ''}>Eþit Deðil</option>
                <option value="startsWith" ${saved.operator === 'startsWith' ? 'selected' : ''}>Ýle Baþlar</option>
                <option value="endsWith" ${saved.operator === 'endsWith' ? 'selected' : ''}>Ýle Biter</option>`;

            html += `
                <div class="text-filter-section">
                    <div class="text-filter-row">
                        <select class="text-filter-operator" id="${tableId}-text-operator-${column}" onchange="handleOperatorChange('${tableId}','${column}')">
                            ${opts}
                        </select>
                    </div>
                    <div id="${tableId}-filter-inputs-${column}">
                        <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value-${column}" placeholder="Deðer girin..." value="${saved.value || ''}" onkeypress="if(event.key==='Enter') applyFilter('${tableId}','${column}')">
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
                    <div class="filter-option" data-value="${value}" onclick="toggleFilterValue('${tableId}','${column}','${value}', event)">
                        <input type="checkbox" ${isChecked ? 'checked' : ''}>
                        <span>${value}</span>
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

    function switchFilterMode(tableId, column, mode, event) {
        if (event) { event.stopPropagation(); event.preventDefault(); }
        const state = getState(tableId); if (!state) return;
        state.currentFilterMode[column] = mode;
        buildFilterDropdown(tableId, column);
    }

    function handleOperatorChange(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const operator = document.getElementById(`${tableId}-text-operator-${column}`)?.value;
        const inputContainer = document.getElementById(`${tableId}-filter-inputs-${column}`);
        const savedFilter = state.textFilters[column] || {};
        const distinctValues = [...new Set(state.data.map(item => item[column]))];
        const isNumeric = distinctValues.every(val => !isNaN(parseFloat(val)) && isFinite(val));
        if (!inputContainer) return;
        if (operator === 'between') {
            inputContainer.innerHTML = `
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input mb-2" id="${tableId}-text-value-${column}" placeholder="Baþlangýç..." value="${savedFilter.value || ''}">
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value2-${column}" placeholder="Bitiþ..." value="${savedFilter.value2 || ''}">
            `;
        } else {
            inputContainer.innerHTML = `
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value-${column}" placeholder="Deðer girin..." value="${savedFilter.value || ''}">
            `;
        }
    }

    function filterDropdownOptions(tableId, column, searchTerm) {
        const options = document.querySelectorAll(`#${tableId}-options-${column} .filter-option`);
        const term = (searchTerm || '').toLowerCase();
        options.forEach(option => {
            const text = option.textContent.toLowerCase();
            option.style.display = text.includes(term) ? 'flex' : 'none';
        });
    }

    function selectAllFilter(tableId, column, event) {
        if (event) {
            event.stopPropagation();
            if (event.target.type === 'checkbox') {
                const isChecked = event.target.checked;
                const options = document.querySelectorAll(`#${tableId}-options-${column} .filter-option input[type="checkbox"]`);
                options.forEach(cb => cb.checked = isChecked);
                return;
            }
        }
        const options = document.querySelectorAll(`#${tableId}-options-${column} .filter-option input[type="checkbox"]`);
        const selectAll = options[0];
        const isChecked = !selectAll.checked;
        options.forEach(cb => cb.checked = isChecked);
    }

    function toggleFilterValue(tableId, column, value, event) {
        if (event) {
            event.stopPropagation();
            if (event.target.type === 'checkbox') return;
        }
        const checkbox = event.currentTarget.querySelector('input[type="checkbox"]');
        if (checkbox) checkbox.checked = !checkbox.checked;
    }

    function applyFilter(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const mode = state.currentFilterMode[column] || 'list';
        if (mode === 'number') {
            const operator = document.getElementById(`${tableId}-text-operator-${column}`)?.value;
            const value = document.getElementById(`${tableId}-text-value-${column}`)?.value || '';
            if (operator === 'between') {
                const value2 = document.getElementById(`${tableId}-text-value2-${column}`)?.value || '';
                if (value.trim() && value2.trim()) {
                    state.textFilters[column] = { operator, value, value2 };
                    delete state.columnFilters[column];
                } else { delete state.textFilters[column]; }
            } else {
                if (value.trim()) {
                    state.textFilters[column] = { operator, value };
                    delete state.columnFilters[column];
                } else { delete state.textFilters[column]; }
            }
        } else {
            const selectedValues = [];
            const checkboxes = document.querySelectorAll(`#${tableId}-options-${column} .filter-option[data-value] input[type="checkbox"]:checked`);
            checkboxes.forEach(cb => {
                const v = cb.parentElement.dataset.value;
                if (v !== undefined && v !== null) selectedValues.push(v);
            });
            const allValues = [...new Set(state.filteredData.map(item => item[column]))];
            if (selectedValues.length === 0 || selectedValues.length === allValues.length) {
                delete state.columnFilters[column];
            } else {
                state.columnFilters[column] = selectedValues;
            }
            delete state.textFilters[column];
        }
        updateFilterIcon(tableId, column);
        document.getElementById(`${tableId}-filter-${column}`)?.classList.remove('show');
        state.currentOpenFilter = null;
        applyAllFilters(tableId);
        displayActiveFilters(tableId);
    }

    function clearFilter(tableId, column) {
        const state = getState(tableId); if (!state) return;
        delete state.columnFilters[column];
        delete state.textFilters[column];
        updateFilterIcon(tableId, column);
        document.getElementById(`${tableId}-filter-${column}`)?.classList.remove('show');
        state.currentOpenFilter = null;
        applyAllFilters(tableId);
        displayActiveFilters(tableId);
    }

    function updateFilterIcon(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const icon = document.querySelector(`#${tableId}-filter-${column}`)?.previousElementSibling;
        const count = document.getElementById(`${tableId}-count-${column}`);
        const hasFilter = (state.columnFilters[column] && state.columnFilters[column].length > 0) || state.textFilters[column];
        if (hasFilter) {
            icon?.classList.add('active');
            if (count) { count.style.display = 'inline-block'; count.textContent = state.columnFilters[column] ? state.columnFilters[column].length : '1'; }
        } else {
            icon?.classList.remove('active');
            if (count) count.style.display = 'none';
        }
    }

    function applyAllFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        const searchTerm = (document.getElementById(`${tableId}-searchInput`)?.value || '').toLocaleLowerCase('tr-TR');
        state.filteredData = state.data.filter(item => {
            if (searchTerm) {
                const matchSearch = Object.values(item).some(val => String(val).toLocaleLowerCase('tr-TR').includes(searchTerm));
                if (!matchSearch) return false;
            }
            for (let column in state.columnFilters) {
                const itemValue = String(item[column]);
                const filterValues = state.columnFilters[column].map(v => String(v));
                if (!filterValues.includes(itemValue)) return false;
            }
            for (let column in state.textFilters) {
                const filter = state.textFilters[column];
                const itemValue = String(item[column]).toLowerCase();
                const filterValue = filter.value.toLowerCase();
                let match = false;
                switch (filter.operator) {
                    case 'contains': match = itemValue.includes(filterValue); break;
                    case 'notContains': match = !itemValue.includes(filterValue); break;
                    case 'equals': match = itemValue === filterValue; break;
                    case 'notEquals': match = itemValue !== filterValue; break;
                    case 'startsWith': match = itemValue.startsWith(filterValue); break;
                    case 'endsWith': match = itemValue.endsWith(filterValue); break;
                    case 'greaterThan': match = parseFloat(item[column]) > parseFloat(filter.value); break;
                    case 'greaterOrEqual': match = parseFloat(item[column]) >= parseFloat(filter.value); break;
                    case 'lessThan': match = parseFloat(item[column]) < parseFloat(filter.value); break;
                    case 'lessOrEqual': match = parseFloat(item[column]) <= parseFloat(filter.value); break;
                    case 'between':
                        const numValue = parseFloat(item[column]);
                        const min = parseFloat(filter.value);
                        const max = parseFloat(filter.value2);
                        match = numValue >= min && numValue <= max;
                        break;
                }
                if (!match) return false;
            }
            return true;
        });
        state.currentPage = 1;
        render(tableId);
    }

    function displayActiveFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        document.querySelectorAll(`#${tableId}-activeFilters .filter-count`).forEach(el => {
            const column = el.id.replace(`${tableId}-count-`, '');
            updateFilterIcon(tableId, column);
        });
    }

    function resetAllFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        const search = document.getElementById(`${tableId}-searchInput`);
        if (search) search.value = '';
        state.columnFilters = {};
        state.textFilters = {};
        state.currentFilterMode = {};
        Object.keys(state.fieldNames).forEach(column => updateFilterIcon(tableId, column));
        state.filteredData = [...state.data];
        state.currentPage = 1;
        render(tableId);
    }

    function sortTable(tableId, column) {
        const state = getState(tableId); if (!state) return;
        if (state.sortColumn === column) { state.sortAscending = !state.sortAscending; }
        else { state.sortColumn = column; state.sortAscending = true; }
        state.filteredData.sort((a, b) => {
            let valA = a[column]; let valB = b[column];
            if (typeof valA === 'string') { valA = valA.toLowerCase(); valB = valB.toLowerCase(); }
            if (state.sortAscending) return valA > valB ? 1 : valA < valB ? -1 : 0;
            return valA < valB ? 1 : valA > valB ? -1 : 0;
        });
        document.querySelectorAll(`#${tableId}-dataTable .header-text`).forEach(header => {
            const icon = header.querySelector('.sort-icon');
            if (icon) { header.classList.remove('sorted'); icon.className = 'bi bi-arrow-down-up sort-icon'; }
        });
        const activeHeader = document.querySelector(`#${tableId}-dataTable [onclick="sortTable('${tableId}','${column}')"]`);
        if (activeHeader) {
            const icon = activeHeader.querySelector('.sort-icon');
            activeHeader.classList.add('sorted');
            icon.className = state.sortAscending ? 'bi bi-arrow-up sort-icon' : 'bi bi-arrow-down sort-icon';
        }
        render(tableId);
    }

    function render(tableId) {
        const state = getState(tableId); if (!state) return;
        const tbody = document.getElementById(`${tableId}-tableBody`);
        if (!tbody) return;
        tbody.innerHTML = '';
        const start = (state.currentPage - 1) * state.itemsPerPage;
        const end = start + state.itemsPerPage;
        const pageData = state.filteredData.slice(start, end);
        if (pageData.length === 0) {
            tbody.innerHTML = '<tr><td colspan="50" class="text-center py-4">Sonuç bulunamadý</td></tr>';
            updateInfo(tableId);
            return;
        }
        pageData.forEach(item => {
            const rowTds = state.columns.map(col => {
                const val = item[col.field];
                return `<td>${val ?? ''}</td>`;
            }).join('');
            tbody.innerHTML += `<tr>${rowTds}</tr>`;
        });
        updatePagination(tableId);
        updateInfo(tableId);
    }

    function updatePagination(tableId) {
        const state = getState(tableId); if (!state) return;
        const totalPages = Math.ceil(state.filteredData.length / state.itemsPerPage) || 1;
        const pagination = document.getElementById(`${tableId}-pagination`);
        if (!pagination) return;
        pagination.innerHTML = '';
        const prevDisabled = state.currentPage === 1 ? 'disabled' : '';
        const nextDisabled = state.currentPage === totalPages || totalPages === 0 ? 'disabled' : '';
        pagination.innerHTML += `
            <li class="page-item ${prevDisabled}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage('${tableId}',1); return false;" style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">« En Baþa</a>
            </li>`;
        pagination.innerHTML += `
            <li class="page-item ${prevDisabled}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage('${tableId}',${state.currentPage - 1}); return false;" style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">‹ Önceki</a>
            </li>`;
        pagination.innerHTML += `
            <li class="page-item" style="display: flex; align-items: center; margin-right: 4px;">
                <input type="number" id="${tableId}-pageNumberInput" class="form-control form-control-sm" value="${state.currentPage}" min="1" max="${totalPages}"
                       onkeypress="if(event.key==='Enter') goToPage('${tableId}')" onblur="goToPage('${tableId}')"
                       style="width: 55px; text-align: center; font-size: 0.85rem; padding: 0 8px; border-radius: 6px; height: 36px; border: 2px solid #667eea; color: #667eea; font-weight: 600; box-sizing: border-box;">
            </li>
            <li class="page-item" style="display: flex; align-items: center; margin-right: 8px;">
                <span style="font-size: 0.85rem; color: #666; padding: 0 4px; height: 36px; display: flex; align-items: center; font-weight: 600;">/ ${totalPages}</span>
            </li>`;
        pagination.innerHTML += `
            <li class="page-item ${nextDisabled}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage('${tableId}',${state.currentPage + 1}); return false;" style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">Sonraki ›</a>
            </li>`;
        pagination.innerHTML += `
            <li class="page-item ${nextDisabled}">
                <a class="page-link" href="#" onclick="changePage('${tableId}',${totalPages}); return false;" style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">En Son »</a>
            </li>`;
    }

    function goToPage(tableId) {
        const state = getState(tableId); if (!state) return;
        const input = document.getElementById(`${tableId}-pageNumberInput`);
        let pageNum = parseInt(input?.value);
        const totalPages = Math.ceil(state.filteredData.length / state.itemsPerPage) || 1;
        if (isNaN(pageNum) || pageNum < 1) pageNum = 1;
        else if (pageNum > totalPages) pageNum = totalPages;
        if (input) input.value = pageNum;
        changePage(tableId, pageNum);
    }

    function changePage(tableId, page) {
        const state = getState(tableId); if (!state) return false;
        const totalPages = Math.ceil(state.filteredData.length / state.itemsPerPage) || 1;
        if (page >= 1 && page <= totalPages) {
            state.currentPage = page;
            render(tableId);
        }
        return false;
    }

    function changeItemsPerPage(tableId) {
        const state = getState(tableId); if (!state) return;
        const sel = document.getElementById(`${tableId}-itemsPerPageSelect`);
        if (!sel) return;
        state.itemsPerPage = parseInt(sel.value) || 10;
        state.currentPage = 1;
        render(tableId);
    }

    function updateInfo(tableId) {
        const state = getState(tableId); if (!state) return;
        const start = state.filteredData.length === 0 ? 0 : (state.currentPage - 1) * state.itemsPerPage + 1;
        const end = Math.min(state.currentPage * state.itemsPerPage, state.filteredData.length);
        const hasActiveFilters = Object.keys(state.columnFilters).length > 0 || Object.keys(state.textFilters).length > 0 || (document.getElementById(`${tableId}-searchInput`)?.value.trim() || '') !== '';
        let infoText = '';
        if (hasActiveFilters && state.filteredData.length < state.data.length) {
            infoText = `${state.data.length} kayýttan ${state.filteredData.length} tanesi bulundu`;
        } else {
            infoText = `Gösteriliyor: ${start}-${end} / ${state.filteredData.length}`;
        }
        const showInfo = document.getElementById(`${tableId}-showingInfo`);
        if (showInfo) showInfo.textContent = infoText;
        updateRecordCount(tableId);
    }

    // Click outside to close filters
    document.addEventListener('click', function (e) {
        Object.keys(states).forEach(tableId => {
            if (!e.target.closest('.filter-dropdown') && !e.target.closest('.filter-icon')) {
                document.querySelectorAll(`#${tableId}-dataTable .filter-dropdown`).forEach(dd => dd.classList.remove('show'));
                const state = getState(tableId); if (state) state.currentOpenFilter = null;
            }
        });
    });

    // expose globals
    window.initGridTable = initGridTable;
    window.toggleFilter = toggleFilter;
    window.switchFilterMode = switchFilterMode;
    window.handleOperatorChange = handleOperatorChange;
    window.applyFilter = applyFilter;
    window.clearFilter = clearFilter;
    window.filterDropdownOptions = filterDropdownOptions;
    window.selectAllFilter = selectAllFilter;
    window.toggleFilterValue = toggleFilterValue;
    window.applyAllFilters = applyAllFilters;
    window.resetAllFilters = resetAllFilters;
    window.sortTable = sortTable;
    window.changePage = changePage;
    window.changeItemsPerPage = changeItemsPerPage;
    window.goToPage = goToPage;
})(window);
