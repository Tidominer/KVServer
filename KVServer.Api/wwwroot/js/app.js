// Main application logic
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

function initializeApp() {
    // Setup login form
    const loginForm = document.getElementById('login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    // Setup logout button
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', handleLogout);
    }

    // Setup toolbar buttons
    const createKeyBtn = document.getElementById('create-key-btn');
    if (createKeyBtn) {
        createKeyBtn.addEventListener('click', handleCreateKey);
    }

    const refreshBtn = document.getElementById('refresh-btn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', loadKeys);
    }

    // Setup key form
    const keyForm = document.getElementById('key-form');
    if (keyForm) {
        keyForm.addEventListener('submit', handleKeyFormSubmit);
    }

    // Setup code samples controls
    const languageSelect = document.getElementById('language-select');
    const actionSelect = document.getElementById('action-select');
    const copyCodeBtn = document.getElementById('copy-code-btn');

    if (languageSelect && actionSelect) {
        languageSelect.addEventListener('change', updateCodeSample);
        actionSelect.addEventListener('change', updateCodeSample);

        // Initialize with default sample
        updateCodeSample();
    }

    if (copyCodeBtn) {
        copyCodeBtn.addEventListener('click', copyCodeToClipboard);
    }

    // Setup theme toggle
    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', toggleTheme);
    }

    // Initialize theme from localStorage
    initializeTheme();

    // Load keys if on dashboard
    if (auth.getCurrentPage() === 'dashboard') {
        loadKeys();
    }
}

async function handleLogin(e) {
    e.preventDefault();
    const token = document.getElementById('access-token').value;

    if (await auth.login(token)) {
        document.getElementById('access-token').value = '';
        loadKeys();
    }
}

function handleLogout() {
    auth.logout();
}

async function loadKeys() {
    try {
        const data = await api.getKeys();
        renderKeys(data.keys);
    } catch (error) {
        console.error('Failed to load keys:', error);
        if (error.message.includes('access token')) {
            auth.logout();
        } else {
            alert('Failed to load keys: ' + error.message);
        }
    }
}

function renderKeys(keys) {
    const keyList = document.getElementById('key-list');

    if (!keys || keys.length === 0) {
        keyList.innerHTML = '<p class="empty-state">No keys yet. Create your first key-value pair!</p>';
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
                <button class="btn-icon" onclick="viewKey('${escapeHtml(key.key)}')" title="View">
                    👁️
                </button>
                <button class="btn-icon" onclick="editKey('${escapeHtml(key.key)}')" title="Edit">
                    ✏️
                </button>
                <button class="btn-icon" onclick="viewHistory('${escapeHtml(key.key)}')" title="History">
                    📜
                </button>
                <button class="btn-icon" onclick="deleteKey('${escapeHtml(key.key)}')" title="Delete">
                    🗑️
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
        // Fetch current value to pre-fill the form
        const data = await api.getKeyValue(keyName);

        document.getElementById('modal-title').textContent = 'Edit Key';
        document.getElementById('key-name').value = keyName;
        document.getElementById('key-name').disabled = true;
        document.getElementById('key-value').value = data.value; // Pre-fill with current value
        document.getElementById('key-form').dataset.mode = 'edit';
        document.getElementById('key-form').dataset.originalKey = keyName;

        openModal();
    } catch (error) {
        alert('Error loading key value: ' + error.message);
    }
}

async function handleKeyFormSubmit(e) {
    e.preventDefault();

    const mode = document.getElementById('key-form').dataset.mode;
    const keyName = document.getElementById('key-name').value;
    const value = document.getElementById('key-value').value;

    try {
        if (mode === 'create') {
            await api.createKey(keyName, value);
            alert('Key created successfully!');
        } else if (mode === 'edit') {
            await api.updateKey(keyName, value);
            alert('Key updated successfully!');
        }

        closeModal();
        loadKeys();
    } catch (error) {
        alert('Error: ' + error.message);
    }
}

async function viewKey(keyName) {
    try {
        const data = await api.getKeyValue(keyName);
        alert(`Key: ${keyName}\nValue: ${data.value}\nVersion: ${data.version}`);
    } catch (error) {
        alert('Error: ' + error.message);
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
        alert('Error: ' + error.message);
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
    if (!confirm(`Are you sure you want to delete '${keyName}'? This will remove all versions.`)) {
        return;
    }

    try {
        await api.deleteKey(keyName);
        alert('Key deleted successfully!');
        loadKeys();
    } catch (error) {
        alert('Error: ' + error.message);
    }
}

function updateCodeSample() {
    const language = document.getElementById('language-select').value;
    const action = document.getElementById('action-select').value;
    codeSamples.updateSample(language, action);
}

function copyCodeToClipboard() {
    const codeContent = document.getElementById('code-content').textContent;

    navigator.clipboard.writeText(codeContent).then(() => {
        const copyBtn = document.getElementById('copy-code-btn');
        copyBtn.textContent = 'Copied!';
        setTimeout(() => {
            copyBtn.textContent = 'Copy';
        }, 2000);
    }).catch(err => {
        alert('Failed to copy code: ' + err);
    });
}

// Modal functions
function openModal() {
    document.getElementById('key-editor-modal').classList.add('active');
}

function closeModal() {
    document.getElementById('key-editor-modal').classList.remove('active');
}

function openHistoryModal() {
    document.getElementById('history-modal').classList.add('active');
}

function closeHistoryModal() {
    document.getElementById('history-modal').classList.remove('active');
}

// Utility functions
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString();
}

// Make functions globally available
window.viewKey = viewKey;
window.editKey = editKey;
window.viewHistory = viewHistory;
window.deleteKey = deleteKey;
window.closeModal = closeModal;
window.closeHistoryModal = closeHistoryModal;

// Theme Management
function initializeTheme() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.body.classList.add('dark-mode');
        updateThemeIcon(true);
    } else {
        document.body.classList.remove('dark-mode');
        updateThemeIcon(false);
    }
}

function toggleTheme() {
    const isDarkMode = document.body.classList.toggle('dark-mode');
    localStorage.setItem('theme', isDarkMode ? 'dark' : 'light');
    updateThemeIcon(isDarkMode);
}

function updateThemeIcon(isDarkMode) {
    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.textContent = isDarkMode ? '☀️' : '🌙';
    }
}