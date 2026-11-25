// Mobile bottom nav: active section, mirrored cart/wishlist counts, search focus.
(function () {
  'use strict';

  var nav = document.querySelector('.mobile-bottom-nav');
  if (!nav) return;

  var items = Array.prototype.slice.call(nav.querySelectorAll('.mbn-item'));
  var reduceMotion = window.matchMedia
    && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  function currentKey() {
    var path = location.pathname.replace(/\/+$/, '') || '/';
    if (path === '/') {
      var params = new URLSearchParams(location.search);
      var filtered = ['category', 'season', 'fabric', 'size', 'sort',
        'minPrice', 'maxPrice', 'inStock', 'onSale', 'page']
        .some(function (key) { return params.has(key); });
      return (filtered || location.hash === '#products') ? 'shop' : 'home';
    }
    if (path === '/Products/Search') return '';
    if (path.indexOf('/Products') === 0) return 'shop';
    if (path.indexOf('/Wishlist') === 0) return 'wishlist';
    if (path.indexOf('/Cart') === 0) return 'cart';
    return '';
  }

  function setActive(key) {
    items.forEach(function (item) {
      var on = item.getAttribute('data-mbn') === key;
      item.classList.toggle('is-active', on);
      if (on) item.setAttribute('aria-current', 'page');
      else item.removeAttribute('aria-current');
    });
  }

  function refreshActive() { setActive(currentKey()); }
  function scheduleRefresh() { setTimeout(refreshActive, 0); }

  // Re-run after every AJAX page swap (pageInit calls immediately as well).
  if (typeof window.pageInit === 'function') pageInit(scheduleRefresh);
  else scheduleRefresh();
  window.addEventListener('popstate', scheduleRefresh);

  // Mirror the header badges so both stay in sync without a second request.
  function mirror(sourceId, badge, label, unit) {
    var source = document.getElementById(sourceId);
    if (!source || !badge) return;
    var link = badge.closest('.mbn-item');

    function apply() {
      var count = parseInt((source.textContent || '').trim(), 10);
      if (isNaN(count)) count = 0;
      var visible = count > 0 && source.style.display !== 'none';
      badge.textContent = count > 99 ? '99+' : String(count);
      badge.hidden = !visible;
      if (link) link.setAttribute('aria-label', label + (visible ? ', ' + count + ' ' + unit : ''));
    }

    apply();
    new MutationObserver(apply).observe(source, {
      childList: true,
      characterData: true,
      subtree: true,
      attributes: true,
      attributeFilter: ['style']
    });
  }

  mirror('cartBadge', nav.querySelector('[data-mbn-cart]'), 'Cart', 'items');
  mirror('wishlistBadge', nav.querySelector('[data-mbn-wishlist]'), 'Wishlist', 'saved');

  // Tap targets update the highlight immediately; AJAX swaps refresh it again.
  items.forEach(function (item) {
    item.addEventListener('click', function () {
      var key = item.getAttribute('data-mbn');
      if (key) setActive(key);
    });
  });

  // Home on the home page: clear filters/hash and return to the top.
  var home = nav.querySelector('[data-mbn="home"]');
  if (home) {
    home.addEventListener('click', function (event) {
      if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
      var path = location.pathname.replace(/\/+$/, '') || '/';
      if (path !== '/') return;
      event.preventDefault();
      event.stopPropagation();
      history.replaceState({ an: 1 }, '', '/');
      setActive('home');
      window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
    });
  }

  // Search: focus the always-visible mobile search field.
  var search = nav.querySelector('[data-mbn-search]');
  if (search) {
    search.addEventListener('click', function () {
      var input = document.getElementById('mobileNavSearch');
      if (!input) return;
      try { input.focus({ preventScroll: true }); } catch (e) { input.focus(); }
      if (window.scrollY > 8) {
        window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
      }
    });
  }
})();
