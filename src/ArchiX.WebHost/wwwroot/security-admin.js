// security-admin.js - ArchiX Security Admin Panel

(function () {
    'use strict';

    window.SecurityAdmin = {
        initDashboardCharts: function (errorStatsData) {
            if (!errorStatsData || errorStatsData.length === 0) return;

            const ctx = document.getElementById('errorStatsChart');
            if (!ctx) return;

            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: errorStatsData.map(x => x.date),
                    datasets: [{
                        label: 'Hatalı Denemeler',
                        data: errorStatsData.map(x => x.count),
                        borderColor: 'rgb(220, 53, 69)',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                title: function (context) {
                                    return context[0].label;
                                },
                                label: function (context) {
                                    return 'Hata: ' + context.parsed.y;
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 }
                        }
                    }
                }
            });
        },

        initBlacklistDataTable: function () {
            const table = $('#blacklistTable');
            if (table.length === 0) return;

            table.DataTable({
                processing: true,
                serverSide: false,
                pageLength: 25,
                order: [[2, 'desc']],
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/tr.json'
                },
                columns: [
                    { data: 'word', title: 'Kelime' },
                    { data: 'createdBy', title: 'Ekleyen' },
                    {
                        data: 'createdAtUtc',
                        title: 'Tarih',
                        render: function (data) {
                            return new Date(data).toLocaleString('tr-TR');
                        }
                    },
                    {
                        data: 'isActive',
                        title: 'Durum',
                        render: function (data) {
                            return data
                                ? '<span class="badge bg-success">Aktif</span>'
                                : '<span class="badge bg-secondary">Pasif</span>';
                        }
                    },
                    {
                        data: 'id',
                        title: 'İşlem',
                        orderable: false,
                        render: function (data) {
                            return `<button type="button" class="btn btn-sm btn-danger" onclick="SecurityAdmin.deleteBlacklistWord(${data})">
                                        <i class="fas fa-trash"></i>
                                    </button>`;
                        }
                    }
                ]
            });
        },

        deleteBlacklistWord: function (id) {
            if (!confirm('Bu kelimeyi silmek istediğinizden emin misiniz?')) return;

            const form = document.createElement('form');
            form.method = 'POST';
            form.action = window.location.pathname + '?handler=Delete';

            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenInput) {
                const token = tokenInput.cloneNode(true);
                form.appendChild(token);
            }

            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'id';
            idInput.value = id;
            form.appendChild(idInput);

            const appIdInput = document.querySelector('input[name="ApplicationId"]');
            if (appIdInput) {
                const appId = appIdInput.cloneNode(true);
                form.appendChild(appId);
            }

            document.body.appendChild(form);
            form.submit();
        },

        initAuditDiff: function (auditId) {
            if (!auditId) return;

            fetch(`?handler=AuditDiff&id=${auditId}`)
                .then(response => response.json())
                .then(data => {
                    if (!data || !data.oldJson || !data.newJson) return;

                    const oldObj = JSON.parse(data.oldJson);
                    const newObj = JSON.parse(data.newJson);

                    const delta = jsondiffpatch.diff(oldObj, newObj);
                    if (!delta) {
                        document.getElementById('diffContainer').innerHTML = '<p class="text-muted">Değişiklik bulunamadı.</p>';
                        return;
                    }

                    const html = jsondiffpatch.formatters.html.format(delta, oldObj);
                    document.getElementById('diffContainer').innerHTML = html;
                })
                .catch(err => {
                    console.error('Diff yüklenemedi:', err);
                    SecurityAdmin.showToast('Diff yüklenemedi.', 'error');
                });
        },

        initPolicyTestValidation: function () {
            const passwordInput = document.getElementById('testPassword');
            const validateBtn = document.getElementById('validateBtn');

            if (!passwordInput || !validateBtn) return;

            let debounceTimer;
            passwordInput.addEventListener('input', function () {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    const password = passwordInput.value;
                    if (password.length > 0) {
                        SecurityAdmin.validatePasswordLive(password);
                    }
                }, 500);
            });
        },

        validatePasswordLive: function (password) {
            if (!password) return;

            const appId = document.querySelector('input[name="ApplicationId"]')?.value || 1;

            fetch('/Admin/Security/PolicyTest?handler=ValidateLive', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ password, applicationId: appId })
            })
                .then(response => response.json())
                .then(result => SecurityAdmin.displayValidationResults(result))
                .catch(err => console.error('Validation failed:', err));
        },

        displayValidationResults: function (result) {
            const strengthBar = document.getElementById('strengthBar');
            const rulesContainer = document.getElementById('rulesContainer');

            if (!strengthBar || !rulesContainer) return;

            // Strength bar
            const score = result.strengthScore || 0;
            strengthBar.style.width = score + '%';
            strengthBar.className = 'progress-bar';

            if (score < 40) strengthBar.classList.add('bg-danger');
            else if (score < 70) strengthBar.classList.add('bg-warning');
            else strengthBar.classList.add('bg-success');

            strengthBar.textContent = score + '%';

            // Rules
            rulesContainer.innerHTML = '';
            if (result.errors && result.errors.length > 0) {
                result.errors.forEach(error => {
                    const item = document.createElement('div');
                    item.className = 'rule-item text-danger';
                    item.innerHTML = '<i class="fas fa-times-circle"></i> ' + error;
                    rulesContainer.appendChild(item);
                });
            } else {
                const item = document.createElement('div');
                item.className = 'rule-item text-success';
                item.innerHTML = '<i class="fas fa-check-circle"></i> Tüm kurallar sağlandı';
                rulesContainer.appendChild(item);
            }

            if (result.pwnedCount > 0) {
                const warning = document.createElement('div');
                warning.className = 'alert alert-warning mt-2';
                warning.textContent = `⚠ Bu parola ${result.pwnedCount} kez veri ihlalinde görüldü!`;
                rulesContainer.appendChild(warning);
            }
        },

        showToast: function (message, type) {
            const container = document.getElementById('toastContainer');
            if (!container) return;

            const toast = document.createElement('div');
            toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : 'success'} border-0`;
            toast.setAttribute('role', 'alert');
            toast.innerHTML = `
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            `;

            container.appendChild(toast);
            const bsToast = new bootstrap.Toast(toast);
            bsToast.show();

            toast.addEventListener('hidden.bs.toast', () => toast.remove());
        },

        exportBlacklistCsv: function () {
            const appId = document.querySelector('input[name="ApplicationId"]')?.value || 1;
            window.location.href = `/Admin/Security/Blacklist?handler=Export&applicationId=${appId}`;
        }
    };
})();