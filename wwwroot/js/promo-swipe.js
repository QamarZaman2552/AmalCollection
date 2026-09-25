(function () {
    const deck = document.getElementById('promoDeck');
    const dots = document.querySelectorAll('.promo-dot');
    if (!deck) return;

    let cards        = Array.from(deck.querySelectorAll('.promo-card'));
    const total      = cards.length;
    if (!total) return;

    let currentIndex = 0;
    let isDragging   = false;
    let startX = 0, startY = 0, currentX = 0;
    let movedDist = 0;
    let suppressNextClick = false;
    const TAP_MAX   = 8;
    const THRESHOLD  = 80;
    const FLY_MS     = 480;

    function getTopCard() {
        const top = deck.querySelector('.promo-card:last-child');
        return top && !top.classList.contains('is-flying') ? top : null;
    }

    function updateDots(idx) {
        dots.forEach((d, i) => d.classList.toggle('active', i === idx));
    }

    function refreshStack() {
        cards = Array.from(deck.querySelectorAll('.promo-card'));
        cards.forEach((c, i) => {
            if (c.classList.contains('is-flying')) return;
            const isTop = i === cards.length - 1;
            c.classList.toggle('is-back', !isTop);
            c.style.transition = 'transform 0.35s ease, opacity 0.35s ease';
            if (isTop) {
                c.style.transform     = 'none';
                c.style.opacity       = '1';
                c.style.zIndex        = '100';
                c.style.pointerEvents = 'auto';
            } else {
                const depth = cards.length - 1 - i;
                const rot = depth % 2 === 1 ? -8 : 8;
                c.style.transform     = 'scale(' + (1 + depth * 0.055) + ') rotate(' + rot + 'deg)';
                c.style.opacity       = depth > 2 ? '0' : '1';
                c.style.zIndex        = String(50 - depth);
                c.style.pointerEvents = 'none';
            }
        });
    }

    function flyCard(card, dir) {
        if (!card || card.classList.contains('is-flying')) return;

        const cls = dir > 0 ? 'promo-card--fly-right' : 'promo-card--fly-left';
        card.classList.add(cls, 'is-flying');
        card.classList.remove('is-back');
        deck.insertBefore(card, deck.querySelector('.promo-card'));
        refreshStack();

        let done = false;
        function complete(e) {
            if (done) return;
            if (e && e.target !== card) return;
            done = true;
            card.removeEventListener('transitionend', complete);
            card.classList.remove(cls, 'is-flying');
            card.style.transform = '';
            card.style.opacity = '';
            card.style.transition = '';
            currentIndex = (currentIndex + 1) % total;
            updateDots(currentIndex);
            refreshStack();
        }

        card.addEventListener('transitionend', complete);
        setTimeout(complete, FLY_MS);
    }

    function onStart(e) {
        if (e.target && e.target.closest && e.target.closest('a, button, input, form')) {
            if (e.target.closest('.promo-card') === null) return;
        }
        isDragging = true;
        startX  = e.touches ? e.touches[0].clientX : e.clientX;
        startY  = e.touches ? e.touches[0].clientY : e.clientY;
        currentX = 0;
        movedDist = 0;
        suppressNextClick = false;
        const top = getTopCard();
        if (top) top.style.transition = 'none';
    }

    function onMove(e) {
        if (!isDragging) return;
        const x  = e.touches ? e.touches[0].clientX : e.clientX;
        const y  = e.touches ? e.touches[0].clientY : e.clientY;
        currentX = x - startX;
        const dy = y - startY;
        const dist = Math.max(Math.abs(currentX), Math.abs(dy));
        if (dist > movedDist) movedDist = dist;

        if (Math.abs(dy) > Math.abs(currentX) * 1.5) {
            isDragging = false;
            if (movedDist > TAP_MAX) suppressNextClick = true;
            const top = getTopCard();
            if (top) { top.style.transition = ''; top.style.transform = ''; }
            return;
        }

        if (movedDist > TAP_MAX && e.cancelable) e.preventDefault();

        const top = getTopCard();
        if (top) top.style.transform = 'translateX(' + currentX + 'px) rotate(' + (currentX * 0.08) + 'deg)';
    }

    function onEnd() {
        if (!isDragging) return;
        isDragging = false;
        if (movedDist > TAP_MAX) suppressNextClick = true;
        const top = getTopCard();
        if (!top) { currentX = 0; return; }

        if (Math.abs(currentX) >= THRESHOLD) {
            flyCard(top, currentX);
        } else {
            top.style.transition = 'transform 0.35s ease';
            top.style.transform  = 'none';
        }
        currentX = 0;
    }

    // Tap = navigate (agar href ho); drag/swipe ke baad click ko block karo
    deck.addEventListener('click', function (e) {
        const suppress = suppressNextClick;
        suppressNextClick = false;
        if (suppress) {
            e.preventDefault();
            e.stopPropagation();
        }
    }, true);

    // Desktop: <a>/<img> ka native HTML5 drag ghost cursor se chipak jata hai aur
    // mousemove/mouseup stream tod deta hai — swipe ko rok kar rakhta hai.
    // Touch pe ye gesture nahi banta, isliye mobile pe pehle se sahi chalta tha.
    deck.addEventListener('dragstart', function (e) { e.preventDefault(); });

    deck.addEventListener('mousedown',     onStart);
    if (window.__promoMove) document.removeEventListener('mousemove', window.__promoMove);
    if (window.__promoEnd) document.removeEventListener('mouseup', window.__promoEnd);
    window.__promoMove = onMove;
    window.__promoEnd = onEnd;
    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup',   onEnd);

    deck.addEventListener('touchstart', onStart, { passive: true });
    deck.addEventListener('touchmove',  onMove,  { passive: false });
    deck.addEventListener('touchend',   onEnd);

    refreshStack();
    updateDots(0);
})();
