globalThis.blazorHelpers = {
    focusAndSelect: (element) => {
        if (element && typeof element.focus === 'function') {
            element.focus();
            if (typeof element.select === 'function') element.select();
        }
    }
};

globalThis.oauthPopup = {
    open: function (url, verifier) {
        localStorage.setItem('pkce_verifier', verifier);
        const w = 500, h = 700;
        const left = Math.round(screen.width / 2 - w / 2);
        const top  = Math.round(screen.height / 2 - h / 2);
        const popup = window.open(url, 'oauth_popup',
            `width=${w},height=${h},left=${left},top=${top},popup=yes`);

        if (!popup || popup.closed) {
            localStorage.removeItem('pkce_verifier');
            return Promise.reject('popup_blocked');
        }

        return new Promise((resolve, reject) => {
            const channel = new BroadcastChannel('oauth_result');
            const timer = setInterval(() => {
                if (popup.closed) {
                    clearInterval(timer);
                    channel.close();
                    reject('popup_closed');
                }
            }, 500);
            channel.onmessage = (e) => {
                clearInterval(timer);
                channel.close();
                if (e.data === 'success') resolve();
                else reject(e.data || 'oauth_error');
            };
        });
    },

    notify: function (result) {
        const channel = new BroadcastChannel('oauth_result');
        channel.postMessage(result);
        setTimeout(() => { channel.close(); window.close(); }, 100);
    },

    isPopup: function () {
        return window.opener != null;
    }
};
