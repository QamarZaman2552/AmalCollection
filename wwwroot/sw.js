// Minimal service worker: only exists so the app is installable and an offline
// navigation shows a branded page instead of the browser's dinosaur. It caches
// nothing else, so the site can never serve stale products or prices.
'use strict';

var CACHE = 'amal-offline-v1';
var OFFLINE_URL = '/offline.html';

self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE)
            .then(function (cache) { return cache.add(new Request(OFFLINE_URL, { cache: 'reload' })); })
            .then(function () { return self.skipWaiting(); })
    );
});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys()
            .then(function (keys) {
                return Promise.all(keys.filter(function (key) { return key !== CACHE; })
                    .map(function (key) { return caches.delete(key); }));
            })
            .then(function () { return self.clients.claim(); })
    );
});

self.addEventListener('fetch', function (event) {
    // Only page navigations, and only GET: never swallow a form POST.
    if (event.request.mode !== 'navigate' || event.request.method !== 'GET') return;

    event.respondWith(
        fetch(event.request).catch(function () {
            return caches.match(OFFLINE_URL);
        })
    );
});
