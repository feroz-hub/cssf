/**
 * HCL Security Admin - JavaScript Engine
 * Vanilla JS namespace providing UI interactions for the admin panel.
 */
const HCL = (() => {
    'use strict';

    // ---------------------------------------------------------------------------
    // Private state
    // ---------------------------------------------------------------------------
    const SIDEBAR_KEY = 'hcl_sidebar_collapsed';
    let _confirmState = null;
    const _debounceTimers = {};

    // ---------------------------------------------------------------------------
    // Internal helpers
    // ---------------------------------------------------------------------------

    /**
     * Retrieve the CSRF / antiforgery token from the page.
     * Looks for: <meta name="csrf-token">, <input name="__RequestVerificationToken">,
     * or a cookie named XSRF-TOKEN.
     */
    function _getCsrfToken() {
        const meta = document.querySelector('meta[name="csrf-token"]');
        if (meta && meta.content) return meta.content;

        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        if (input && input.value) return input.value;

        const match = document.cookie.match(/(?:^|;\s*)XSRF-TOKEN=([^;]*)/);
        if (match) return decodeURIComponent(match[1]);

        return null;
    }

    /**
     * Build standard headers for fetch requests.
     */
    function _buildHeaders(extra) {
        const headers = {
            'Accept': 'application/json',
            ...extra
        };
        const token = _getCsrfToken();
        if (token) {
            headers['RequestVerificationToken'] = token;
            headers['X-XSRF-TOKEN'] = token;
        }
        return headers;
    }

    /**
     * Create a DOM element from an HTML string.
     */
    function _createElement(html) {
        const tpl = document.createElement('template');
        tpl.innerHTML = html.trim();
        return tpl.content.firstChild;
    }

    /**
     * Generate a simple unique ID.
     */
    function _uid() {
        return 'hcl_' + Math.random().toString(36).slice(2, 10);
    }

    /**
     * Resolve the currently open confirm dialog, if any.
     */
    function _resolveConfirm(confirmed) {
        if (!_confirmState) return;

        const { resolve, onConfirm } = _confirmState;
        _confirmState = null;

        const confirmBtn = document.getElementById('hclConfirmBtn');
        if (confirmBtn) {
            confirmBtn.onclick = null;
        }

        try {
            if (confirmed && typeof onConfirm === 'function') {
                onConfirm();
            }
        } catch (err) {
            setTimeout(() => { throw err; }, 0);
        } finally {
            resolve(confirmed);
        }
    }

    // ---------------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------------
    const api = {

        // -----------------------------------------------------------------------
        // Sidebar
        // -----------------------------------------------------------------------

        /**
         * Toggle sidebar collapsed state and persist to localStorage.
         */
        toggleSidebar() {
            const sidebar = document.getElementById('sidebar');
            if (!sidebar) return;
            sidebar.classList.toggle('collapsed');
            const overlay = document.getElementById('sidebarOverlay');
            if (overlay) {
                overlay.classList.toggle('active', !sidebar.classList.contains('collapsed') && window.innerWidth < 1024);
            }
            try {
                localStorage.setItem(SIDEBAR_KEY, sidebar.classList.contains('collapsed') ? '1' : '0');
            } catch (_) { /* storage unavailable */ }
        },

        /**
         * Restore sidebar state from localStorage.
         */
        restoreSidebar() {
            try {
                const collapsed = localStorage.getItem(SIDEBAR_KEY);
                const sidebar = document.getElementById('sidebar');
                if (!sidebar) return;
                if (collapsed === '1') {
                    sidebar.classList.add('collapsed');
                } else if (collapsed === '0') {
                    sidebar.classList.remove('collapsed');
                }
            } catch (_) { /* storage unavailable */ }
        },

        // -----------------------------------------------------------------------
        // Toast notifications
        // -----------------------------------------------------------------------

        /**
         * Display a toast notification.
         * @param {string} message - Text to display.
         * @param {'info'|'success'|'warning'|'error'} type - Toast variant.
         * @param {number} duration - Auto-dismiss time in ms (0 = sticky).
         */
        toast(message, type = 'info', duration = 5000) {
            const container = document.getElementById('toastContainer');
            if (!container) return;

            const iconMap = {
                success: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>',
                error:   '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>',
                warning: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
                info:    '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
            };

            const id = _uid();
            const toast = _createElement(`
                <div class="hcl-toast hcl-toast-${type}" id="${id}" role="alert">
                    <span class="hcl-toast-icon">${iconMap[type] || iconMap.info}</span>
                    <span class="hcl-toast-message">${message}</span>
                    <button class="hcl-toast-close" aria-label="Dismiss">&times;</button>
                </div>
            `);

            toast.querySelector('.hcl-toast-close').addEventListener('click', () => {
                api._dismissToast(id);
            });

            container.appendChild(toast);

            // Trigger enter animation
            requestAnimationFrame(() => toast.classList.add('hcl-toast-enter'));

            if (duration > 0) {
                setTimeout(() => api._dismissToast(id), duration);
            }

            return id;
        },

        /** @internal Remove a toast with exit animation. */
        _dismissToast(id) {
            const el = document.getElementById(id);
            if (!el) return;
            el.classList.add('hcl-toast-exit');
            el.addEventListener('animationend', () => el.remove(), { once: true });
            // Fallback removal if animationend never fires
            setTimeout(() => { if (el.parentNode) el.remove(); }, 500);
        },

        toastSuccess(msg, duration) { return api.toast(msg, 'success', duration); },
        toastError(msg, duration)   { return api.toast(msg, 'error', duration); },
        toastWarning(msg, duration) { return api.toast(msg, 'warning', duration); },
        toastInfo(msg, duration)    { return api.toast(msg, 'info', duration); },

        // -----------------------------------------------------------------------
        // Modal management
        // -----------------------------------------------------------------------

        /**
         * Open a modal by its backdrop element ID.
         */
        openModal(id) {
            const backdrop = document.getElementById(id);
            if (!backdrop) return;
            backdrop.classList.remove('hidden');
            backdrop.classList.add('active');
            document.body.style.overflow = 'hidden';

            // Focus first focusable element inside modal
            requestAnimationFrame(() => {
                const focusable = backdrop.querySelector('input, select, textarea, button:not(.hcl-btn-icon)');
                if (focusable) focusable.focus();
            });
        },

        /**
         * Close a modal by its backdrop element ID.
         */
        closeModal(id, options = {}) {
            const backdrop = document.getElementById(id);
            if (!backdrop) return;
            backdrop.classList.remove('active');
            backdrop.classList.add('hidden');
            // Restore body scroll only if no other modals are open
            if (!document.querySelector('.hcl-modal-backdrop.active')) {
                document.body.style.overflow = '';
            }

            if (id === 'hclConfirmModal') {
                _resolveConfirm(options.confirmResult === true);
            }
        },

        // -----------------------------------------------------------------------
        // Confirm dialog
        // -----------------------------------------------------------------------

        /**
         * Show a confirmation dialog.
         * @param {string} title - Dialog title.
         * @param {string} message - Dialog body text.
         * @param {Function} onConfirm - Optional callback executed on confirmation.
         */
        confirm(title, message, onConfirm) {
            const modalId = 'hclConfirmModal';
            const titleEl = document.getElementById('hclConfirmTitle');
            const msgEl = document.getElementById('hclConfirmMessage');
            const btn = document.getElementById('hclConfirmBtn');

            if (!titleEl || !msgEl || !btn) {
                const confirmed = window.confirm([title, message].filter(Boolean).join('\n\n') || 'Confirm');
                if (confirmed && typeof onConfirm === 'function') {
                    onConfirm();
                }
                return Promise.resolve(confirmed);
            }

            if (_confirmState) {
                api.closeModal(modalId);
            }

            titleEl.textContent = title || 'Confirm';
            msgEl.textContent = message || '';

            return new Promise((resolve) => {
                _confirmState = { resolve, onConfirm };
                btn.onclick = () => api.closeModal(modalId, { confirmResult: true });
                api.openModal(modalId);
            });
        },

        // -----------------------------------------------------------------------
        // Form helpers
        // -----------------------------------------------------------------------

        /**
         * Submit a form via AJAX.
         * @param {string} formId - ID of the <form> element.
         * @param {string} url - Endpoint URL.
         * @param {Object} options - { method, onSuccess, onError, resetOnSuccess }
         */
        async submitForm(formId, url, options = {}) {
            const form = document.getElementById(formId);
            if (!form) {
                api.toastError('Form not found.');
                return;
            }

            const method = (options.method || form.method || 'POST').toUpperCase();
            const formData = new FormData(form);

            // Disable submit button to prevent double submission
            const submitBtn = form.querySelector('[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.dataset.originalText = submitBtn.textContent;
                submitBtn.textContent = 'Saving...';
            }

            try {
                const headers = _buildHeaders();
                // Do not set Content-Type for FormData (browser sets multipart boundary)
                const response = await fetch(url, { method, body: formData, headers });

                if (!response.ok) {
                    let errorMsg = `Request failed (${response.status})`;
                    try {
                        const errorData = await response.json();
                        errorMsg = errorData.message || errorData.title || errorMsg;
                    } catch (_) { /* not JSON */ }
                    throw new Error(errorMsg);
                }

                let data = null;
                const contentType = response.headers.get('content-type') || '';
                if (contentType.includes('application/json')) {
                    data = await response.json();
                }

                if (options.resetOnSuccess) form.reset();
                if (typeof options.onSuccess === 'function') {
                    options.onSuccess(data, response);
                } else {
                    api.toastSuccess('Saved successfully.');
                }
            } catch (err) {
                if (typeof options.onError === 'function') {
                    options.onError(err);
                } else {
                    api.toastError(err.message || 'An error occurred.');
                }
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = submitBtn.dataset.originalText || 'Save';
                }
            }
        },

        // -----------------------------------------------------------------------
        // API helpers
        // -----------------------------------------------------------------------

        /**
         * Generic fetch wrapper with CSRF, JSON handling, and error management.
         * @param {string} url - Request URL.
         * @param {string} method - HTTP method.
         * @param {Object|null} data - JSON body payload.
         * @returns {Promise<any>}
         */
        async apiRequest(url, method = 'GET', data = null) {
            const options = { method };
            const headers = _buildHeaders();

            if (data !== null && method !== 'GET') {
                headers['Content-Type'] = 'application/json';
                options.body = JSON.stringify(data);
            }

            options.headers = headers;

            const response = await fetch(url, options);

            if (!response.ok) {
                let errorMsg = `HTTP ${response.status}`;
                try {
                    const errBody = await response.json();
                    errorMsg = errBody.message || errBody.title || errorMsg;
                } catch (_) { /* not JSON */ }
                throw new Error(errorMsg);
            }

            const contentType = response.headers.get('content-type') || '';
            if (contentType.includes('application/json')) {
                return response.json();
            }

            return null;
        },

        // -----------------------------------------------------------------------
        // Table helpers
        // -----------------------------------------------------------------------

        /**
         * Initialise a responsive table: adds data-label attributes for mobile card layout.
         * @param {string} tableId - ID of the <table>.
         */
        initTable(tableId) {
            const table = document.getElementById(tableId);
            if (!table) return;

            const headers = Array.from(table.querySelectorAll('thead th')).map(th => th.textContent.trim());
            table.querySelectorAll('tbody tr').forEach(row => {
                row.querySelectorAll('td').forEach((td, i) => {
                    if (headers[i]) td.setAttribute('data-label', headers[i]);
                });
            });
        },

        // -----------------------------------------------------------------------
        // Tab helpers
        // -----------------------------------------------------------------------

        /**
         * Switch active tab within a tab group.
         * @param {string} tabGroup - Data-attribute value identifying the group.
         * @param {string} tabId - ID of the tab panel to activate.
         */
        switchTab(tabGroup, tabId) {
            // Deactivate all tabs in the group
            document.querySelectorAll(`[data-tab-group="${tabGroup}"]`).forEach(btn => {
                btn.classList.remove('active');
            });
            document.querySelectorAll(`[data-tab-panel="${tabGroup}"]`).forEach(panel => {
                panel.classList.remove('active');
                panel.style.display = 'none';
            });

            // Activate selected tab
            const btn = document.querySelector(`[data-tab-group="${tabGroup}"][data-tab-id="${tabId}"]`);
            if (btn) btn.classList.add('active');

            const panel = document.getElementById(tabId);
            if (panel) {
                panel.classList.add('active');
                panel.style.display = '';
            }
        },

        // -----------------------------------------------------------------------
        // Search / Filter
        // -----------------------------------------------------------------------

        /**
         * Bind a debounced search handler to an input field.
         * @param {string} inputId - ID of the input element.
         * @param {Function} callback - Called with the current value after debounce.
         * @param {number} debounceMs - Debounce interval in ms.
         */
        initSearch(inputId, callback, debounceMs = 300) {
            const input = document.getElementById(inputId);
            if (!input || typeof callback !== 'function') return;

            input.addEventListener('input', () => {
                clearTimeout(_debounceTimers[inputId]);
                _debounceTimers[inputId] = setTimeout(() => {
                    callback(input.value.trim());
                }, debounceMs);
            });
        },

        // -----------------------------------------------------------------------
        // Loading states
        // -----------------------------------------------------------------------

        /**
         * Show a loading spinner overlay on an element.
         */
        showLoading(elementId) {
            const el = document.getElementById(elementId);
            if (!el) return;
            el.classList.add('hcl-loading');
            // Prevent duplicate spinners
            if (!el.querySelector('.hcl-loading-spinner')) {
                const spinner = _createElement(
                    '<div class="hcl-loading-spinner"><div class="hcl-spinner"></div></div>'
                );
                el.style.position = el.style.position || 'relative';
                el.appendChild(spinner);
            }
        },

        /**
         * Remove loading spinner overlay from an element.
         */
        hideLoading(elementId) {
            const el = document.getElementById(elementId);
            if (!el) return;
            el.classList.remove('hcl-loading');
            const spinner = el.querySelector('.hcl-loading-spinner');
            if (spinner) spinner.remove();
        },

        // -----------------------------------------------------------------------
        // Pagination
        // -----------------------------------------------------------------------

        /**
         * Render pagination controls inside a container.
         * @param {string} containerId - ID of the container element.
         * @param {number} currentPage - Current active page (1-based).
         * @param {number} totalPages - Total number of pages.
         * @param {Function} onPageChange - Called with the new page number.
         */
        initPagination(containerId, currentPage, totalPages, onPageChange) {
            const container = document.getElementById(containerId);
            if (!container || totalPages < 2) return;

            container.innerHTML = '';

            const createBtn = (label, page, disabled, active) => {
                const btn = document.createElement('button');
                btn.className = 'hcl-btn hcl-btn-sm' + (active ? ' hcl-btn-primary' : ' hcl-btn-ghost');
                btn.textContent = label;
                btn.disabled = disabled;
                if (!disabled && !active) {
                    btn.addEventListener('click', () => {
                        if (typeof onPageChange === 'function') onPageChange(page);
                    });
                }
                return btn;
            };

            // Previous button
            container.appendChild(createBtn('\u2039', currentPage - 1, currentPage <= 1, false));

            // Page numbers with ellipsis
            const range = [];
            const delta = 2;
            for (let i = 1; i <= totalPages; i++) {
                if (i === 1 || i === totalPages || (i >= currentPage - delta && i <= currentPage + delta)) {
                    range.push(i);
                }
            }

            let last = 0;
            range.forEach(page => {
                if (last && page - last > 1) {
                    const ellipsis = document.createElement('span');
                    ellipsis.className = 'hcl-pagination-ellipsis';
                    ellipsis.textContent = '\u2026';
                    container.appendChild(ellipsis);
                }
                container.appendChild(createBtn(String(page), page, false, page === currentPage));
                last = page;
            });

            // Next button
            container.appendChild(createBtn('\u203A', currentPage + 1, currentPage >= totalPages, false));
        },

        // -----------------------------------------------------------------------
        // Clipboard
        // -----------------------------------------------------------------------

        /**
         * Copy text to clipboard and show a toast.
         */
        async copyToClipboard(text) {
            try {
                await navigator.clipboard.writeText(text);
                api.toastSuccess('Copied to clipboard.');
            } catch (_) {
                // Fallback for older browsers
                const textarea = document.createElement('textarea');
                textarea.value = text;
                textarea.style.cssText = 'position:fixed;left:-9999px';
                document.body.appendChild(textarea);
                textarea.select();
                try {
                    document.execCommand('copy');
                    api.toastSuccess('Copied to clipboard.');
                } catch (__) {
                    api.toastError('Failed to copy to clipboard.');
                }
                document.body.removeChild(textarea);
            }
        },

        // -----------------------------------------------------------------------
        // Date formatting
        // -----------------------------------------------------------------------

        /**
         * Format a date string as a locale date (e.g. "Mar 18, 2026").
         */
        formatDate(dateStr) {
            try {
                return new Date(dateStr).toLocaleDateString('en-US', {
                    year: 'numeric', month: 'short', day: 'numeric'
                });
            } catch (_) {
                return dateStr || '';
            }
        },

        /**
         * Format a date string as a locale date + time (e.g. "Mar 18, 2026 2:30 PM").
         */
        formatDateTime(dateStr) {
            try {
                return new Date(dateStr).toLocaleString('en-US', {
                    year: 'numeric', month: 'short', day: 'numeric',
                    hour: 'numeric', minute: '2-digit'
                });
            } catch (_) {
                return dateStr || '';
            }
        },

        /**
         * Return a human-readable relative time string (e.g. "3 minutes ago").
         */
        timeAgo(dateStr) {
            try {
                const seconds = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
                if (seconds < 0) return 'just now';

                const intervals = [
                    { label: 'year',   seconds: 31536000 },
                    { label: 'month',  seconds: 2592000 },
                    { label: 'week',   seconds: 604800 },
                    { label: 'day',    seconds: 86400 },
                    { label: 'hour',   seconds: 3600 },
                    { label: 'minute', seconds: 60 }
                ];

                for (const interval of intervals) {
                    const count = Math.floor(seconds / interval.seconds);
                    if (count >= 1) {
                        return `${count} ${interval.label}${count !== 1 ? 's' : ''} ago`;
                    }
                }

                return 'just now';
            } catch (_) {
                return dateStr || '';
            }
        },

        // -----------------------------------------------------------------------
        // Keyboard shortcuts
        // -----------------------------------------------------------------------

        /** @internal Bind global keyboard shortcuts. */
        _bindKeyboard() {
            document.addEventListener('keydown', (e) => {
                // Escape closes the topmost open modal
                if (e.key === 'Escape') {
                    const openModals = document.querySelectorAll('.hcl-modal-backdrop.active');
                    if (openModals.length > 0) {
                        const last = openModals[openModals.length - 1];
                        api.closeModal(last.id);
                        e.preventDefault();
                    }
                }

                // Cmd/Ctrl+K to focus search if one exists
                if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                    const search = document.querySelector('.hcl-search-input, [data-hcl-search]');
                    if (search) {
                        e.preventDefault();
                        search.focus();
                    }
                }
            });

            // Close modals on backdrop click
            document.addEventListener('click', (e) => {
                if (e.target.classList.contains('hcl-modal-backdrop') && e.target.classList.contains('active')) {
                    api.closeModal(e.target.id);
                }
            });
        },

        // -----------------------------------------------------------------------
        // Init
        // -----------------------------------------------------------------------

        /**
         * Initialise the HCL admin shell. Called automatically on DOMContentLoaded.
         */
        init() {
            api.restoreSidebar();
            api._bindKeyboard();

            // Auto-initialise any tables with [data-hcl-table]
            document.querySelectorAll('[data-hcl-table]').forEach(table => {
                if (table.id) api.initTable(table.id);
            });

            // Handle responsive sidebar: on mobile, start collapsed
            if (window.innerWidth < 1024) {
                const sidebar = document.getElementById('sidebar');
                if (sidebar) sidebar.classList.add('collapsed');
            }

            // Handle window resize
            let resizeTimer;
            window.addEventListener('resize', () => {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(() => {
                    const sidebar = document.getElementById('sidebar');
                    const overlay = document.getElementById('sidebarOverlay');
                    if (window.innerWidth >= 1024 && overlay) {
                        overlay.classList.remove('active');
                    }
                }, 150);
            });
        }
    };

    return api;
})();

// Boot
document.addEventListener('DOMContentLoaded', () => HCL.init());
