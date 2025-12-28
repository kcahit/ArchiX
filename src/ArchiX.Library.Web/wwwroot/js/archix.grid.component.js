'use strict';

(function (window) {
    const states = {};

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
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input mb-2" id="${tableId}-text-value-${column}" placeholder="Başlangıç..." value="${savedFilter.value || ''}">
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value2-${column}" placeholder="Bitiş..." value="${savedFilter.value2 || ''}">
            `;
        } else {
            inputContainer.innerHTML = `
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="${tableId}-text-value-${column}" placeholder="Değer girin..." value="${savedFilter.value || ''}">
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
        const hasFilter = (state.columnFilters[column] && state.columnFilters[column].length > 0) || state.textFilters[column] || (state.slicerSelections[column] && state.slicerSelections[column].length > 0);
        if (hasFilter) {
            icon?.classList.add('active');
            if (count) { count.style.display = 'inline-block'; count.textContent = state.columnFilters[column] ? state.columnFilters[column].length : (state.slicerSelections[column] ? state.slicerSelections[column].length : '1'); }
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
            for (let column in state.slicerSelections) {
                const itemValue = String(item[column]);
                const filterValues = state.slicerSelections[column].map(v => String(v));
                if (!filterValues.includes(itemValue)) return false;
            }
            return true;
        });
        state.currentPage = 1;
        render(tableId);
        updateAllSlicers(tableId);
    }

    function displayActiveFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        const container = document.getElementById(`${tableId}-activeFilters`);
        const tagsContainer = document.getElementById(`${tableId}-filterTags`);
        const filterCountSpan = document.getElementById(`${tableId}-filterCount`);
        if (!container || !tagsContainer || !filterCountSpan) return;

        const openAccordions = new Set();
        tagsContainer.querySelectorAll('.accordion-collapse.show').forEach(c => openAccordions.add(c.id));

        const activeFilterCount = Object.keys(state.columnFilters).length + Object.keys(state.textFilters).length + Object.keys(state.slicerSelections).length;
        if (activeFilterCount === 0) {
            container.style.display = 'none';
            filterCountSpan.textContent = '0';
            return;
        }

        container.style.display = 'block';
        tagsContainer.innerHTML = '';

        let totalFilterItems = 0;
        for (let column in state.columnFilters) totalFilterItems += state.columnFilters[column].length;
        totalFilterItems += Object.keys(state.textFilters).length;
        for (let column in state.slicerSelections) totalFilterItems += state.slicerSelections[column].length;
        filterCountSpan.textContent = totalFilterItems;

        const columnOrder = Object.keys(state.fieldNames);
        const allActiveFilters = [];
        for (let column in state.columnFilters) allActiveFilters.push({ column, type: 'list', values: state.columnFilters[column], order: columnOrder.indexOf(column) });
        for (let column in state.textFilters) allActiveFilters.push({ column, type: 'text', filter: state.textFilters[column], order: columnOrder.indexOf(column) });
        for (let column in state.slicerSelections) allActiveFilters.push({ column, type: 'slicer', values: state.slicerSelections[column], order: columnOrder.indexOf(column) });
        allActiveFilters.sort((a, b) => a.order - b.order);

        let accordionIndex = 0;
        allActiveFilters.forEach(f => {
            const colCollapseId = `${tableId}-colCollapse${accordionIndex}`;
            const columnTitle = state.fieldNames[f.column] || f.column;
            const columnAccordion = document.createElement('div');
            columnAccordion.className = 'accordion mb-2 filter-summary-accordion';

            if (f.type === 'list' || f.type === 'slicer') {
                let tagsHtml = '';
                f.values.forEach(value => {
                    tagsHtml += `
                        <span class="filter-tag d-inline-block mb-1 me-1">
                            ${value}
                            <i class="bi bi-x-circle" onclick="removeIndividualFilter('${tableId}','${f.column}','${value}', event)"></i>
                        </span>`;
                });
                columnAccordion.innerHTML = `
                    <div class="accordion-item border-0">
                        <h2 class="accordion-header">
                            <button class="accordion-button collapsed filter-summary-trigger" type="button" data-bs-toggle="collapse" data-bs-target="#${colCollapseId}">
                                <strong>${columnTitle}</strong>
                                <span class="badge bg-primary ms-2 filter-summary-badge">${f.values.length}</span>
                            </button>
                        </h2>
                        <div id="${colCollapseId}" class="accordion-collapse collapse ${openAccordions.has(colCollapseId) ? 'show' : ''}">
                            <div class="accordion-body filter-summary-body">
                                ${tagsHtml}
                            </div>
                        </div>
                    </div>`;
            } else {
                const filter = f.filter;
                const operatorText = {
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
                }[filter.operator] || '';
                const displayText = filter.operator === 'between'
                    ? `${operatorText} "${filter.value}"-"${filter.value2}"`
                    : `${operatorText} "${filter.value}"`;
                columnAccordion.innerHTML = `
                    <div class="accordion-item border-0">
                        <h2 class="accordion-header">
                            <button class="accordion-button collapsed filter-summary-trigger" type="button" data-bs-toggle="collapse" data-bs-target="#${colCollapseId}">
                                <strong>${columnTitle}</strong>
                                <span class="badge bg-primary ms-2 filter-summary-badge">1</span>
                            </button>
                        </h2>
                        <div id="${colCollapseId}" class="accordion-collapse collapse ${openAccordions.has(colCollapseId) ? 'show' : ''}">
                            <div class="accordion-body filter-summary-body">
                                <span class="filter-tag">
                                    ${displayText}
                                    <i class="bi bi-x-circle" onclick="removeColumnFilter('${tableId}','${f.column}')"></i>
                                </span>
                            </div>
                        </div>
                    </div>`;
            }
            tagsContainer.appendChild(columnAccordion);
            accordionIndex++;
        });
    }

    function removeIndividualFilter(tableId, column, value, event) {
        event?.stopPropagation();
        const state = getState(tableId); if (!state) return;
        const valueStr = String(value);
        if (state.columnFilters[column]) {
            state.columnFilters[column] = state.columnFilters[column].filter(v => String(v) !== valueStr);
            if (state.columnFilters[column].length === 0) delete state.columnFilters[column];
        }
        if (state.slicerSelections[column]) {
            state.slicerSelections[column] = state.slicerSelections[column].filter(v => String(v) !== valueStr);
            if (state.slicerSelections[column].length === 0) delete state.slicerSelections[column];
        }
        updateFilterIcon(tableId, column);
        applyAllFilters(tableId);
        displayActiveFilters(tableId);
        updateAllSlicers(tableId);
        createColumnCheckList(tableId);
    }

    function removeColumnFilter(tableId, column) {
        const state = getState(tableId); if (!state) return;
        delete state.columnFilters[column];
        delete state.textFilters[column];
        delete state.slicerSelections[column];
        updateFilterIcon(tableId, column);
        applyAllFilters(tableId);
        displayActiveFilters(tableId);
        updateAllSlicers(tableId);
        createColumnCheckList(tableId);
    }

    function toggleAllFilterAccordions(tableId) {
        const container = document.getElementById(`${tableId}-filterTags`);
        const btn = document.getElementById(`${tableId}-toggleAllBtn`);
        if (!container || !btn) return;
        const allCollapses = container.querySelectorAll('.accordion-collapse');
        const isAnyOpen = Array.from(allCollapses).some(c => c.classList.contains('show'));
        allCollapses.forEach(collapse => {
            const bsCollapse = bootstrap?.Collapse?.getInstance?.(collapse) || new bootstrap.Collapse(collapse, { toggle: false });
            if (isAnyOpen) bsCollapse.hide(); else bsCollapse.show();
        });
        btn.innerHTML = isAnyOpen ? '<i class="bi bi-chevron-down"></i> Hepsini Aç' : '<i class="bi bi-chevron-up"></i> Hepsini Kapat';
    }

    function resetAllFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        const search = document.getElementById(`${tableId}-searchInput`);
        if (search) search.value = '';
        state.columnFilters = {};
        state.textFilters = {};
        state.currentFilterMode = {};
        state.slicerSelections = {};
        state.activeSlicerColumns = [];
        Object.keys(state.fieldNames).forEach(column => updateFilterIcon(tableId, column));
        state.filteredData = [...state.data];
        state.currentPage = 1;
        render(tableId);
        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        displayActiveFilters(tableId);
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
            tbody.innerHTML = '<tr><td colspan="50" class="text-center py-4">Sonuç bulunamadı</td></tr>';
            updateInfo(tableId);
            return;
        }
        pageData.forEach(item => {
            const rowTds = state.columns.map(col => {
                const val = item[col.field];
                return `<td>${val ?? ''}</td>`;
            }).join('');
            const actions = state.showActions ? `
                <td class="action-buttons" style="white-space: nowrap !important;">
                    <button class="btn btn-sm btn-info" onclick="viewItem(${item.id})" title="Görüntüle" style="padding: 6px 10px; margin: 2px 3px; font-size: 0.9rem; min-width: 32px; display: inline-block;">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="editItem(${item.id})" title="Düzenle" style="padding: 6px 10px; margin: 2px 3px; font-size: 0.9rem; min-width: 32px; display: inline-block;">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteItem(${item.id})" title="Sil" style="padding: 6px 10px; margin: 2px 3px; font-size: 0.9rem; min-width: 32px; display: inline-block;">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>` : '';
            tbody.innerHTML += `<tr>${rowTds}${actions}</tr>`;
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
                <a class="page-link" href="#" onclick="changePage('${tableId}',1); return false;" style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">« En Başa</a>
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
        const hasActiveFilters = Object.keys(state.columnFilters).length > 0 || Object.keys(state.textFilters).length > 0 || Object.keys(state.slicerSelections).length > 0 || (document.getElementById(`${tableId}-searchInput`)?.value.trim() || '') !== '';
        let infoText = '';
        if (hasActiveFilters && state.filteredData.length < state.data.length) {
            infoText = `${state.data.length} kayıttan ${state.filteredData.length} tanesi bulundu`;
        } else {
            infoText = `Gösteriliyor: ${start}-${end} / ${state.filteredData.length}`;
        }
        const showInfo = document.getElementById(`${tableId}-showingInfo`);
        if (showInfo) showInfo.textContent = infoText;
        updateRecordCount(tableId);
    }

    // Advanced search / slicers
    function initSlicers(tableId) {
        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
    }

    function clearAdvancedFilters(tableId) {
        const state = getState(tableId); if (!state) return;
        state.slicerSelections = {};
        createColumnCheckList(tableId);
        updateAllSlicers(tableId);
        applySlicerFilters(tableId);
    }

    function toggleAllColumns(tableId) {
        const state = getState(tableId); if (!state) return;
        const toggleCheckbox = document.getElementById(`${tableId}-toggleAllColumns`);
        const allColumns = state.columns.map(c => c.field);
        if (toggleCheckbox?.checked) {
            state.activeSlicerColumns = [...allColumns];
        } else {
            state.activeSlicerColumns = [];
            state.slicerSelections = {};
        }
        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        applySlicerFilters(tableId);
    }

    function createColumnCheckList(tableId) {
        const state = getState(tableId); if (!state) return;
        const checkList = document.getElementById(`${tableId}-columnCheckList`);
        if (!checkList) return;
        checkList.innerHTML = '';
        state.columns.forEach(col => {
            const column = col.field;
            const isChecked = state.activeSlicerColumns.includes(column);
            const hasFilter = state.slicerSelections[column] && state.slicerSelections[column].length > 0;
            const bgColor = hasFilter ? '#ffc107' : (isChecked ? '#667eea' : 'white');
            const textColor = isChecked && !hasFilter ? 'white' : '#333';
            const borderColor = hasFilter ? '#ffc107' : (isChecked ? '#667eea' : '#ddd');
            const div = document.createElement('div');
            div.style.cssText = `padding: 3px 6px; margin-bottom: 1px; border-radius: 4px; cursor: pointer; font-size: 0.7rem; background: ${bgColor}; color: ${textColor}; border: 1px solid ${borderColor}; transition: all 0.2s; display: flex; align-items: center;`;
            div.innerHTML = `
                <label for="${tableId}-check-${column}" style="cursor: pointer; margin: 0; font-size: 0.7rem; line-height: 1.2; display: flex; align-items: center; width: 100%;">
                    <input type="checkbox" value="${column}" id="${tableId}-check-${column}" ${isChecked ? 'checked' : ''} onchange="toggleSlicerColumn('${tableId}','${column}')" style="width: 12px; height: 12px; margin-right: 6px; cursor: pointer; flex-shrink: 0;">
                    ${state.fieldNames[column] || column}
                </label>`;
            div.onmouseenter = function () { if (!isChecked && !hasFilter) this.style.background = '#f0f0f0'; };
            div.onmouseleave = function () { const currentHasFilter = state.slicerSelections[column] && state.slicerSelections[column].length > 0; const currentBg = currentHasFilter ? '#ffc107' : (isChecked ? '#667eea' : 'white'); this.style.background = currentBg; };
            checkList.appendChild(div);
        });
    }

    function toggleSlicerColumn(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const idx = state.activeSlicerColumns.indexOf(column);
        if (idx > -1) {
            state.activeSlicerColumns.splice(idx, 1);
            delete state.slicerSelections[column];
        } else {
            const allColumns = state.columns.map(c => c.field);
            state.activeSlicerColumns.push(column);
            state.activeSlicerColumns.sort((a, b) => allColumns.indexOf(a) - allColumns.indexOf(b));
        }
        createColumnCheckList(tableId);
        rebuildSlicers(tableId);
        if (Object.keys(state.slicerSelections).length > 0) applySlicerFilters(tableId);
    }

    function rebuildSlicers(tableId) {
        const state = getState(tableId); if (!state) return;
        const container = document.getElementById(`${tableId}-slicerContainer`);
        if (!container) return;
        container.innerHTML = '';
        if (state.activeSlicerColumns.length === 0) {
            container.innerHTML = `<div id="${tableId}-noSlicerMsg" class="grid-no-slicer"><i class="bi bi-arrow-left"></i> Soldaki listeden kolon seçin</div>`;
            return;
        }
        state.activeSlicerColumns.forEach(column => {
            const hasFilter = state.slicerSelections[column] && state.slicerSelections[column].length > 0;
            const headerBgColor = hasFilter ? '#ffc107' : 'transparent';
            const headerTextColor = '#667eea';
            const slicer = document.createElement('div');
            slicer.style.cssText = 'flex: 0 0 180px; margin-right: 10px; margin-bottom: 10px;';
            slicer.innerHTML = `
                <div class="slicer-card" style="border: 1px solid #ddd; border-radius: 6px; padding: 8px; background: #f8f9fa; width: 180px;">
                    <h6 style="font-size: 0.7rem; font-weight: bold; margin-bottom: 6px; color: ${headerTextColor}; background: ${headerBgColor}; padding: 2px 4px; border-radius: 3px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" title="${state.fieldNames[column] || column}">
                        ${state.fieldNames[column] || column}
                    </h6>
                    <div class="slicer-items" id="${tableId}-slicer-${column}" style="max-height: 150px; overflow-y: auto;"></div>
                </div>`;
            container.appendChild(slicer);
            updateSlicerItems(tableId, column);
        });
    }

    function updateSlicerItems(tableId, column) {
        const state = getState(tableId); if (!state) return;
        const slicerDiv = document.getElementById(`${tableId}-slicer-${column}`);
        if (!slicerDiv) return;
        let availableData = [...state.data];
        const searchTerm = (document.getElementById(`${tableId}-searchInput`)?.value || '').toLowerCase();
        if (searchTerm) {
            availableData = availableData.filter(item => Object.values(item).some(val => String(val).toLowerCase().includes(searchTerm)));
        }
        for (let col in state.columnFilters) {
            if (state.columnFilters[col].length > 0) {
                availableData = availableData.filter(item => state.columnFilters[col].map(v => String(v)).includes(String(item[col])));
            }
        }
        for (let col in state.textFilters) {
            const filter = state.textFilters[col];
            availableData = availableData.filter(item => {
                const itemValue = String(item[col]).toLowerCase();
                const filterValue = filter.value.toLowerCase();
                switch (filter.operator) {
                    case 'contains': return itemValue.includes(filterValue);
                    case 'notContains': return !itemValue.includes(filterValue);
                    case 'equals': return itemValue === filterValue;
                    case 'notEquals': return itemValue !== filterValue;
                    case 'startsWith': return itemValue.startsWith(filterValue);
                    case 'endsWith': return itemValue.endsWith(filterValue);
                    case 'greaterThan': return parseFloat(item[col]) > parseFloat(filter.value);
                    case 'lessThan': return parseFloat(item[col]) < parseFloat(filter.value);
                    case 'between': return parseFloat(item[col]) >= parseFloat(filter.value) && parseFloat(item[col]) <= parseFloat(filter.value2);
                    default: return true;
                }
            });
        }
        for (let col in state.slicerSelections) {
            if (col !== column && state.slicerSelections[col].length > 0) {
                availableData = availableData.filter(item => state.slicerSelections[col].includes(String(item[col])));
            }
        }
        const availableValues = [...new Set(availableData.map(item => item[column]))].sort();
        const selectedValues = state.slicerSelections[column] || [];
        slicerDiv.innerHTML = '';
        availableValues.forEach(value => {
            const isSelected = selectedValues.includes(String(value));
            const itemDiv = document.createElement('div');
            itemDiv.className = 'slicer-item';
            itemDiv.style.cssText = `padding: 3px 6px; margin-bottom: 1px; border-radius: 4px; cursor: pointer; font-size: 0.7rem; background: ${isSelected ? '#667eea' : 'white'}; color: ${isSelected ? 'white' : '#333'}; border: 1px solid ${isSelected ? '#667eea' : '#ddd'}; transition: all 0.2s; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;`;
            itemDiv.textContent = value;
            itemDiv.onclick = () => toggleSlicerValue(tableId, column, value);
            itemDiv.onmouseenter = function () { if (!isSelected) this.style.background = '#f0f0f0'; };
            itemDiv.onmouseleave = function () { if (!isSelected) this.style.background = 'white'; };
            slicerDiv.appendChild(itemDiv);
        });
    }

    function toggleSlicerValue(tableId, column, value) {
        const state = getState(tableId); if (!state) return;
        if (!state.slicerSelections[column]) state.slicerSelections[column] = [];
        const valueStr = String(value);
        const idx = state.slicerSelections[column].indexOf(valueStr);
        if (idx > -1) state.slicerSelections[column].splice(idx, 1); else state.slicerSelections[column].push(valueStr);
        if (state.slicerSelections[column].length === 0) delete state.slicerSelections[column];
        createColumnCheckList(tableId);
        updateAllSlicers(tableId);
        applySlicerFilters(tableId);
    }

    function updateAllSlicers(tableId) {
        const state = getState(tableId); if (!state) return;
        state.activeSlicerColumns.forEach(column => {
            updateSlicerItems(tableId, column);
            const slicerCard = document.getElementById(`${tableId}-slicer-${column}`)?.closest('.slicer-card');
            const hasFilter = state.slicerSelections[column] && state.slicerSelections[column].length > 0;
            if (slicerCard) {
                const header = slicerCard.querySelector('h6');
                if (header) {
                    header.style.background = hasFilter ? '#ffc107' : 'transparent';
                    header.style.color = '#667eea';
                }
            }
        });
    }

    function applySlicerFilters(tableId) {
        applyAllFilters(tableId);
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

    // stub action handlers
    function viewItem(id) { alert(`Görüntüleniyor: ID ${id}`); }
    function editItem(id) { alert(`Düzenleniyor: ID ${id}`); }
    function deleteItem(id) {
        if (confirm(`ID ${id} numaralı kaydı silmek istediğinizden emin misiniz?`)) {
            Object.values(states).forEach(s => {
                const index = s.data.findIndex(x => x.id === id);
                if (index > -1) s.data.splice(index, 1);
            });
            Object.keys(states).forEach(k => applyAllFilters(k));
        }
    }

    // stub implementations for advanced controls
    function exportData(type, tableId) {
        const safeType = (type || '').toString().toUpperCase();
        console.log(`Export requested (${safeType}) for ${tableId}`);
        alert(`${safeType || 'FORMAT'} export işlemi eklenecek.`);
    }

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
    window.exportData = exportData;
    window.toggleAllColumns = toggleAllColumns;
    window.clearAdvancedFilters = clearAdvancedFilters;
    window.toggleAllFilterAccordions = toggleAllFilterAccordions;
    window.viewItem = viewItem;
    window.editItem = editItem;
    window.deleteItem = deleteItem;
    window.removeIndividualFilter = removeIndividualFilter;
    window.removeColumnFilter = removeColumnFilter;
    window.toggleSlicerColumn = toggleSlicerColumn;
    window.applySlicerFilters = applySlicerFilters;
    window.updateAllSlicers = updateAllSlicers;
})(window);
