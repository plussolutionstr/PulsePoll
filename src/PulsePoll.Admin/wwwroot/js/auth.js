window.adminAuth = {
    login: async (data) => {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
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
