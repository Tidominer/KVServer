// Authentication and routing management
class AuthManager {
    constructor() {
        this.currentPage = 'login';
        this.init();
    }

    init() {
        // Check if user is already logged in
        const token = localStorage.getItem('access_token');
        if (token) {
            api.setToken(token);
            this.navigateTo('dashboard');
        }
    }

    async login(token, errorEl) {
        try {
            api.setToken(token);
            await api.getKeys();
            this.navigateTo('dashboard');
            return true;
        } catch (error) {
            console.error('Login error:', error);
            api.clearToken();
            if (errorEl) {
                errorEl.textContent = error.status === 429
                    ? 'Too many failed attempts. Please wait before trying again.'
                    : 'Invalid access token. Please check and try again.';
                errorEl.style.display = 'block';
            }
            return false;
        }
    }

    logout() {
        api.clearToken();
        this.navigateTo('login');
    }

    navigateTo(page) {
        // Hide all pages
        document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));

        // Show target page
        const targetPage = document.getElementById(`${page}-page`);
        if (targetPage) {
            targetPage.classList.add('active');
            this.currentPage = page;
        }
    }

    getCurrentPage() {
        return this.currentPage;
    }
}

// Create global auth manager instance
const auth = new AuthManager();