// ─── AJAX Navigation Engine ─────────────────────────────────────────────
// Har form/link navigation ko bina page reload kiye server se HTML le kar
// content root swap karta hai. Server logic bilkul wahi rehta hai (same
// endpoints, same redirects, same antiforgery) — sirf full reload ki jagah
// partial refresh hota hai. Koi bhi fail hone par native submit/location
// par fallback chalta hai, is liye UI/logic kabhi destroy nahi hota.
(function () {
    'use strict';
    if (window.__ajaxNav) return;
    window.__ajaxNav = true;

    var ROOT_SELECTORS = ['main.main-content', '.admin-content'];
    var chain = Promise.resolve();

    function rootInfo(doc) {
        for (var i = 0; i < ROOT_SELECTORS.length; i++) {
            var el = doc.querySelector(ROOT_SELECTORS[i]);
            if (el) return { el: el, sel: ROOT_SELECTORS[i] };
        }
        return null;
    }

    function runScripts(scope) {
        if (!scope) return;
        var list = scope.querySelectorAll('script');
        for (var i = 0; i < list.length; i++) {
            var old = list[i];
            var s = document.createElement('script');
            for (var j = 0; j < old.attributes.length; j++) {
                s.setAttribute(old.attributes[j].name, old.attributes[j].value);
            }
            s.textContent = old.textContent;
            old.parentNode.replaceChild(s, old);
        }
    }

    function syncAdminSidebar(doc) {
        var fresh = doc.querySelector('.admin-sidebar');
        var cur = document.querySelector('.admin-sidebar');
        if (!fresh || !cur) return;
        var fa = fresh.querySelectorAll('[data-active]');
        var ca = cur.querySelectorAll('[data-active]');
        if (!fa.length || fa.length !== ca.length) return;
        for (var i = 0; i < fa.length; i++) ca[i].className = fa[i].className;
    }

    function afterSwap(doc, finalUrl, opts) {
        document.title = doc.title || document.title;
        var t1 = document.querySelector('h1.admin-page-title');
        var t2 = doc.querySelector('h1.admin-page-title');
        if (t1 && t2) t1.textContent = t2.textContent;
        syncAdminSidebar(doc);

        document.body.style.overflowY = '';

        try { if (typeof updateCartBadge === 'function') updateCartBadge(); } catch (e) {}
        try { if (typeof updateWishlistBadge === 'function') updateWishlistBadge(); } catch (e) {}

        var inits = window.__pageInits || [];
        for (var i = 0; i < inits.length; i++) {
            try { inits[i](); } catch (e) {}
        }

        var u;
        try { u = new URL(finalUrl, location.href); } catch (e) { return; }
        var target = u.pathname + u.search + u.hash;
        if (opts.push) history.pushState({ an: 1 }, '', target);
        else history.replaceState({ an: 1 }, '', target);

        if (u.hash) {
            var anchor = document.getElementById(decodeURIComponent(u.hash.substring(1)));
            if (anchor) { anchor.scrollIntoView(); return; }
        }
        if (u.pathname !== opts.prevPath) window.scrollTo(0, 0);
    }

    function swap(doc, finalUrl, opts) {
        var freshRoot = rootInfo(doc);
        var curRoot = rootInfo(document);
        if (!freshRoot || !curRoot || freshRoot.sel !== curRoot.sel) return false;

        var freshScripts = doc.getElementById('pageScripts');
        var curScripts = document.getElementById('pageScripts');
        if (!!freshScripts !== !!curScripts) return false;

        curRoot.el.parentNode.replaceChild(freshRoot.el, curRoot.el);
        if (freshScripts) curScripts.parentNode.replaceChild(freshScripts, curScripts);

        runScripts(freshRoot.el);
        runScripts(document.getElementById('pageScripts'));

        afterSwap(doc, finalUrl, { push: opts.push, prevPath: opts.prevPath });
        return true;
    }

    function doFetch(method, url, body) {
        var opts = {
            method: method,
            redirect: 'follow',
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        };
        if (body) opts.body = body;
        return fetch(url, opts).then(function (res) {
            var fu = new URL(res.url || url, location.href);
            if (fu.origin !== location.origin) {
                location.href = fu.href;
                return null;
            }
            if (!res.ok) throw new Error('HTTP ' + res.status);
            var ct = res.headers.get('content-type') || '';
            if (ct.indexOf('html') === -1) throw new Error('Non-HTML response');
            return res.text().then(function (html) {
                return { doc: new DOMParser().parseFromString(html, 'text/html'), url: fu.href };
            });
        });
    }

    function navigate(method, url, body, push, fallback) {
        var prevPath = location.pathname;
        chain = chain.then(function () {
            return doFetch(method, url, body).then(function (r) {
                if (!r) return;
                if (!swap(r.doc, r.url, { push: push, prevPath: prevPath })) fallback();
            }).catch(function () {
                try { fallback(); } catch (e) { location.href = url; }
            });
        });
        return chain;
    }

    // ─── Forms (GET + POST) ────────────────────────────────────────────
    document.addEventListener('submit', function (e) {
        if (e.defaultPrevented) return;
        var form = e.target;
        if (!form || form.tagName !== 'FORM') return;
        if (form.hasAttribute('data-noajax')) return;
        if (form.target && form.target !== '_self') return;
        var m = (form.method || 'get').toLowerCase();
        if (m !== 'get' && m !== 'post') return;

        var submitter = e.submitter || null;
        // btn.formAction page URL deta hai jab formaction attr na ho — sirf attr use karo
        var rawAction = submitter ? submitter.getAttribute('formaction') : null;
        var actionUrl;
        try {
            actionUrl = new URL(rawAction || form.action, location.href);
        } catch (err) { return; }
        if (actionUrl.origin !== location.origin) return;

        e.preventDefault();

        var url = actionUrl.href;
        var body = null;
        if (m === 'post') {
            body = new FormData(form);
        } else {
            var qs = new URLSearchParams(new FormData(form)).toString();
            url = actionUrl.origin + actionUrl.pathname + (qs ? '?' + qs : '') + actionUrl.hash;
        }

        navigate(m.toUpperCase(), url, body, m === 'get', function () {
            try { form.submit(); } catch (err) { location.href = actionUrl.href; }
        });
    });

    // ─── Quick Add (sirf badge update + toast; page pe hi rehta hai) ──
    function quickToast(msg, ok) {
        var old = document.getElementById('quickToast');
        if (old && old.parentNode) old.parentNode.removeChild(old);
        var d = document.createElement('div');
        d.className = 'toast ' + (ok ? 'toast-success' : 'toast-error');
        d.id = 'quickToast';
        var ic = document.createElement('i');
        ic.className = ok ? 'bi bi-check-circle-fill' : 'bi bi-x-circle';
        d.appendChild(ic);
        d.appendChild(document.createTextNode(' ' + msg));
        document.body.appendChild(d);
        setTimeout(function () { if (d.parentNode) d.parentNode.removeChild(d); }, 4000);
    }

    function quickAdd(a, u) {
        if (a.getAttribute('data-busy') === '1') return;
        a.setAttribute('data-busy', '1');
        function done() { a.removeAttribute('data-busy'); }

        function doAdd() {
            fetch(u.href, {
                method: 'GET',
                redirect: 'follow',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            }).then(function (res) {
                var fu = new URL(res.url || u.href, location.href);
                if (fu.origin !== location.origin) { location.href = fu.href; return null; }
                if (!res.ok) throw new Error('HTTP ' + res.status);
                // Success = server /Cart pe redirect kare; warna error redirect (stock/not-found)
                var ok = fu.pathname === '/Cart' || fu.pathname === '/Cart/';
                if (ok) return { ok: true };
                return res.text().then(function (html) {
                    var doc = new DOMParser().parseFromString(html, 'text/html');
                    var t = doc.querySelector('.toast');
                    return { ok: false, msg: t ? t.textContent.replace(/\s+/g, ' ').trim() : 'Could not add to cart' };
                });
            }).then(function (r) {
                done();
                if (!r) return;
                if (r.ok) {
                    try { if (typeof updateCartBadge === 'function') updateCartBadge(); } catch (e) {}
                    var icon = a.querySelector('i');
                    var oldCls = icon ? icon.className : '';
                    if (icon) icon.className = 'bi bi-check-lg';
                    if (icon) setTimeout(function () { icon.className = oldCls; }, 1500);
                    quickToast('Added to cart', true);
                } else {
                    quickToast(r.msg || 'Could not add to cart', false);
                }
            }).catch(function () {
                done();
                quickToast('Could not add to cart', false);
            });
        }

        // Already-in-cart guard: pehle check, agar product cart me hai to add mat karo
        var pm = /productId=(\d+)/.exec(u.href);
        if (!pm) { doAdd(); return; }
        fetch('/Cart/Contains?productId=' + pm[1], {
            method: 'GET',
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return res.json();
        }).then(function (data) {
            if (data && data.inCart) {
                done();
                quickToast('Already in cart', true);
                return;
            }
            doAdd();
        }).catch(function () {
            // Contains fail (jaise purana server) = fail-open, add chalta rahe
            doAdd();
        });
    }

    // ─── Links (sirf content root ke andar; layout nav/footer reload) ──
    document.addEventListener('click', function (e) {
        if (e.defaultPrevented) return;
        if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        var a = e.target && e.target.closest ? e.target.closest('a[href]') : null;
        if (!a || a.hasAttribute('data-noajax') || a.hasAttribute('download')) return;
        if (a.target && a.target !== '_self') return;
        var hrefAttr = a.getAttribute('href') || '';
        if (!hrefAttr || hrefAttr.charAt(0) === '#') return;

        var u;
        try { u = new URL(a.href, location.href); } catch (err) { return; }
        if (u.origin !== location.origin) return;

        // Quick Add: navigate mat karo — AJAX se add karke badge/toast dikhao
        if (a.classList.contains('quick-add')) {
            e.preventDefault();
            quickAdd(a, u);
            return;
        }

        if (u.pathname === location.pathname && u.search === location.search) {
            if (u.hash) {
                var anchor = document.getElementById(decodeURIComponent(u.hash.substring(1)));
                if (anchor) {
                    e.preventDefault();
                    history.replaceState({ an: 1 }, '', u.pathname + u.search + u.hash);
                    anchor.scrollIntoView();
                }
            }
            return;
        }

        if (!a.closest(ROOT_SELECTORS.join(', '))) return;

        e.preventDefault();
        navigate('GET', u.href, null, true, function () { location.href = u.href; });
    });

    // ─── Back/Forward ──────────────────────────────────────────────────
    window.addEventListener('popstate', function () {
        navigate('GET', location.href, null, false, function () { location.reload(); });
    });
})();
