// Main application logic

const PAGE_SIZE = 20;
let allKeys = [];
let currentPage = 1;
let searchQuery = '';

document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

function initializeApp() {
    const loginForm = document.getElementById('login-form');
    if (loginForm) loginForm.addEventListener('submit', handleLogin);

    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', handleLogout);

    const createKeyBtn = document.getElementById('create-key-btn');
    if (createKeyBtn) createKeyBtn.addEventListener('click', handleCreateKey);

    const refreshBtn = document.getElementById('refresh-btn');
    if (refreshBtn) refreshBtn.addEventListener('click', loadKeys);

    const keyForm = document.getElementById('key-form');
    if (keyForm) keyForm.addEventListener('submit', handleKeyFormSubmit);

    const keyValueTextarea = document.getElementById('key-value');
    if (keyValueTextarea) {
        keyValueTextarea.addEventListener('input', updateLineNumbers);
        keyValueTextarea.addEventListener('scroll', syncLineNumberScroll);
        keyValueTextarea.addEventListener('keydown', updateLineNumbers);
        window.addEventListener('resize', updateLineNumbers);
        updateLineNumbers();
    }

    const languageSelect = document.getElementById('language-select');
    const copyCodeBtn = document.getElementById('copy-code-btn');

    if (languageSelect) {
        languageSelect.addEventListener('change', updateCodeSample);
        updateCodeSample();
    }

    if (copyCodeBtn) copyCodeBtn.addEventListener('click', copyCodeToClipboard);

    const searchInput = document.getElementById('search-keys');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            searchQuery = e.target.value;
            currentPage = 1;
            renderKeysPage();
        });
    }

    const codePre = document.getElementById('code-pre');
    if (codePre) {
        codePre.addEventListener('scroll', () => {
            const lineNums = document.getElementById('code-line-numbers');
            if (lineNums) lineNums.scrollTop = codePre.scrollTop;
        });
    }

    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) themeToggle.addEventListener('click', toggleTheme);

    initializeTheme();

    if (auth.getCurrentPage() === 'dashboard') loadKeys();
}

// ── Toast ────────────────────────────────────────────────────────────────────

function showToast(message, type = 'info') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 3100);
}

// ── Confirm modal ─────────────────────────────────────────────────────────────

function showConfirm(message, title = 'Confirm') {
    return new Promise(resolve => {
        document.getElementById('confirm-title').textContent = title;
        document.getElementById('confirm-message').textContent = message;
        const modal = document.getElementById('confirm-modal');
        modal.classList.add('active');

        const ok = document.getElementById('confirm-ok');
        const cancel = document.getElementById('confirm-cancel');

        function cleanup(result) {
            modal.classList.remove('active');
            ok.removeEventListener('click', onOk);
            cancel.removeEventListener('click', onCancel);
            resolve(result);
        }
        function onOk()     { cleanup(true);  }
        function onCancel() { cleanup(false); }

        ok.addEventListener('click', onOk);
        cancel.addEventListener('click', onCancel);
    });
}

// ── Auth ─────────────────────────────────────────────────────────────────────

async function handleLogin(e) {
    e.preventDefault();
    const tokenInput = document.getElementById('access-token');
    const errorEl    = document.getElementById('login-error');
    errorEl.style.display = 'none';

    if (!(await auth.login(tokenInput.value, errorEl))) return;
    tokenInput.value = '';
    loadKeys();
}

function handleLogout() {
    allKeys = [];
    currentPage = 1;
    searchQuery = '';
    const searchInput = document.getElementById('search-keys');
    if (searchInput) searchInput.value = '';
    setStorageName(null);
    auth.logout();
}

// ── Keys ─────────────────────────────────────────────────────────────────────

async function loadKeys() {
    try {
        const data = await api.getKeys();
        allKeys = data.keys || [];
        currentPage = 1;
        renderKeysPage();
    } catch (error) {
        console.error('Failed to load keys:', error);
        if (error.message.includes('access token')) {
            auth.logout();
        } else {
            showToast('Failed to load keys: ' + error.message, 'error');
        }
    }
}

function renderKeysPage() {
    const q = searchQuery.trim().toLowerCase();
    const filtered = q ? allKeys.filter(k => k.key.toLowerCase().includes(q)) : allKeys;

    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * PAGE_SIZE;
    renderKeys(filtered.slice(start, start + PAGE_SIZE), q && filtered.length === 0);
    renderPagination(filtered.length, totalPages);
}

function renderPagination(totalItems, totalPages) {
    const container = document.getElementById('pagination');
    if (!container) return;

    if (totalItems <= PAGE_SIZE) {
        container.style.display = 'none';
        return;
    }
    container.style.display = 'flex';

    // Collect page numbers to show: always first/last + window of ±2 around current
    const show = new Set([1, totalPages]);
    for (let p = Math.max(1, currentPage - 2); p <= Math.min(totalPages, currentPage + 2); p++) show.add(p);
    const pages = [...show].sort((a, b) => a - b);

    let btnHTML = '';
    let prev = 0;
    for (const p of pages) {
        if (p - prev > 1) btnHTML += `<span class="pagination-ellipsis">…</span>`;
        btnHTML += `<button class="page-btn${p === currentPage ? ' active' : ''}" data-page="${p}">${p}</button>`;
        prev = p;
    }

    container.innerHTML = `
        <button class="page-btn page-nav" id="prev-page-btn" ${currentPage === 1 ? 'disabled' : ''}>&#8249;</button>
        ${btnHTML}
        <button class="page-btn page-nav" id="next-page-btn" ${currentPage === totalPages ? 'disabled' : ''}>&#8250;</button>
        <div class="page-jump">
            <input id="page-input" type="number" min="1" max="${totalPages}" value="${currentPage}">
            <span>/ ${totalPages}</span>
        </div>
    `;

    container.querySelectorAll('.page-btn[data-page]').forEach(btn => {
        btn.addEventListener('click', () => { currentPage = +btn.dataset.page; renderKeysPage(); });
    });
    container.querySelector('#prev-page-btn').addEventListener('click', () => {
        if (currentPage > 1) { currentPage--; renderKeysPage(); }
    });
    container.querySelector('#next-page-btn').addEventListener('click', () => {
        if (currentPage < totalPages) { currentPage++; renderKeysPage(); }
    });
    container.querySelector('#page-input').addEventListener('change', (e) => {
        const p = parseInt(e.target.value, 10);
        if (p >= 1 && p <= totalPages) { currentPage = p; renderKeysPage(); }
        else e.target.value = currentPage;
    });
}

function renderKeys(keys, isSearchEmpty = false) {
    const keyList = document.getElementById('key-list');

    if (!keys || keys.length === 0) {
        keyList.innerHTML = isSearchEmpty
            ? '<p class="empty-state">No keys match your search.</p>'
            : '<p class="empty-state">No keys yet. Create your first key-value pair!</p>';
        return;
    }

    keyList.innerHTML = keys.map(key => `
        <div class="key-item" data-key="${escapeHtml(key.key)}">
            <div class="key-info">
                <strong>${escapeHtml(key.key)}</strong>
                <div>
                    <span class="version">v${key.version}</span>
                    <span class="modified">${formatDate(key.lastModified)}</span>
                </div>
            </div>
            <div class="key-actions">
                <button class="btn-icon" onclick="editKey('${escapeHtml(key.key)}')" title="Edit">
                    <img src="svg/edit.svg" class="icon" alt="Edit">
                </button>
                <button class="btn-icon" onclick="viewHistory('${escapeHtml(key.key)}')" title="History">
                    <img src="svg/versions.svg" class="icon icon-history" alt="History">
                </button>
                <button class="btn-icon" onclick="deleteKey('${escapeHtml(key.key)}')" title="Delete">
                    <img src="svg/remove.svg" class="icon icon-remove" alt="Delete">
                </button>
            </div>
        </div>
    `).join('');
}

function handleCreateKey() {
    document.getElementById('modal-title').textContent = 'Create Key';
    document.getElementById('key-name').value = '';
    document.getElementById('key-name').disabled = false;
    document.getElementById('key-value').value = '';
    document.getElementById('key-form').dataset.mode = 'create';
    document.getElementById('key-form').dataset.originalKey = '';
    openModal();
}

async function handleEditKey(keyName) {
    try {
        const data = await api.getKeyValue(keyName);
        document.getElementById('modal-title').textContent = 'Edit Key';
        document.getElementById('key-name').value = keyName;
        document.getElementById('key-name').disabled = true;
        document.getElementById('key-value').value = data.value;
        document.getElementById('key-form').dataset.mode = 'edit';
        document.getElementById('key-form').dataset.originalKey = keyName;
        openModal();
    } catch (error) {
        showToast('Error loading key value: ' + error.message, 'error');
    }
}

async function handleKeyFormSubmit(e) {
    e.preventDefault();
    const mode    = document.getElementById('key-form').dataset.mode;
    const keyName = document.getElementById('key-name').value;
    const value   = document.getElementById('key-value').value;

    const form      = document.getElementById('key-form');
    const saveBtn   = form.querySelector('button[type="submit"]');
    const cancelBtn = form.querySelector('button[type="button"]');

    saveBtn.disabled   = true;
    cancelBtn.disabled = true;
    const originalText = saveBtn.textContent;
    saveBtn.textContent = 'Saving…';

    try {
        if (mode === 'create') {
            await api.createKey(keyName, value);
            showToast('Key created successfully!', 'success');
        } else if (mode === 'edit') {
            await api.updateKey(keyName, value);
            showToast('Key updated successfully!', 'success');
        }
        closeModal();
        loadKeys();
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
        saveBtn.disabled   = false;
        cancelBtn.disabled = false;
        saveBtn.textContent = originalText;
    }
}

function editKey(keyName) {
    handleEditKey(keyName);
}

async function viewHistory(keyName) {
    try {
        const data = await api.getKeyHistory(keyName);
        renderHistory(data.history);
        openHistoryModal();
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
    }
}

function renderHistory(history) {
    const historyContent = document.getElementById('history-content');
    if (!history || history.length === 0) {
        historyContent.innerHTML = '<p class="empty-state">No history available.</p>';
        return;
    }
    historyContent.innerHTML = '<div class="history-content">' +
        history.map(version => `
            <div class="version-item">
                <div class="version-header">
                    <span class="version-number">Version ${version.version}</span>
                    <span class="version-date">${formatDate(version.createdAt)}</span>
                </div>
                <div class="version-value">${escapeHtml(version.value)}</div>
            </div>
        `).join('') +
        '</div>';
}

async function deleteKey(keyName) {
    const confirmed = await showConfirm(
        `Are you sure you want to delete '${keyName}'? This will remove all versions.`,
        'Delete Key'
    );
    if (!confirmed) return;

    try {
        await api.deleteKey(keyName);
        showToast('Key deleted successfully!', 'success');
        loadKeys();
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
    }
}

function updateCodeSample() {
    const language = document.getElementById('language-select').value;
    codeSamples.updateSample(language, 'read');
}

function copyCodeToClipboard() {
    const codeContent = document.getElementById('code-content').textContent;
    navigator.clipboard.writeText(codeContent).then(() => {
        const copyBtn = document.getElementById('copy-code-btn');
        const original = copyBtn.innerHTML;
        copyBtn.innerHTML = '<img src="svg/done.svg" class="icon icon-btn" alt=""> Copied!';
        setTimeout(() => { copyBtn.innerHTML = original; }, 2000);
    }).catch(err => {
        showToast('Failed to copy code: ' + err, 'error');
    });
}

// ── Modals ────────────────────────────────────────────────────────────────────

function openModal() {
    document.getElementById('key-editor-modal').classList.add('active');
    requestAnimationFrame(updateLineNumbers);
}

function closeModal() {
    document.getElementById('key-editor-modal').classList.remove('active');
    const form    = document.getElementById('key-form');
    const saveBtn = form.querySelector('button[type="submit"]');
    const cancelBtn = form.querySelector('button[type="button"]');
    saveBtn.disabled    = false;
    cancelBtn.disabled  = false;
    saveBtn.textContent = 'Save';
}

function openHistoryModal() {
    document.getElementById('history-modal').classList.add('active');
}

function closeHistoryModal() {
    document.getElementById('history-modal').classList.remove('active');
}

// ── Utilities ────────────────────────────────────────────────────────────────

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleString();
}

// Make functions globally available
window.editKey          = editKey;
window.viewHistory      = viewHistory;
window.deleteKey        = deleteKey;
window.closeModal        = closeModal;
window.closeHistoryModal = closeHistoryModal;

// ── Line Number Editor ────────────────────────────────────────────────────────

function updateLineNumbers() {
    const textarea     = document.getElementById('key-value');
    const lineNumbersEl = document.getElementById('line-numbers');
    if (!textarea || !lineNumbersEl) return;

    const style = getComputedStyle(textarea);
    const lineHeight     = parseFloat(style.lineHeight);
    const paddingLeft    = parseFloat(style.paddingLeft);
    const paddingRight   = parseFloat(style.paddingRight);
    const availableWidth = textarea.clientWidth - paddingLeft - paddingRight;

    if (!updateLineNumbers._canvas) {
        updateLineNumbers._canvas = document.createElement('canvas');
    }
    const ctx = updateLineNumbers._canvas.getContext('2d');
    ctx.font = style.font;

    const lines = textarea.value.split('\n');
    let html = '';

    for (let i = 0; i < lines.length; i++) {
        const text = lines[i];
        let visualRows = 1;

        if (text.length > 0 && availableWidth > 0) {
            let currentWidth = 0;
            for (let j = 0; j < text.length; j++) {
                const charWidth = ctx.measureText(text[j]).width;
                currentWidth += charWidth;
                if (currentWidth > availableWidth) {
                    visualRows++;
                    currentWidth = charWidth;
                }
            }
        }

        html += `<span style="height:${lineHeight * visualRows}px">${i + 1}</span>`;
    }

    lineNumbersEl.innerHTML = html;
    syncLineNumberScroll();
}

function syncLineNumberScroll() {
    const textarea      = document.getElementById('key-value');
    const lineNumbersEl = document.getElementById('line-numbers');
    if (!textarea || !lineNumbersEl) return;
    lineNumbersEl.scrollTop = textarea.scrollTop;
}

// ── Storage name ──────────────────────────────────────────────────────────────

function setStorageName(name) {
    const el = document.getElementById('storage-name');
    if (!el) return;
    el.textContent = name ?? '';
    el.style.display = name ? '' : 'none';
}

// ── Theme ─────────────────────────────────────────────────────────────────────

function initializeTheme() {
    const savedTheme = localStorage.getItem('theme');
    const isDark = savedTheme === 'dark';
    document.body.classList.toggle('dark-mode', isDark);
    document.getElementById('theme-toggle')?.classList.toggle('is-dark', isDark);
}

function toggleTheme() {
    const isDark = document.body.classList.toggle('dark-mode');
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    document.getElementById('theme-toggle')?.classList.toggle('is-dark', isDark);
}
