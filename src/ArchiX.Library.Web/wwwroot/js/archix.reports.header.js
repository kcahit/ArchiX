/**
 * ArchiX Reports Header Component
 * Grid ve Pivot template'leri için ortak header yapýsý
 */

(function(window) {
    'use strict';

    window.ArchiXReportsHeader = {
        config: {
            title: '',
            icon: 'bi-grid-3x3-gap-fill',
            showExport: true,
            showAdvancedSearch: true,
            exportFormats: ['excel', 'pdf', 'csv', 'txt', 'json']
        },

        /**
         * Initialize header
         * @param {Object} options - Configuration options
         * @param {string} options.title - Page title
         * @param {string} options.icon - Bootstrap icon class
         * @param {boolean} options.showExport - Show export dropdown
         * @param {boolean} options.showAdvancedSearch - Show advanced search button
         * @param {Array} options.exportFormats - Export formats to show
         * @param {Function} options.onExport - Export callback function
         * @param {Function} options.onReset - Reset callback function
         */
        init: function(options) {
            this.config = { ...this.config, ...options };
            this.render();
            this.attachEvents();
        },

        /**
         * Render header HTML
         */
        render: function() {
            const headerContainer = document.getElementById('reportHeader');
            if (!headerContainer) {
                console.error('Header container #reportHeader not found');
                return;
            }

            const exportDropdownHtml = this.config.showExport ? this.renderExportDropdown() : '';
            const advancedSearchHtml = this.config.showAdvancedSearch ? this.renderAdvancedSearchButton() : '';

            headerContainer.innerHTML = `
                <div class="d-flex justify-content-between align-items-center gap-2 mb-3">
                    <!-- Sol: Baþlýk + Kayýt Sayýsý -->
                    <div class="d-flex align-items-center gap-3">
                        <h1 class="pivot-title" style="font-size: 1.5rem; font-weight: 600; color: #667eea; margin: 0;">
                            <i class="${this.config.icon} me-2"></i>${this.config.title}
                        </h1>
                        <span class="record-count-badge" style="background: #667eea; color: white; padding: 4px 12px; border-radius: 20px; font-size: 0.85rem; font-weight: 600;">
                            <strong><span id="totalRecords">0</span></strong> Kayýt
                        </span>
                    </div>

                    <!-- Orta: Arama + Sýfýrla -->
                    <div class="d-flex align-items-center gap-2 flex-grow-1">
                        <div class="search-box flex-grow-1" style="position: relative; max-width: 400px;">
                            <i class="bi bi-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #999;"></i>
                            <input type="text" class="form-control form-control-sm grid-search-input" id="searchInput" placeholder="Genel arama..." style="padding-left: 35px;">
                        </div>
                        <button class="btn btn-primary btn-sm grid-btn-reset" onclick="ArchiXReportsHeader.handleReset()" title="Sýfýrla">
                            <i class="bi bi-arrow-clockwise text-white"></i>
                        </button>
                    </div>

                    ${advancedSearchHtml}
                    ${exportDropdownHtml}
                </div>
            `;
        },

        /**
         * Render advanced search button
         */
        renderAdvancedSearchButton: function() {
            return `
                <div class="mx-3">
                    <button class="btn btn-outline-primary btn-sm grid-btn-advanced" type="button" data-bs-toggle="collapse" data-bs-target="#advancedSearch">
                        <i class="bi bi-sliders me-1"></i> Filtreler
                    </button>
                </div>
            `;
        },

        /**
         * Render export dropdown
         */
        renderExportDropdown: function() {
            const formats = {
                excel: { icon: 'bi-file-earmark-excel', label: 'Excel' },
                pdf: { icon: 'bi-file-earmark-pdf', label: 'PDF' },
                csv: { icon: 'bi-filetype-csv', label: 'CSV' },
                txt: { icon: 'bi-file-text', label: 'TXT' },
                json: { icon: 'bi-filetype-json', label: 'JSON' }
            };

            let items = '';
            this.config.exportFormats.forEach(format => {
                const f = formats[format];
                if (f) {
                    items += `<li><a class="dropdown-item" href="#" onclick="ArchiXReportsHeader.handleExport('${format}'); return false;"><i class="${f.icon} me-2"></i>${f.label}</a></li>`;
                }
            });

            return `
                <div class="ms-auto">
                    <div class="dropdown">
                        <button class="btn btn-primary btn-sm dropdown-toggle grid-btn-export" type="button" id="exportDropdown" data-bs-toggle="dropdown">
                            <i class="bi bi-download me-1"></i> Aktar
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end">
                            ${items}
                        </ul>
                    </div>
                </div>
            `;
        },

        /**
         * Attach event listeners
         */
        attachEvents: function() {
            // Search input is handled by slicer core
        },

        /**
         * Handle export action
         */
        handleExport: function(format) {
            if (this.config.onExport) {
                this.config.onExport(format);
            } else {
                alert(`${format.toUpperCase()} formatýnda dýþa aktarýlýyor...`);
            }
        },

        /**
         * Handle reset action
         */
        handleReset: function() {
            if (this.config.onReset) {
                this.config.onReset();
            } else if (window.ArchiXReportsSlicer) {
                window.ArchiXReportsSlicer.resetAllFilters();
            }
        },

        /**
         * Update record count
         */
        updateRecordCount: function(count) {
            const recordCountElement = document.getElementById('totalRecords');
            if (recordCountElement) {
                recordCountElement.textContent = count;
            }
        }
    };

})(window);
