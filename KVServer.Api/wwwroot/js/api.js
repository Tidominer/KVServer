// API Client for KVServer
class APIClient {
    constructor() {
        this.baseURL = '/api';
        this.token = localStorage.getItem('access_token');
    }

    setToken(token) {
        this.token = token;
        localStorage.setItem('access_token', token);
    }

    clearToken() {
        this.token = null;
        localStorage.removeItem('access_token');
    }

    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (this.token) {
            headers['X-Access-Token'] = this.token;
        }

        const response = await fetch(url, { ...options, headers });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Request failed');
        }

        return response.json();
    }

    // Storage operations
    async getKeys() {
        return this.request('/keys');
    }

    async createKey(key, value) {
        return this.request('/keys', {
            method: 'POST',
            body: JSON.stringify({ key, value })
        });
    }

    async updateKey(key, value) {
        return this.request(`/keys/${key}`, {
            method: 'PUT',
            body: JSON.stringify({ value })
        });
    }

    async deleteKey(key) {
        return this.request(`/keys/${key}`, {
            method: 'DELETE'
        });
    }

    async getKeyValue(key) {
        return this.request(`/keys/${key}`);
    }

    async getKeyHistory(key) {
        return this.request(`/keys/${key}/history`);
    }

    async getKeyVersion(key, version) {
        return this.request(`/keys/${key}/versions/${version}`);
    }
}

// Create global API client instance
const api = new APIClient();