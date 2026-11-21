// ── Toast Notifications ──────────────────────────────────────────────────────
window.showToast = function(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `<span>${icons[type] || '💬'}</span><span>${message}</span>`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity .4s';
        setTimeout(() => toast.remove(), 400);
    }, 3500);
};

// ── Auto-show TempData toasts ─────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const s = document.getElementById('__toast_success');
    const e = document.getElementById('__toast_error');
    const w = document.getElementById('__toast_warning');
    const i = document.getElementById('__toast_info');

    if (s && s.dataset.msg) showToast(s.dataset.msg, 'success');
    if (e && e.dataset.msg) showToast(e.dataset.msg, 'error');
    if (w && w.dataset.msg) showToast(w.dataset.msg, 'warning');
    if (i && i.dataset.msg) showToast(i.dataset.msg, 'info');
});

// ── Sidebar toggle ────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const hamburger = document.getElementById('hamburger');
    const sidebar   = document.getElementById('sidebar');
    if (hamburger && sidebar) {
        hamburger.addEventListener('click', () => sidebar.classList.toggle('open'));
    }
});

// ── Dropdown ──────────────────────────────────────────────────────────────────
document.addEventListener('click', e => {
    document.querySelectorAll('.dropdown.open').forEach(d => {
        if (!d.contains(e.target)) d.classList.remove('open');
    });

    const trigger = e.target.closest('[data-dropdown]');
    if (trigger) {
        const dropdown = trigger.closest('.dropdown');
        if (dropdown) dropdown.classList.toggle('open');
    }
});

// ── Tabs ──────────────────────────────────────────────────────────────────────
document.addEventListener('click', e => {
    const btn = e.target.closest('.tab-btn');
    if (!btn) return;

    const container = btn.closest('[data-tabs]');
    if (!container) return;

    container.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    const target = btn.dataset.tab;
    container.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
    const pane = container.querySelector('#' + target);
    if (pane) pane.classList.add('active');
});

// ── Accordion ─────────────────────────────────────────────────────────────────
document.addEventListener('click', e => {
    const header = e.target.closest('.accordion-header');
    if (!header) return;
    const item = header.closest('.accordion-item');
    if (item) item.classList.toggle('open');
});

// ── Modal ─────────────────────────────────────────────────────────────────────
window.openModal = id => {
    const el = document.getElementById(id);
    if (el) el.classList.add('open');
};

window.closeModal = id => {
    const el = document.getElementById(id);
    if (el) el.classList.remove('open');
};

document.addEventListener('click', e => {
    if (e.target.classList.contains('modal-backdrop')) {
        e.target.classList.remove('open');
    }

    const btn = e.target.closest('[data-modal]');
    if (btn) openModal(btn.dataset.modal);

    const close = e.target.closest('[data-close-modal]');
    if (close) closeModal(close.dataset.closeModal);
});

// ── Confirm Delete ────────────────────────────────────────────────────────────
document.addEventListener('submit', e => {
    const form = e.target;
    if (form.dataset.confirm) {
        if (!confirm(form.dataset.confirm)) e.preventDefault();
    }
});

// ── Image Preview ─────────────────────────────────────────────────────────────
document.addEventListener('change', e => {
    const input = e.target;
    if (input.dataset.preview && input.files && input.files[0]) {
        const preview = document.getElementById(input.dataset.preview);
        if (preview) preview.src = URL.createObjectURL(input.files[0]);
    }
});
