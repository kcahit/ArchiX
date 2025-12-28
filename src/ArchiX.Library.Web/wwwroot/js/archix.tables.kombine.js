(function (window, $) {
    'use strict';

    const rawData = window.kombineDataRaw || [];
    const data = rawData.map(item => ({
        id: item.Id,
        name: item.Name,
        department: item.Department,
        position: item.Position,
        city: item.City,
        salary: item.Salary,
        status: item.Status
    }));

    let filteredData = [...data];
    let currentPage = 1;
    let itemsPerPage = 10;
    let sortColumn = '';
    let sortAscending = true;
    let columnFilters = {};
    let textFilters = {};
    let currentOpenFilter = null;
    let currentFilterMode = {};

    const fieldNames = {
        id: 'ID',
        name: 'İsim',
        department: 'Departman',
        position: 'Pozisyon',
        city: 'Şehir',
        salary: 'Maaş',
        status: 'Durum'
    };

    $(function () {
        updateRecordCount();
        renderTable();
        initPivot();
    });

    function initPivot() {
        if (!$('#pivotContainer').length || typeof $.fn.pivotUI !== 'function') return;
        $('#pivotContainer').pivotUI(rawData, {
            rows: ['Department', 'City'],
            cols: ['Status'],
            vals: ['Salary'],
            aggregatorName: 'Sum',
            rendererName: 'Table Barchart'
        }, true, 'tr');
    }

    function updateRecordCount() {
        const el = document.getElementById('totalRecords');
        if (el) el.textContent = filteredData.length;
    }

    function toggleFilter(column, event) {
        if (event) {
            event.stopPropagation();
            event.preventDefault();
        }
        const dropdown = document.getElementById(`filter-${column}`);
        if (!dropdown) return;

        if (currentOpenFilter && currentOpenFilter !== column) {
            const oldDropdown = document.getElementById(`filter-${currentOpenFilter}`);
            if (oldDropdown) oldDropdown.classList.remove('show');
        }

        const isShowing = dropdown.classList.contains('show');
        if (isShowing) {
            dropdown.classList.remove('show');
            currentOpenFilter = null;
        } else {
            buildFilterDropdown(column);
            dropdown.classList.add('show');
            currentOpenFilter = column;

            const rect = dropdown.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            if (rect.right > viewportWidth) {
                dropdown.style.left = 'auto';
                dropdown.style.right = '0';
            } else {
                dropdown.style.left = '0';
                dropdown.style.right = 'auto';
            }
        }
    }

    function buildFilterDropdown(column) {
        const dropdown = document.getElementById(`filter-${column}`);
        const mode = currentFilterMode[column] || 'list';
        const distinctValues = [...new Set(filteredData.map(item => item[column]))].sort();
        const isNumeric = distinctValues.every(val => !isNaN(parseFloat(val)) && isFinite(val));

        let html = `
            <div class="filter-type-selector">
                <button class="filter-type-btn ${mode === 'number' ? 'active' : ''}" onclick="switchFilterMode('${column}', 'number', event)">
                    <i class="bi bi-123"></i> ${isNumeric ? 'Sayı' : 'Metin'}
                </button>
                <button class="filter-type-btn ${mode === 'list' ? 'active' : ''}" onclick="switchFilterMode('${column}', 'list', event)">
                    <i class="bi bi-list-ul"></i> Liste
                </button>
            </div>
        `;

        if (mode === 'number') {
            const savedFilter = textFilters[column] || { operator: 'equals', value: '' };
            const operatorOptions = isNumeric
                ? `
                    <option value="equals" ${savedFilter.operator === 'equals' ? 'selected' : ''}>Eşittir</option>
                    <option value="notEquals" ${savedFilter.operator === 'notEquals' ? 'selected' : ''}>Eşit Değil</option>
                    <option value="greaterThan" ${savedFilter.operator === 'greaterThan' ? 'selected' : ''}>Büyüktür</option>
                    <option value="greaterOrEqual" ${savedFilter.operator === 'greaterOrEqual' ? 'selected' : ''}>Büyük veya Eşit</option>
                    <option value="lessThan" ${savedFilter.operator === 'lessThan' ? 'selected' : ''}>Küçüktür</option>
                    <option value="lessOrEqual" ${savedFilter.operator === 'lessOrEqual' ? 'selected' : ''}>Küçük veya Eşit</option>
                    <option value="between" ${savedFilter.operator === 'between' ? 'selected' : ''}>Arasında</option>
                `
                : `
                    <option value="contains" ${savedFilter.operator === 'contains' ? 'selected' : ''}>İçerir</option>
                    <option value="notContains" ${savedFilter.operator === 'notContains' ? 'selected' : ''}>İçermez</option>
                    <option value="equals" ${savedFilter.operator === 'equals' ? 'selected' : ''}>Eşittir</option>
                    <option value="notEquals" ${savedFilter.operator === 'notEquals' ? 'selected' : ''}>Eşit Değil</option>
                    <option value="startsWith" ${savedFilter.operator === 'startsWith' ? 'selected' : ''}>İle Başlar</option>
                    <option value="endsWith" ${savedFilter.operator === 'endsWith' ? 'selected' : ''}>İle Biter</option>
                `;

            html += `
                <div class="text-filter-section">
                    <div class="text-filter-row">
                        <select class="text-filter-operator" id="text-operator-${column}" onchange="handleOperatorChange('${column}')">
                            ${operatorOptions}
                        </select>
                    </div>
                    <div id="filter-inputs-${column}">
                        <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="text-value-${column}" placeholder="Değer girin..." value="${savedFilter.value || ''}" onkeypress="if(event.key==='Enter') applyFilter('${column}')">
                    </div>
                </div>
            `;
        } else {
            html += `
                <input type="text" class="filter-search" placeholder="Ara..." onkeyup="filterDropdownOptions('${column}', this.value)" onkeypress="if(event.key==='Enter') applyFilter('${column}')">
                <div class="filter-options" id="options-${column}">
                    <div class="filter-option" onclick="selectAllFilter('${column}', event)">
                        <input type="checkbox" ${!columnFilters[column] || columnFilters[column].length === 0 ? 'checked' : ''}>
                        <strong>(Tümünü Seç)</strong>
                    </div>
            `;

            distinctValues.forEach(value => {
                const isChecked = !columnFilters[column] || columnFilters[column].includes(value);
                html += `
                    <div class="filter-option" data-value="${value}" onclick="toggleFilterValue('${column}', '${value}', event)">
                        <input type="checkbox" ${isChecked ? 'checked' : ''}>
                        <span>${value}</span>
                    </div>
                `;
            });

            html += '</div>';
        }

        html += `
            <div class="filter-actions">
                <button class="btn-apply-filter" onclick="applyFilter('${column}')">
                    <i class="bi bi-check-circle"></i> Uygula
                </button>
                <button class="btn-clear-filter" onclick="clearFilter('${column}')">
                    <i class="bi bi-x-circle"></i> Temizle
                </button>
            </div>
        `;

        dropdown.innerHTML = html;
    }

    function switchFilterMode(column, mode, event) {
        if (event) {
            event.stopPropagation();
            event.preventDefault();
        }
        currentFilterMode[column] = mode;
        buildFilterDropdown(column);
    }

    function handleOperatorChange(column) {
        const operator = document.getElementById(`text-operator-${column}`).value;
        const inputContainer = document.getElementById(`filter-inputs-${column}`);
        const savedFilter = textFilters[column] || {};
        const distinctValues = [...new Set(data.map(item => item[column]))];
        const isNumeric = distinctValues.every(val => !isNaN(parseFloat(val)) && isFinite(val));

        if (operator === 'between') {
            inputContainer.innerHTML = `
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input mb-2" id="text-value-${column}" placeholder="Başlangıç..." value="${savedFilter.value || ''}">
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="text-value2-${column}" placeholder="Bitiş..." value="${savedFilter.value2 || ''}">
            `;
        } else {
            inputContainer.innerHTML = `
                <input type="${isNumeric ? 'number' : 'text'}" class="text-filter-input" id="text-value-${column}" placeholder="Değer girin..." value="${savedFilter.value || ''}">
            `;
        }
    }

    function filterDropdownOptions(column, searchTerm) {
        const options = document.querySelectorAll(`#options-${column} .filter-option`);
        const term = searchTerm.toLowerCase();
        options.forEach(option => {
            const text = option.textContent.toLowerCase();
            option.style.display = text.includes(term) ? 'flex' : 'none';
        });
    }

    function selectAllFilter(column, event) {
        if (event) {
            event.stopPropagation();
            if (event.target.type === 'checkbox') {
                const isChecked = event.target.checked;
                const options = document.querySelectorAll(`#options-${column} .filter-option input[type="checkbox"]`);
                options.forEach(checkbox => { checkbox.checked = isChecked; });
                return;
            }
        }
        const options = document.querySelectorAll(`#options-${column} .filter-option input[type="checkbox"]`);
        const selectAll = options[0];
        const isChecked = !selectAll.checked;
        options.forEach(checkbox => { checkbox.checked = isChecked; });
    }

    function toggleFilterValue(column, value, event) {
        if (event) {
            event.stopPropagation();
            if (event.target.type === 'checkbox') return;
        }
        const checkbox = event.currentTarget.querySelector('input[type="checkbox"]');
        if (checkbox) checkbox.checked = !checkbox.checked;
    }

    function applyFilter(column) {
        const mode = currentFilterMode[column] || 'list';

        if (mode === 'number') {
            const operator = document.getElementById(`text-operator-${column}`).value;
            const value = document.getElementById(`text-value-${column}`).value;
            if (operator === 'between') {
                const value2 = document.getElementById(`text-value2-${column}`).value;
                if (value.trim() && value2.trim()) {
                    textFilters[column] = { operator, value, value2 };
                    delete columnFilters[column];
                } else {
                    delete textFilters[column];
                }
            } else {
                if (value.trim()) {
                    textFilters[column] = { operator, value };
                    delete columnFilters[column];
                } else {
                    delete textFilters[column];
                }
            }
        } else {
            const selectedValues = [];
            const checkboxes = document.querySelectorAll(`#options-${column} .filter-option[data-value] input[type="checkbox"]:checked`);
            checkboxes.forEach(checkbox => {
                const v = checkbox.parentElement.dataset.value;
                if (v !== undefined && v !== null) selectedValues.push(v);
            });
            const allValues = [...new Set(filteredData.map(item => item[column]))];
            if (selectedValues.length === 0 || selectedValues.length === allValues.length) {
                delete columnFilters[column];
            } else {
                columnFilters[column] = selectedValues;
            }
            delete textFilters[column];
        }

        updateFilterIcon(column);
        document.getElementById(`filter-${column}`)?.classList.remove('show');
        currentOpenFilter = null;
        applyAllFilters();
        displayActiveFilters();
    }

    function clearFilter(column) {
        delete columnFilters[column];
        delete textFilters[column];
        updateFilterIcon(column);
        document.getElementById(`filter-${column}`)?.classList.remove('show');
        currentOpenFilter = null;
        applyAllFilters();
        displayActiveFilters();
    }

    function updateFilterIcon(column) {
        const icon = document.querySelector(`#filter-${column}`)?.previousElementSibling;
        const count = document.getElementById(`count-${column}`);
        const hasFilter = (columnFilters[column] && columnFilters[column].length > 0) || textFilters[column];
        if (hasFilter) {
            icon?.classList.add('active');
            if (count) {
                count.style.display = 'inline-block';
                count.textContent = columnFilters[column] ? columnFilters[column].length : '1';
            }
        } else {
            icon?.classList.remove('active');
            if (count) count.style.display = 'none';
        }
    }

    function applyAllFilters() {
        const searchTerm = document.getElementById('searchInput').value.toLocaleLowerCase('tr-TR');
        filteredData = data.filter(item => {
            if (searchTerm) {
                const matchSearch = Object.values(item).some(val => String(val).toLocaleLowerCase('tr-TR').includes(searchTerm));
                if (!matchSearch) return false;
            }
            for (let column in columnFilters) {
                const itemValue = String(item[column]);
                const filterValues = columnFilters[column].map(v => String(v));
                if (!filterValues.includes(itemValue)) return false;
            }
            for (let column in textFilters) {
                const filter = textFilters[column];
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
        currentPage = 1;
        renderTable();
        updateRecordCount();
    }

    function displayActiveFilters() {
        document.querySelectorAll('.filter-count').forEach(el => {
            const column = el.id.replace('count-', '');
            updateFilterIcon(column);
        });
    }

    function resetAllFilters() {
        document.getElementById('searchInput').value = '';
        columnFilters = {};
        textFilters = {};
        currentFilterMode = {};
        Object.keys(fieldNames).forEach(column => updateFilterIcon(column));
        filteredData = [...data];
        currentPage = 1;
        renderTable();
        updateRecordCount();
        if ($('#pivotContainer').length) {
            $('#pivotContainer').pivotUI(rawData, {}, true, 'tr');
        }
    }

    function sortTable(column) {
        if (sortColumn === column) {
            sortAscending = !sortAscending;
        } else {
            sortColumn = column;
            sortAscending = true;
        }
        filteredData.sort((a, b) => {
            let valA = a[column];
            let valB = b[column];
            if (typeof valA === 'string') {
                valA = valA.toLowerCase();
                valB = valB.toLowerCase();
            }
            if (sortAscending) return valA > valB ? 1 : valA < valB ? -1 : 0;
            return valA < valB ? 1 : valA > valB ? -1 : 0;
        });

        document.querySelectorAll('.header-text').forEach(header => {
            const icon = header.querySelector('.sort-icon');
            if (icon) {
                header.classList.remove('sorted');
                icon.className = 'bi bi-arrow-down-up sort-icon';
            }
        });
        const activeHeader = document.querySelector(`[onclick="sortTable('${column}')"]`);
        if (activeHeader) {
            const icon = activeHeader.querySelector('.sort-icon');
            activeHeader.classList.add('sorted');
            icon.className = sortAscending ? 'bi bi-arrow-up sort-icon' : 'bi bi-arrow-down sort-icon';
        }
        renderTable();
    }

    function renderTable() {
        const tbody = document.getElementById('tableBody');
        tbody.innerHTML = '';
        const start = (currentPage - 1) * itemsPerPage;
        const end = start + itemsPerPage;
        const pageData = filteredData.slice(start, end);

        if (pageData.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center py-4">Sonuç bulunamadı</td></tr>';
            updateInfo();
            return;
        }

        pageData.forEach(item => {
            const statusClass = item.status === 'Aktif' ? 'status-active' : item.status === 'Pasif' ? 'status-inactive' : 'status-pending';
            const row = `
                <tr>
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.department}</td>
                    <td>${item.position}</td>
                    <td>${item.city}</td>
                    <td>${item.salary}?</td>
                    <td><span class="status-badge ${statusClass}">${item.status}</span></td>
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
                    </td>
                </tr>`;
            tbody.innerHTML += row;
        });
        updatePagination();
        updateInfo();
    }

    function updatePagination() {
        const totalPages = Math.ceil(filteredData.length / itemsPerPage);
        const pagination = document.getElementById('pagination');
        pagination.innerHTML = '';

        pagination.innerHTML += `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage(1); return false;"
                   style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">« En Başa</a>
            </li>`;

        pagination.innerHTML += `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage(${currentPage - 1}); return false;"
                   style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">‹ Önceki</a>
            </li>`;

        pagination.innerHTML += `
            <li class="page-item" style="display: flex; align-items: center; margin-right: 4px;">
                <input type="number" id="pageNumberInput" class="form-control form-control-sm" value="${currentPage}" min="1" max="${totalPages}"
                       onkeypress="if(event.key==='Enter') goToPage()" onblur="goToPage()"
                       style="width: 55px; text-align: center; font-size: 0.85rem; padding: 0 8px; border-radius: 6px; height: 36px; border: 2px solid #667eea; color: #667eea; font-weight: 600; box-sizing: border-box;">
            </li>
            <li class="page-item" style="display: flex; align-items: center; margin-right: 8px;">
                <span style="font-size: 0.85rem; color: #666; padding: 0 4px; height: 36px; display: flex; align-items: center; font-weight: 600;">/ ${totalPages}</span>
            </li>`;

        pagination.innerHTML += `
            <li class="page-item ${currentPage === totalPages || totalPages === 0 ? 'disabled' : ''}" style="margin-right: 8px;">
                <a class="page-link" href="#" onclick="changePage(${currentPage + 1}); return false;"
                   style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">Sonraki ›</a>
            </li>`;

        pagination.innerHTML += `
            <li class="page-item ${currentPage === totalPages || totalPages === 0 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="changePage(${totalPages}); return false;"
                   style="font-size: 0.85rem; padding: 6px 12px; border: 2px solid #667eea; color: #667eea; background: white; border-radius: 6px; text-decoration: none; display: inline-flex; align-items: center; height: 36px; font-weight: 600;">En Son »</a>
            </li>`;
    }

    function goToPage() {
        const input = document.getElementById('pageNumberInput');
        let pageNum = parseInt(input.value);
        const totalPages = Math.ceil(filteredData.length / itemsPerPage);
        if (isNaN(pageNum) || pageNum < 1) pageNum = 1;
        else if (pageNum > totalPages) pageNum = totalPages;
        input.value = pageNum;
        changePage(pageNum);
    }

    function changePage(page) {
        const totalPages = Math.ceil(filteredData.length / itemsPerPage);
        if (page >= 1 && page <= totalPages) {
            currentPage = page;
            renderTable();
        }
        return false;
    }

    function changeItemsPerPage() {
        itemsPerPage = parseInt(document.getElementById('itemsPerPageSelect').value);
        currentPage = 1;
        renderTable();
    }

    function updateInfo() {
        const start = filteredData.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
        const end = Math.min(currentPage * itemsPerPage, filteredData.length);
        const hasActiveFilters = Object.keys(columnFilters).length > 0 || Object.keys(textFilters).length > 0 || document.getElementById('searchInput').value.trim() !== '';
        let infoText = '';
        if (hasActiveFilters && filteredData.length < data.length) {
            infoText = `${data.length} kayıttan ${filteredData.length} tanesi bulundu`;
        } else {
            infoText = `Gösteriliyor: ${start}-${end} / ${filteredData.length}`;
        }
        document.getElementById('showingInfo').textContent = infoText;
        document.getElementById('totalRecords').textContent = filteredData.length;
    }

    function viewItem(id) { alert(`Görüntüleniyor: ID ${id}`); }
    function editItem(id) { alert(`Düzenleniyor: ID ${id}`); }
    function deleteItem(id) {
        if (confirm(`ID ${id} numaralı kaydı silmek istediğinizden emin misiniz?`)) {
            const index = data.findIndex(item => item.id === id);
            if (index > -1) {
                data.splice(index, 1);
                applyAllFilters();
            }
        }
    }

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.filter-dropdown') && !e.target.closest('.filter-icon')) {
            document.querySelectorAll('.filter-dropdown').forEach(dropdown => dropdown.classList.remove('show'));
            currentOpenFilter = null;
        }
    });

    // expose globals for inline handlers
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
    window.viewItem = viewItem;
    window.editItem = editItem;
    window.deleteItem = deleteItem;
})(window, jQuery);
