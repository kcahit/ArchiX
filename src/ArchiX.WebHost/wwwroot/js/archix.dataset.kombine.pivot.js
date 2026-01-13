'use strict';

(function (window) {
    function getGridState(tableId) {
        // grid state is internal; rebuild pivot data from the rendered table rows
        const tbody = document.getElementById(`${tableId}-tableBody`);
        const table = document.getElementById(`${tableId}-dataTable`);
        if (!tbody || !table) return { rows: [], columns: [] };

        const headers = Array.from(table.querySelectorAll('thead th'))
            .map(th => (th.textContent || '').trim())
            .filter(x => x && x !== 'İşlemler');

        const trList = Array.from(tbody.querySelectorAll('tr'));
        const rows = trList.map(tr => {
            const tds = Array.from(tr.querySelectorAll('td'));
            const obj = {};
            for (let i = 0; i < headers.length && i < tds.length; i++) {
                obj[headers[i]] = (tds[i].textContent || '').trim();
            }
            return obj;
        });

        return { rows, columns: headers };
    }

    function initPivot(tableId) {
        const pivotEl = document.getElementById(`${tableId}-pivotContainer`);
        if (!pivotEl) return;

        if (!window.jQuery || !window.jQuery.fn || typeof window.jQuery.fn.pivotUI !== 'function') return;

        const { rows } = getGridState(tableId);

        pivotEl.innerHTML = '';
        window.jQuery(pivotEl).pivotUI(rows, {
            rendererName: 'Table',
        }, true, 'tr');
    }

    function bindAutoRefresh(tableId) {
        const tbody = document.getElementById(`${tableId}-tableBody`);
        if (!tbody || tbody.dataset.archixPivotBound === '1') return;
        tbody.dataset.archixPivotBound = '1';

        const debounced = (function () {
            let t = null;
            return function () {
                if (t) clearTimeout(t);
                t = setTimeout(() => initPivot(tableId), 30);
            };
        })();

        const obs = new MutationObserver(debounced);
        obs.observe(tbody, { childList: true, subtree: true, characterData: true });

        initPivot(tableId);
    }

    window.archixDatasetKombinePivot = {
        init: initPivot,
        bindAutoRefresh: bindAutoRefresh
    };
})(window);
