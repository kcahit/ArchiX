'use strict';

(function () {
    function initDatasetRecord() {
        var ctxEl = document.getElementById('dsrec-ctx');
        var backUrl = ctxEl ? (ctxEl.getAttribute('data-backurl') || '/Tools/Dataset/Grid') : '/Tools/Dataset/Grid';
        var hasOps = ctxEl ? (ctxEl.getAttribute('data-hasops') === '1') : false;
        var isNew = ctxEl ? (ctxEl.getAttribute('data-isnew') === '1') : false;

        var isDirty = false;

        var form = document.getElementById('formDetail');
        function markDirty() { isDirty = true; }

        if (form) {
            form.addEventListener('input', markDirty);
            form.addEventListener('change', markDirty);
        }

        var btnKapat = document.getElementById('btnKapat');
        var btnDegistir = document.getElementById('btnDegistir');
        var btnSil = document.getElementById('btnSil');

        function goBack() { window.location.href = backUrl; }

        if (btnDegistir) {
            btnDegistir.addEventListener('click', function () {
                if (!hasOps) return;
                isDirty = false;
                alert('Degistir (fake) tamamlandi.');
            });
        }

        if (btnSil) {
            btnSil.addEventListener('click', function () {
                if (!hasOps) return;
                if (isNew) return;
                if (confirm('Silmek istediginize emin misiniz?')) {
                    isDirty = false;
                    alert('Sil (fake) tamamlandi.');
                    goBack();
                }
            });
        }

        if (btnKapat) {
            btnKapat.addEventListener('click', function () {
                if (!isDirty) {
                    goBack();
                    return;
                }

                if (!window.bootstrap || !window.bootstrap.Modal) {
                    goBack();
                    return;
                }

                var modalEl = document.getElementById('closeConfirmModal');
                var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
                modal.show();
            });
        }

        var btnEvet = document.getElementById('btnModalEvet');
        var btnHayir = document.getElementById('btnModalHayir');

        if (btnEvet) {
            btnEvet.addEventListener('click', function () {
                isDirty = false;
                goBack();
            });
        }

        if (btnHayir) {
            btnHayir.addEventListener('click', function () {
                isDirty = false;
                goBack();
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDatasetRecord);
    } else {
        initDatasetRecord();
    }
})();
