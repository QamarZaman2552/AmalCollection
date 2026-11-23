// Registers the service worker from sw.js. Offline support is a bonus: if the
// browser rejects the registration (private mode, insecure origin) the store
// keeps working exactly as before.
(function () {
    'use strict';
    if (!('serviceWorker' in navigator)) return;

    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/sw.js').catch(function () {
            // Ignore: installability and the offline page are optional.
        });
    });
})();
