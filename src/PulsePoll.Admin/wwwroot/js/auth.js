window.adminAuth = {
    login: async (data) => {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const headers = { 'Content-Type': 'application/json' };
            if (token) {
                headers['RequestVerificationToken'] = token;
            }
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(data)
            });
            if (!response.ok) {
                return { success: false, errorMessage: 'Sunucu hatası (' + response.status + ').' };
            }
            return await response.json();
        } catch (e) {
            return { success: false, errorMessage: 'Sunucu bağlantı hatası.' };
        }
    }
};
