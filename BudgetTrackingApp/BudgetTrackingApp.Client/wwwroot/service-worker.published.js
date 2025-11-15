// WARNING: HEY, LISTEN!
//
// This file is a basic service worker for a Blazor WebAssembly app.
// It's responsible for caching assets and enabling offline functionality.
//
// This file is NOT intended to be a robust, production-ready service worker.
// It lacks features like:
//  - "Cache-busting" for new deployments.
//  - Background sync.
//  - Push notifications.
//
// For a more robust PWA experience, you should enhance this file significantly
// or use a library like Workbox.
//
// -----------------------------------------------------------------------------

const CACHE_NAME = 'budget-tracking-app-cache-v1';
// This list includes runtime-generated Blazor files.
// It will also include files in wwwroot (like CSS, JS, images).
const ASSETS_TO_CACHE = [
    './',
    './_framework/blazor.webassembly.js',
    // Add paths to other essential assets in wwwroot here
    // e.g., './css/app.css', './favicon.ico'
    // The Blazor build process might automatically inject assets here.
    // Check your build output. For this project, let's add:
    './appsettings.json',
    './appsettings.Development.json'
];

self.addEventListener('install', event => {
    console.log('[Service Worker] Install');

    // Pre-cache essential assets
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            console.log('[Service Worker] Caching app shell');
            // Note: `ASSETS_TO_CACHE` is often populated by the build process.
            // For manual setup, you must list all critical files.
            // This is a minimal example.
            return cache.addAll(ASSETS_TO_CACHE.map(url => new Request(url, { cache: 'no-cache' })));
        })
    );
});

self.addEventListener('activate', event => {
    console.log('[Service Worker] Activate');

    // Clean up old caches
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('[Service Worker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );

    return self.clients.claim();
});

self.addEventListener('fetch', event => {
    // We only want to cache GET requests.
    if (event.request.method !== 'GET') {
        return;
    }

    // Network-first strategy for navigation requests
    // For API calls (like /api/...), always go to network.
    if (event.request.url.includes('/api/')) {
        event.respondWith(fetch(event.request));
        return;
    }

    // Cache-first (or network-first) strategy for other assets
    event.respondWith(
        caches.open(CACHE_NAME).then(cache => {
            return cache.match(event.request)
                .then(response => {
                    // If found in cache, return it.
                    if (response) {
                        // console.log('[Service Worker] Fetched from cache:', event.request.url);
                        return response;
                    }

                    // Not in cache, fetch from network
                    return fetch(event.request).then(networkResponse => {
                        // console.log('[Service Worker] Fetched from network:', event.request.url);

                        // Check if we received a valid response
                        if (networkResponse && networkResponse.status === 200) {
                            // Clone the response because it's a stream and can only be consumed once.
                            // We need one for the cache and one for the browser.
                            let responseToCache = networkResponse.clone();
                            cache.put(event.request, responseToCache);
                        }

                        return networkResponse;
                    });
                }).catch(error => {
                    console.error('[Service Worker] Fetch error:', error);
                    // You could return a fallback offline page here if you have one.
                    // return caches.match('offline.html');
                });
        })
    );
});