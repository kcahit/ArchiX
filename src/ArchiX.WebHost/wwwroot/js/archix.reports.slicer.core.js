/**
 * ArchiX Reports Slicer Core Engine
 * Power BI benzeri slicer/filtre motoru - tüm rapor template'lerinde kullanýlýr
 */

(function(window) {
    'use strict';

    // Global state
    window.ArchiXReportsSlicer = {
        data: [],
        filteredData: [],
        columnFilters: {},
        textFilters: {},
        slicerSelections: {},
        activeSlicerColumns: [],
        currentFilterMode: {},
        fieldNames: {},
        
        // Callbacks
        onFilterChange: null,
        
        /**
         * Initialize the slicer engine
         */
        init: function(data, fieldNames, onFilterChange) {
            this.data = data;
            this.filteredData = [...data];
            this.fieldNames = fieldNames || {};
            this.onFilterChange = onFilterChange;
            
            this.initSlicers();
            this.applyAllFilters();
        },
        
        /**
         * Apply all filters (search + column + text + slicer)
         */
        applyAllFilters: function() {
            const searchInput = document.getElementById('searchInput');
            const searchTerm = searchInput ? searchInput.value.toLocaleLowerCase('tr-TR') : '';

            this.filteredData = this.data.filter(item => {
                // 1. Genel arama
                if (searchTerm) {
                    const matchSearch = Object.values(item).some(val =>
                        String(val).toLocaleLowerCase('tr-TR').includes(searchTerm)
                    );
                    if (!matchSearch) return false;
                }

                // 2. Tablo kolon liste filtreleri
                for (let column in this.columnFilters) {
                    const itemValue = String(item[column]);
                    const filterValues = this.columnFilters[column].map(v => String(v));
                    if (!filterValues.includes(itemValue)) {
                        return false;
                    }
                }

                // 3. Tablo metin filtreleri
                for (let column in this.textFilters) {
                    const filter = this.textFilters[column];
                    const itemValue = String(item[column]).toLowerCase();
                    const filterValue = filter.value.toLowerCase();

                    let match = false;
                    switch(filter.operator) {
                        case 'contains':
                            match = itemValue.includes(filterValue);
                            break;
                        case 'notContains':
                            match = !itemValue.includes(filterValue);
                            break;
                        case 'equals':
                            match = itemValue === filterValue;
                            break;
                        case 'notEquals':
                            match = itemValue !== filterValue;
                            break;
                        case 'startsWith':
                            match = itemValue.startsWith(filterValue);
                            break;
                        case 'endsWith':
                            match = itemValue.endsWith(filterValue);
                            break;
                        case 'greaterThan':
                            match = parseFloat(item[column]) > parseFloat(filter.value);
                            break;
                        case 'greaterOrEqual':
                            match = parseFloat(item[column]) >= parseFloat(filter.value);
                            break;
                        case 'lessThan':
                            match = parseFloat(item[column]) < parseFloat(filter.value);
                            break;
                        case 'lessOrEqual':
                            match = parseFloat(item[column]) <= parseFloat(filter.value);
                            break;
                        case 'between':
                            const numValue = parseFloat(item[column]);
                            const min = parseFloat(filter.value);
                            const max = parseFloat(filter.value2);
                            match = numValue >= min && numValue <= max;
                            break;
                    }

                    if (!match) return false;
                }

                // 4. Slicer seçimleri
                for (let column in this.slicerSelections) {
                    const itemValue = String(item[column]);
                    const filterValues = this.slicerSelections[column].map(v => String(v));
                    if (!filterValues.includes(itemValue)) {
                        return false;
                    }
                }

                return true;
            });

            // Callback çaðýr
            if (this.onFilterChange) {
                this.onFilterChange(this.filteredData);
            }
        },
        
        /**
         * Initialize slicer system
         */
        initSlicers: function() {
            this.createColumnCheckList();
            
            // Tablo filtresi olan kolonlarý otomatik aç
            Object.keys(this.columnFilters).forEach(column => {
                if (!this.activeSlicerColumns.includes(column)) {
                    this.activeSlicerColumns.push(column);
                }
            });

            if (this.activeSlicerColumns.length > 0) {
                this.createColumnCheckList();
                this.rebuildSlicers();
            }
        },
        
        /**
         * Create column checkbox list
         */
        createColumnCheckList: function() {
            const checkList = document.getElementById('columnCheckList');
            if (!checkList) return;
            
            const allColumns = Object.keys(this.fieldNames).filter(col => col !== 'id');

            checkList.innerHTML = '';
            allColumns.forEach(column => {
                const isChecked = this.activeSlicerColumns.includes(column);
                const hasFilter = this.slicerSelections[column] && this.slicerSelections[column].length > 0;
                const bgColor = hasFilter ? '#ffc107' : (isChecked ? '#667eea' : 'white');
                const textColor = isChecked && !hasFilter ? 'white' : '#333';
                const borderColor = hasFilter ? '#ffc107' : (isChecked ? '#667eea' : '#ddd');

                const checkDiv = document.createElement('div');
                checkDiv.style.cssText = `
                    padding: 3px 6px;
                    margin-bottom: 1px;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 0.7rem;
                    background: ${bgColor};
                    color: ${textColor};
                    border: 1px solid ${borderColor};
                    transition: all 0.2s;
                    display: flex;
                    align-items: center;
                `;

                checkDiv.innerHTML = `
                    <label for="check-${column}" style="cursor: pointer; margin: 0; font-size: 0.7rem; line-height: 1.2; display: flex; align-items: center; width: 100%;">
                        <input type="checkbox" value="${column}" id="check-${column}"
                               ${isChecked ? 'checked' : ''} onchange="ArchiXReportsSlicer.toggleSlicerColumn('${column}')"
                               style="width: 12px; height: 12px; margin-right: 6px; cursor: pointer; flex-shrink: 0;">
                        ${this.fieldNames[column]}
                    </label>
                `;

                checkDiv.onmouseenter = function() {
                    if (!isChecked && !hasFilter) {
                        this.style.background = '#f0f0f0';
                    }
                };
                checkDiv.onmouseleave = function() {
                    const currentHasFilter = ArchiXReportsSlicer.slicerSelections[column] && ArchiXReportsSlicer.slicerSelections[column].length > 0;
                    const currentBgColor = currentHasFilter ? '#ffc107' : (isChecked ? '#667eea' : 'white');
                    this.style.background = currentBgColor;
                };

                checkList.appendChild(checkDiv);
            });
        },
        
        /**
         * Toggle slicer column on/off
         */
        toggleSlicerColumn: function(column) {
            const index = this.activeSlicerColumns.indexOf(column);

            if (index > -1) {
                this.activeSlicerColumns.splice(index, 1);
                delete this.slicerSelections[column];
            } else {
                const allColumns = Object.keys(this.fieldNames);
                this.activeSlicerColumns.push(column);
                this.activeSlicerColumns.sort((a, b) => allColumns.indexOf(a) - allColumns.indexOf(b));
            }

            this.createColumnCheckList();
            this.rebuildSlicers();

            if (Object.keys(this.slicerSelections).length > 0) {
                this.applyAllFilters();
            }
        },
        
        /**
         * Rebuild all slicers
         */
        rebuildSlicers: function() {
            const slicerContainer = document.getElementById('slicerContainer');
            if (!slicerContainer) return;

            slicerContainer.innerHTML = '';

            if (this.activeSlicerColumns.length === 0) {
                slicerContainer.innerHTML = `
                    <div style="width: 100%; text-align: center; color: #6c757d; padding: 40px; font-size: 0.85rem;">
                        <i class="bi bi-arrow-left"></i> Soldaki listeden kolon seçin
                    </div>
                `;
                return;
            }

            this.activeSlicerColumns.forEach(column => {
                const hasFilter = this.slicerSelections[column] && this.slicerSelections[column].length > 0;
                const headerBgColor = hasFilter ? '#ffc107' : 'transparent';
                const headerTextColor = '#667eea';

                const slicer = document.createElement('div');
                slicer.style.cssText = 'flex: 0 0 180px; margin-right: 10px; margin-bottom: 10px;';
                slicer.innerHTML = `
                    <div class="slicer-card" style="border: 1px solid #ddd; border-radius: 6px; padding: 8px; background: #f8f9fa; width: 180px;">
                        <h6 style="font-size: 0.7rem; font-weight: bold; margin-bottom: 6px; color: ${headerTextColor}; background: ${headerBgColor}; padding: 2px 4px; border-radius: 3px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" title="${this.fieldNames[column]}">
                            ${this.fieldNames[column]}
                        </h6>
                        <div class="slicer-items" id="slicer-${column}" style="max-height: 150px; overflow-y: auto;">
                            <!-- Items will be populated dynamically -->
                        </div>
                    </div>
                `;
                slicerContainer.appendChild(slicer);

                this.updateSlicerItems(column);
            });
        },
        
        /**
         * Update slicer items (dynamic based on other filters)
         */
        updateSlicerItems: function(column) {
            const slicerDiv = document.getElementById(`slicer-${column}`);
            if (!slicerDiv) return;

            let availableData = [...this.data];

            // Apply search filter
            const searchInput = document.getElementById('searchInput');
            const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
            if (searchTerm) {
                availableData = availableData.filter(item =>
                    Object.values(item).some(val =>
                        String(val).toLowerCase().includes(searchTerm)
                    )
                );
            }

            // Apply column filters
            for (let col in this.columnFilters) {
                if (this.columnFilters[col].length > 0) {
                    availableData = availableData.filter(item =>
                        this.columnFilters[col].map(v => String(v)).includes(String(item[col]))
                    );
                }
            }

            // Apply text filters
            for (let col in this.textFilters) {
                const filter = this.textFilters[col];
                availableData = availableData.filter(item => {
                    const itemValue = String(item[col]).toLowerCase();
                    const filterValue = filter.value.toLowerCase();

                    switch(filter.operator) {
                        case 'contains': return itemValue.includes(filterValue);
                        case 'notContains': return !itemValue.includes(filterValue);
                        case 'equals': return itemValue === filterValue;
                        case 'notEquals': return itemValue !== filterValue;
                        case 'startsWith': return itemValue.startsWith(filterValue);
                        case 'endsWith': return itemValue.endsWith(filterValue);
                        case 'greaterThan': return parseFloat(item[col]) > parseFloat(filter.value);
                        case 'lessThan': return parseFloat(item[col]) < parseFloat(filter.value);
                        case 'between':
                            const numValue = parseFloat(item[col]);
                            return numValue >= parseFloat(filter.value) && numValue <= parseFloat(filter.value2);
                        default: return true;
                    }
                });
            }

            // Apply other slicer selections
            for (let col in this.slicerSelections) {
                if (col !== column && this.slicerSelections[col].length > 0) {
                    availableData = availableData.filter(item =>
                        this.slicerSelections[col].includes(String(item[col]))
                    );
                }
            }

            const availableValues = [...new Set(availableData.map(item => item[column]))].sort();
            const selectedValues = this.slicerSelections[column] || [];

            slicerDiv.innerHTML = '';
            availableValues.forEach(value => {
                const isSelected = selectedValues.includes(String(value));
                const itemDiv = document.createElement('div');
                itemDiv.className = 'slicer-item';
                itemDiv.style.cssText = `
                    padding: 3px 6px;
                    margin-bottom: 1px;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 0.7rem;
                    background: ${isSelected ? '#667eea' : 'white'};
                    color: ${isSelected ? 'white' : '#333'};
                    border: 1px solid ${isSelected ? '#667eea' : '#ddd'};
                    transition: all 0.2s;
                    white-space: nowrap;
                    overflow: hidden;
                    text-overflow: ellipsis;
                `;
                itemDiv.textContent = value;
                itemDiv.onclick = () => this.toggleSlicerValue(column, value);

                itemDiv.onmouseenter = function() {
                    if (!isSelected) {
                        this.style.background = '#f0f0f0';
                    }
                };
                itemDiv.onmouseleave = function() {
                    if (!isSelected) {
                        this.style.background = 'white';
                    }
                };

                slicerDiv.appendChild(itemDiv);
            });
        },
        
        /**
         * Toggle slicer value selection
         */
        toggleSlicerValue: function(column, value) {
            if (!this.slicerSelections[column]) {
                this.slicerSelections[column] = [];
            }

            const valueStr = String(value);
            const index = this.slicerSelections[column].indexOf(valueStr);

            if (index > -1) {
                this.slicerSelections[column].splice(index, 1);
            } else {
                this.slicerSelections[column].push(valueStr);
            }

            if (this.slicerSelections[column].length === 0) {
                delete this.slicerSelections[column];
            }

            this.createColumnCheckList();
            this.updateAllSlicers();
            this.applyAllFilters();
        },
        
        /**
         * Update all slicers
         */
        updateAllSlicers: function() {
            this.activeSlicerColumns.forEach(column => {
                this.updateSlicerItems(column);
                const hasFilter = this.slicerSelections[column] && this.slicerSelections[column].length > 0;
                const headerBgColor = hasFilter ? '#ffc107' : 'transparent';
                const slicerCard = document.querySelector(`#slicer-${column}`)?.closest('.slicer-card');
                if (slicerCard) {
                    const header = slicerCard.querySelector('h6');
                    if (header) {
                        header.style.background = headerBgColor;
                        header.style.color = '#667eea';
                    }
                }
            });
        },
        
        /**
         * Clear advanced filters (slicer selections)
         */
        clearAdvancedFilters: function() {
            this.slicerSelections = {};
            this.createColumnCheckList();
            this.updateAllSlicers();
            this.applyAllFilters();
        },
        
        /**
         * Toggle all columns on/off
         */
        toggleAllColumns: function() {
            const toggleCheckbox = document.getElementById('toggleAllColumns');
            const allColumns = Object.keys(this.fieldNames).filter(col => col !== 'id');

            if (toggleCheckbox.checked) {
                this.activeSlicerColumns = [...allColumns];
            } else {
                this.activeSlicerColumns = [];
                this.slicerSelections = {};
            }

            this.createColumnCheckList();
            this.rebuildSlicers();
            this.applyAllFilters();
        },
        
        /**
         * Reset all filters
         */
        resetAllFilters: function() {
            const searchInput = document.getElementById('searchInput');
            if (searchInput) searchInput.value = '';
            
            this.columnFilters = {};
            this.textFilters = {};
            this.currentFilterMode = {};
            this.slicerSelections = {};
            this.activeSlicerColumns = [];

            this.createColumnCheckList();
            this.rebuildSlicers();
            this.applyAllFilters();
        }
    };

    // Setup search input listener
    window.addEventListener('DOMContentLoaded', function() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', function() {
                window.ArchiXReportsSlicer.applyAllFilters();
                if (window.ArchiXReportsSlicer.activeSlicerColumns.length > 0) {
                    window.ArchiXReportsSlicer.updateAllSlicers();
                }
            });
        }
    });

})(window);
