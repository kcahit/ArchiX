'use strict';

// Minimal accordion toggle for Active Filters area.
// Goal: ensure click toggles open/close reliably via Bootstrap Collapse API.

(function (window) {
    function getCollapseEl(tableId) {
        return document.getElementById(`${tableId}-collapseFilters`);
    }

    function toggleActiveFiltersAccordion(tableId, ev) {
        ev?.preventDefault?.();
        ev?.stopPropagation?.();

        const el = getCollapseEl(tableId);
        if (!el || !window.bootstrap?.Collapse) return;

        const instance = window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
        if (el.classList.contains('show')) instance.hide(); else instance.show();
    }

    window.ArchiXGrid = window.ArchiXGrid || {};
    window.ArchiXGrid.toggleActiveFiltersAccordion = toggleActiveFiltersAccordion;
})(window);
