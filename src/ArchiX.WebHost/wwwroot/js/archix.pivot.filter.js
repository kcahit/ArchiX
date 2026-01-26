/**
 * ArchiX Pivot Filter Enhancement
 * PivotTable.js filtre popup'larini Turkcelestir ve ArchiX stiline cevir
 */

(function(window) {
    'use strict';

    window.ArchiXPivotFilter = {
        
        enhanceFilters: function() {
            setTimeout(() => {
                const boxes = document.querySelectorAll('.pvtFilterBox');
                boxes.forEach(box => this.turkcelestir(box));
            }, 100);
        },

        turkcelestir: function(filterBox) {
            if (filterBox.dataset.turkce) return;
            filterBox.dataset.turkce = 'true';
            
            const searchInput = filterBox.querySelector('input[type="text"]');
            if (searchInput) {
                searchInput.placeholder = 'Ara...';
            }
            
            const allButtons = filterBox.querySelectorAll('button');
            
            allButtons.forEach(btn => {
                const text = btn.textContent.trim();
                
                if (text === 'Select All') {
                    btn.innerHTML = '<i class="bi bi-check-square"></i> T\u00FCm\u00FCn\u00FC Se\u00E7';
                    btn.style.cssText = 'padding: 6px 12px; margin-right: 4px; border: 1px solid #667eea; background: white; color: #667eea; border-radius: 4px; font-size: 0.8rem; font-weight: 600;';
                }
                else if (text === 'Select None') {
                    btn.innerHTML = '<i class="bi bi-square"></i> Hi\u00E7biri';
                    btn.style.cssText = 'padding: 6px 12px; border: 1px solid #ddd; background: white; color: #666; border-radius: 4px; font-size: 0.8rem; font-weight: 600;';
                }
                else if (text === 'OK') {
                    btn.innerHTML = '<i class="bi bi-check-circle"></i> Uygula';
                }
                else if (text === 'Cancel') {
                    btn.innerHTML = '<i class="bi bi-x-circle"></i> Temizle';
                }
            });
        },

        init: function() {
            this.enhanceFilters();
            
            const observer = new MutationObserver(() => {
                const boxes = document.querySelectorAll('.pvtFilterBox:not([data-turkce])');
                boxes.forEach(box => this.turkcelestir(box));
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        }
    };

})(window);
