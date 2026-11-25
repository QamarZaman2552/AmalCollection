// Home reviews carousel: scroll-snap track with prev/next controls.
(function () {
  'use strict';

  var track = document.querySelector('[data-testi-track]');
  if (!track) return;

  var nav = document.querySelector('[data-testi-nav]');
  var prev = document.querySelector('[data-testi-prev]');
  var next = document.querySelector('[data-testi-next]');
  if (!nav || !prev || !next) return;

  var reduceMotion = window.matchMedia
    && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  function cardStep() {
    var card = track.querySelector('.testi-card');
    if (!card) return track.clientWidth;
    var styles = getComputedStyle(track);
    var gap = parseFloat(styles.columnGap || styles.gap) || 0;
    return card.getBoundingClientRect().width + gap;
  }

  function update() {
    var max = track.scrollWidth - track.clientWidth;
    nav.hidden = max < 8;
    prev.disabled = track.scrollLeft <= 4;
    next.disabled = track.scrollLeft >= max - 4;
  }

  function move(direction) {
    track.scrollBy({
      left: direction * cardStep(),
      behavior: reduceMotion ? 'auto' : 'smooth'
    });
  }

  prev.addEventListener('click', function () { move(-1); });
  next.addEventListener('click', function () { move(1); });
  track.addEventListener('scroll', update, { passive: true });

  // Re-runs after an AJAX page swap; drop the old window listener first.
  if (window.__testiResize) window.removeEventListener('resize', window.__testiResize);
  window.__testiResize = update;
  window.addEventListener('resize', window.__testiResize);

  update();
  window.addEventListener('load', update, { once: true });
})();
