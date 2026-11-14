// â”€â”€â”€ Mobile Hamburger Menu â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
(function () {
  const btn  = document.getElementById('hamburgerBtn');
  const menu = document.getElementById('mobileMenu');
  const overlay = document.getElementById('mobileOverlay');
  if (!btn || !menu) return;

  function openMenu()  {
    btn.classList.add('open'); menu.classList.add('open');
    btn.setAttribute('aria-expanded', 'true');
    if (overlay) overlay.classList.add('open');
  }
  function closeMenu() {
    btn.classList.remove('open'); menu.classList.remove('open');
    btn.setAttribute('aria-expanded', 'false');
    if (overlay) overlay.classList.remove('open');
  }
  function toggleMenu() { btn.classList.contains('open') ? closeMenu() : openMenu(); }

  btn.addEventListener('click', (e) => { e.stopPropagation(); toggleMenu(); });
  if (overlay) overlay.addEventListener('click', closeMenu);

  // Close when any menu link is clicked
  menu.querySelectorAll('a').forEach(a => a.addEventListener('click', closeMenu));

  // Close on resize back to desktop
  window.addEventListener('resize', () => { if (window.innerWidth > 768) closeMenu(); });
})();

// --- Page Init Registry (ajax-nav content swap ke baad dobara chalta hai) ---
window.__pageInits = window.__pageInits || [];
function pageInit(fn) { window.__pageInits.push(fn); fn(); }

// â”€â”€â”€ Cart Badge â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
async function updateCartBadge() {
  try {
    const res = await fetch('/Cart/Count');
    const count = await res.json();
    const badge = document.getElementById('cartBadge');
    if (badge) badge.textContent = count > 0 ? count : '0';
  } catch {}
}
updateCartBadge();

// â”€â”€â”€ Wishlist Badge â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
async function updateWishlistBadge() {
  try {
    const res = await fetch('/Wishlist/Count');
    const count = await res.json();
    const badge = document.getElementById('wishlistBadge');
    if (badge) {
      badge.textContent = count > 0 ? count : '0';
      badge.style.display = count > 0 ? 'inline-flex' : 'none';
    }
  } catch {}
}
updateWishlistBadge();


// â”€â”€â”€ Auto-dismiss Toast â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initToast() {
  const toast = document.getElementById('toast') || document.getElementById('toast-success') || document.getElementById('toast-error');
  if (toast) setTimeout(() => toast.remove(), 4000);
}
pageInit(initToast);

// â”€â”€â”€ Chatbot Widget â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const bubble    = document.getElementById('chatBubble');
const chatWin   = document.getElementById('chatWindow');
const chatClose = document.getElementById('chatClose');
const chatInput = document.getElementById('chatInput');
const chatSend  = document.getElementById('chatSend');
const chatMsgs  = document.getElementById('chatMessages');

const CHAT_STARTER_CHIPS = ['Sizing help', 'Delivery time', 'Track my order', 'Cash on delivery'];

if (bubble && chatWin) {
  let starterShown = false;

  function openChat() {
    chatWin.classList.add('open');
    chatWin.setAttribute('aria-hidden', 'false');
    bubble.style.display = 'none';
    chatInput.focus();
    if (!starterShown) {
      starterShown = true;
      showChips(CHAT_STARTER_CHIPS);
    }
  }

  function closeChat() {
    const hadFocus = chatWin.contains(document.activeElement);
    chatWin.classList.remove('open');
    chatWin.setAttribute('aria-hidden', 'true');
    bubble.style.display = 'flex';
    if (hadFocus) bubble.focus();
  }

  bubble.addEventListener('click', () => {
    chatWin.classList.contains('open') ? closeChat() : openChat();
  });
  bubble.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openChat(); }
  });

  chatClose.addEventListener('click', closeChat);

  document.addEventListener('click', (e) => {
    if (chatWin.classList.contains('open') && !chatWin.contains(e.target) && !bubble.contains(e.target)) {
      closeChat();
    }
  });

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && chatWin.classList.contains('open')) {
      closeChat();
      bubble.focus();
    }
  });

  chatSend.addEventListener('click', sendMessage);
  chatInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
  });
}

function addMsg(text, type) {
  const msg = document.createElement('div');
  msg.className = `chat-msg ${type}`;
  if (type === 'bot' && text.startsWith('\u26A0')) msg.classList.add('chat-error');
  const bubble_el = document.createElement('div');
  bubble_el.className = 'chat-bubble';
  if (type === 'bot') {
    bubble_el.innerHTML = text
      .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
      .replace(/_([^_]+)_/g, '<em>$1</em>')
      .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>');
  } else {
    bubble_el.textContent = text;
  }
  msg.appendChild(bubble_el);
  chatMsgs.appendChild(msg);
  chatMsgs.scrollTop = chatMsgs.scrollHeight;
  return msg;
}

function showChips(chips) {
  chatMsgs.querySelectorAll('.chat-chips').forEach(c => c.remove());
  const wrap = document.createElement('div');
  wrap.className = 'chat-chips';
  chips.forEach(label => {
    const btn = document.createElement('button');
    btn.className = 'chip';
    btn.textContent = label;
    btn.onclick = () => { chatInput.value = label; sendMessage(); };
    wrap.appendChild(btn);
  });
  chatMsgs.appendChild(wrap);
  chatMsgs.scrollTop = chatMsgs.scrollHeight;
}

function showTyping() {
  const msg = document.createElement('div');
  msg.className = 'chat-msg bot typing';
  msg.id = 'typingIndicator';
  msg.innerHTML = '<div class="chat-bubble"><span class="typing-dot"></span><span class="typing-dot"></span><span class="typing-dot"></span></div>';
  chatMsgs.appendChild(msg);
  chatMsgs.scrollTop = chatMsgs.scrollHeight;
}

function hideTyping() {
  const t = document.getElementById('typingIndicator');
  if (t) t.remove();
}

async function sendMessage() {
  const text = chatInput.value.trim();
  if (!text) return;

  chatInput.value = '';
  chatSend.disabled = true;
  addMsg(text, 'user');
  chatMsgs.querySelectorAll('.chat-chips').forEach(c => c.remove());
  showTyping();

  try {
    const res = await fetch('/Chat/SendMessage', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message: text })
    });
    const data = await res.json();
    hideTyping();
    addMsg(data.reply, 'bot');
    const lc = data.reply.toLowerCase();
    if (lc.includes('product') || lc.includes('item')) showChips(['budget', 'expensive', 'show all']);
    else if (lc.includes('recommend') || lc.includes('suggest')) showChips(['budget', 'show all']);
    else showChips(CHAT_STARTER_CHIPS);
  } catch {
    hideTyping();
    addMsg('âš ï¸ Connection error. Please try again.', 'bot');
  } finally {
    chatSend.disabled = false;
    chatInput.focus();
  }
}

// â”€â”€â”€ Hero Slider handled by Prototype Hero Slider (#heroSlider) below â”€â”€â”€

// â”€â”€â”€ rn-Slider (Homepage Hero Banner) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initRnSlider() {
  if (window.__rnSliderStop) { window.__rnSliderStop(); window.__rnSliderStop = null; }
  const slider = document.getElementById('rnSlider');
  if (!slider) return;

  const slides  = slider.querySelectorAll('.rn-slide');
  const dots    = slider.querySelectorAll('.rn-dot');
  const prevBtn = document.getElementById('rnPrev');
  const nextBtn = document.getElementById('rnNext');
  const DELAY   = 5000;
  let   current = 0;
  let   timer   = null;

  function goTo(idx) {
    const total = slides.length;
    const next  = ((idx % total) + total) % total;
    slides[current].classList.remove('active');
    dots[current] && dots[current].classList.remove('active');
    current = next;
    slides[current].classList.add('active');
    dots[current] && dots[current].classList.add('active');
  }

  function start() { timer = setInterval(() => goTo(current + 1), DELAY); }
  function stop()  { clearInterval(timer); }
  function reset() { stop(); start(); }

  if (prevBtn) prevBtn.addEventListener('click', () => { goTo(current - 1); reset(); });
  if (nextBtn) nextBtn.addEventListener('click', () => { goTo(current + 1); reset(); });

  dots.forEach(d => d.addEventListener('click', () => {
    goTo(parseInt(d.dataset.goto, 10)); reset();
  }));

  // Pause on hover
  slider.addEventListener('mouseenter', stop);
  slider.addEventListener('mouseleave', () => { stop(); start(); });

  // Touch / swipe
  let tx = 0;
  slider.addEventListener('touchstart', e => { tx = e.changedTouches[0].clientX; }, { passive: true });
  slider.addEventListener('touchend',   e => {
    const diff = tx - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 45) { diff > 0 ? goTo(current + 1) : goTo(current - 1); reset(); }
  }, { passive: true });

  // Keyboard
  slider.setAttribute('tabindex', '0');
  slider.addEventListener('keydown', e => {
    if (e.key === 'ArrowLeft')  { goTo(current - 1); reset(); }
    if (e.key === 'ArrowRight') { goTo(current + 1); reset(); }
  });

  start();
  window.__rnSliderStop = stop;
}
pageInit(initRnSlider);// â”€â”€â”€ Scroll Reveal â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initReveal() {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(el => {
      if (el.isIntersecting) {
        el.target.classList.add('visible');
        observer.unobserve(el.target);
      }
    });
  }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

  // Auto-apply .reveal to common elements
  document.querySelectorAll('.product-card, .section-title, .cart-item, .review-card, .order-card, .analytics-stat-card, .faq-item, .msg-card').forEach((el, i) => {
    el.classList.add('reveal');
    if (i % 4 === 1) el.classList.add('reveal-delay-1');
    if (i % 4 === 2) el.classList.add('reveal-delay-2');
    if (i % 4 === 3) el.classList.add('reveal-delay-3');
    observer.observe(el);
  });
}
pageInit(initReveal);

// â”€â”€â”€ Button Ripple Effect â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
document.addEventListener('click', (e) => {
  const btn = e.target.closest('.btn-primary, .btn-glass');
  if (!btn) return;
  const ripple = document.createElement('span');
  const rect   = btn.getBoundingClientRect();
  const size   = Math.max(rect.width, rect.height) * 2;
  ripple.style.cssText = `
    position:absolute; border-radius:50%;
    width:${size}px; height:${size}px;
    left:${e.clientX - rect.left - size/2}px;
    top:${e.clientY - rect.top - size/2}px;
    background:rgba(255,255,255,0.18);
    transform:scale(0); animation:rippleAnim 0.55s ease forwards;
    pointer-events:none; z-index:10;
  `;
  btn.style.position = 'relative';
  btn.style.overflow = 'hidden';
  btn.appendChild(ripple);
  ripple.addEventListener('animationend', () => ripple.remove());
});

// Inject ripple keyframe once
if (!document.getElementById('rippleStyle')) {
  const s = document.createElement('style');
  s.id = 'rippleStyle';
  s.textContent = '@keyframes rippleAnim { to { transform:scale(1); opacity:0; } }';
  document.head.appendChild(s);
}

// â”€â”€â”€ Navbar scroll shadow (cached nav ref) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
(function () {
  const nav = document.querySelector('.navbar.ac-nav');
  if (!nav) return;
  window.addEventListener('scroll', () => {
    nav.style.boxShadow = window.scrollY > 10 ? '0 4px 24px rgba(0,0,0,0.08)' : '';
  }, { passive: true });
})();

// â”€â”€â”€ Password: show/hide + strength meter + match â”€â”€â”€â”€â”€â”€â”€â”€
(function () {
  function scorePassword(pw) {
    if (!pw) return 0;
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[a-z]/.test(pw) && /[A-Z]/.test(pw)) score++;
    if (/\d/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    if (pw.length >= 12) score++;
    return score; // 0â€“5
  }
  function levelOf(score, len) {
    if (len === 0) return '';
    if (score <= 2) return 'weak';
    if (score === 3 || score === 4) return 'medium';
    return 'strong';
  }

  document.querySelectorAll('.pw-field input').forEach(function (input) {
    const field = input.closest('.pw-field');
    const group = field.parentElement;
    const toggle = field.querySelector('.pw-toggle');
    const strength = group.querySelector('.pw-strength');

    if (toggle) {
      toggle.addEventListener('click', function () {
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        const icon = toggle.querySelector('i');
        if (icon) {
          icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
        }
        toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
      });
    }

    if (!strength) return;
    const bar = strength.querySelector('.pw-strength-bar span');
    const label = strength.querySelector('.pw-strength-label');
    const reqs = group.querySelector('.pw-reqs');

    function update() {
      const pw = input.value;
      const level = levelOf(scorePassword(pw), pw.length);
      bar.className = level;
      label.className = 'pw-strength-label' + (level ? ' ' + level : '');
      label.textContent = level ? level.charAt(0).toUpperCase() + level.slice(1) : 'Password strength';
      if (reqs) {
        const rules = {
          length: pw.length >= 8,
          lower: /[a-z]/.test(pw),
          upper: /[A-Z]/.test(pw),
          digit: /\d/.test(pw),
          symbol: /[^A-Za-z0-9]/.test(pw)
        };
        reqs.querySelectorAll('li').forEach(function (li) {
          li.classList.toggle('valid', !!rules[li.dataset.rule]);
        });
      }
    }
    input.addEventListener('input', update);
    update();
  });

  document.querySelectorAll('input[data-match]').forEach(function (m) {
    const target = document.getElementById(m.dataset.match);
    if (!target) return;
    const hint = m.closest('.form-group').querySelector('.pw-match');
    function sync() {
      if (!m.value) { if (hint) hint.textContent = ''; return; }
      const ok = m.value === target.value;
      if (hint) {
        hint.textContent = ok ? 'âœ“ Passwords match' : 'âœ— Passwords do not match';
        hint.className = 'pw-match ' + (ok ? 'ok' : 'no');
      }
    }
    m.addEventListener('input', sync);
    target.addEventListener('input', sync);
  });
})();

// â”€â”€â”€ Auth Pages: prevent pinch zoom on mobile â”€â”€â”€â”€â”€â”€
(function () {
  var authPage = document.querySelector('.auth-page');
  if (!authPage) return;
  document.addEventListener('gesturestart', function (e) { e.preventDefault(); });
})();

// â”€â”€â”€ Light/Dark Theme Toggle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function toggleTheme() {
  var body = document.body;
  if (body.classList.contains('light-mode')) {
    body.classList.remove('light-mode');
    localStorage.setItem('theme', 'dark');
  } else {
    body.classList.add('light-mode');
    localStorage.setItem('theme', 'light');
  }
}
window.addEventListener('DOMContentLoaded', function () {
  var saved = localStorage.getItem('theme');
  if (saved === 'dark') {
    document.body.classList.remove('light-mode');
  } else {
    document.body.classList.add('light-mode');
  }
  var btn = document.getElementById('themeToggle');
  var mobileBtn = document.getElementById('mobileThemeToggle');
  if (btn) btn.addEventListener('click', toggleTheme);
  if (mobileBtn) mobileBtn.addEventListener('click', toggleTheme);
});

// â”€â”€â”€ Prototype Hero Slider (#heroSlider) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initHeroSlider() {
  if (window.__heroSliderStop) { window.__heroSliderStop(); window.__heroSliderStop = null; }
  const hero = document.getElementById('heroSlider');
  if (!hero) return;
  const slides = hero.querySelectorAll('.hero-slide');
  const dots = hero.querySelectorAll('.hero-dot');
  const num = document.getElementById('heroNum');
  const bar = document.getElementById('heroBar');
  if (!slides.length) return;
  let cur = 0;
  let timer;
  const DURATION = 6000;

  function restartBar() {
    if (!bar) return;
    bar.style.animation = 'none';
    void bar.offsetWidth;
    bar.style.animation = '';
  }
  function go(i) {
    cur = (i + slides.length) % slides.length;
    slides.forEach((s, idx) => s.classList.toggle('active', idx === cur));
    dots.forEach((d, idx) => d.classList.toggle('active', idx === cur));
    if (num) num.textContent = String(cur + 1).padStart(2, '0');
    restartBar();
    resetTimer();
  }
  function next() { go(cur + 1); }
  function prev() { go(cur - 1); }
  function resetTimer() {
    clearInterval(timer);
    timer = setInterval(next, DURATION);
  }
  hero.querySelector('#heroNext')?.addEventListener('click', next);
  hero.querySelector('#heroPrev')?.addEventListener('click', prev);
  dots.forEach(d => d.addEventListener('click', () => go(parseInt(d.dataset.goto))));
  hero.addEventListener('mouseenter', () => { clearInterval(timer); hero.classList.add('paused'); });
  hero.addEventListener('mouseleave', () => { hero.classList.remove('paused'); resetTimer(); });
  let startX = 0;
  hero.addEventListener('touchstart', e => { startX = e.touches[0].clientX; }, { passive: true });
  hero.addEventListener('touchend', e => {
    const diff = startX - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 50) { diff > 0 ? next() : prev(); }
  }, { passive: true });
  resetTimer();
  window.__heroSliderStop = function () { clearInterval(timer); };
}
pageInit(initHeroSlider);

// â”€â”€â”€ Prototype size/season filter chips â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
document.addEventListener('click', function (e) {
  const chip = e.target.closest('.size-chip');
  if (!chip) return;
  const group = chip.closest('.size-filter');
  if (!group) return;
  group.querySelectorAll('.size-chip').forEach(c => c.classList.remove('active'));
  chip.classList.add('active');
});

// â”€â”€â”€ Close Bootstrap collapse nav on link click â”€â”€â”€â”€
document.addEventListener('DOMContentLoaded', function () {
  const mainNav = document.getElementById('mainNav');
  if (!mainNav || typeof bootstrap === 'undefined') return;
  mainNav.querySelectorAll('.nav-link').forEach(link => {
    link.addEventListener('click', () => {
      const el = bootstrap.Collapse.getInstance(mainNav);
      if (el && mainNav.classList.contains('show')) el.hide();
    });
  });
});

// â”€â”€â”€ Checkout pay option active state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
document.addEventListener('click', function (e) {
  const pay = e.target.closest('.co-pay');
  if (!pay || pay.classList.contains('disabled')) return;
  document.querySelectorAll('.co-pay').forEach(x => x.classList.remove('active'));
  pay.classList.add('active');
});

// â”€â”€â”€ Checkout summary: open on desktop â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initOrderSummary() {
  const el = document.getElementById('orderSummary');
  if (!el) return;
  if (window.matchMedia('(min-width: 992px)').matches) {
    el.classList.add('show');
    document.getElementById('sumToggle')?.setAttribute('aria-expanded', 'true');
  }
}
pageInit(initOrderSummary);

// â”€â”€â”€ Product detail size/color chip select â”€â”€â”€â”€â”€â”€â”€â”€â”€
document.addEventListener('click', function (e) {
  const sizeBtn = e.target.closest('.size-opt:not(.disabled)');
  if (sizeBtn) {
    sizeBtn.parentElement?.querySelectorAll('.size-opt').forEach(x => {
      x.classList.remove('active');
      x.setAttribute('aria-pressed', 'false');
    });
    sizeBtn.classList.add('active');
    sizeBtn.setAttribute('aria-pressed', 'true');
  }
  const colorBtn = e.target.closest('.color-opt');
  if (colorBtn) {
    colorBtn.parentElement?.querySelectorAll('.color-opt').forEach(x => {
      x.classList.remove('active');
      x.setAttribute('aria-pressed', 'false');
    });
    colorBtn.classList.add('active');
    colorBtn.setAttribute('aria-pressed', 'true');
  }
});

// Wishlist heart pop: runs on tap so the heart reacts before the round-trip finishes.
document.addEventListener('click', function (e) {
  const btn = e.target.closest ? e.target.closest('.wish-btn, .pd-wish') : null;
  if (!btn) return;
  btn.classList.remove('is-pop');
  void btn.offsetWidth;
  btn.classList.add('is-pop');
  btn.addEventListener('animationend', function () { btn.classList.remove('is-pop'); }, { once: true });
});

// ─── Filter Drawer (Offcanvas) ───────────────────────────────
function initFilterDrawer() {
  const toggle   = document.getElementById('filterDrawerToggle');
  const drawer   = document.getElementById('filterDrawer');
  const backdrop = document.getElementById('filterBackdrop');
  const closeBtn = document.getElementById('filterDrawerClose');
  if (!toggle || !drawer) return;

  let lastFocus = null;

  function openDrawer() {
    lastFocus = document.activeElement;
    drawer.classList.add('open');
    if (backdrop) backdrop.classList.add('open');
    document.body.style.overflowY = 'hidden';
    toggle.setAttribute('aria-expanded', 'true');
    drawer.setAttribute('aria-hidden', 'false');
    drawer.removeAttribute('inert');
    if (closeBtn) setTimeout(() => closeBtn.focus(), 30);
  }
  function closeDrawer() {
    const wasOpen = drawer.classList.contains('open');
    drawer.classList.remove('open');
    if (backdrop) backdrop.classList.remove('open');
    document.body.style.overflowY = '';
    toggle.setAttribute('aria-expanded', 'false');
    drawer.setAttribute('aria-hidden', 'true');
    drawer.setAttribute('inert', '');
    if (wasOpen && lastFocus && typeof lastFocus.focus === 'function') lastFocus.focus();
  }

  let toggling = false;
  toggle.addEventListener('click', (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (toggling) return;
    toggling = true;
    setTimeout(() => { toggling = false; }, 350);
    drawer.classList.contains('open') ? closeDrawer() : openDrawer();
  });
  if (closeBtn)   closeBtn.addEventListener('click', (e) => { e.stopPropagation(); closeDrawer(); });
  if (backdrop)   backdrop.addEventListener('click', (e) => { e.stopPropagation(); closeDrawer(); });

  // Keep Tab inside the open filter drawer.
  drawer.addEventListener('keydown', function (e) {
    if (e.key !== 'Tab') return;
    const items = drawer.querySelectorAll(
      'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (!items.length) return;
    const first = items[0];
    const last = items[items.length - 1];
    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
  });
  if (window.__fdEscHandler) document.removeEventListener('keydown', window.__fdEscHandler);
  window.__fdEscHandler = (e) => { if (e.key === 'Escape') closeDrawer(); };
  document.addEventListener('keydown', window.__fdEscHandler);

  drawer.querySelectorAll('input[type="radio"]').forEach(radio => {
    radio.addEventListener('change', function () {
      const group = this.closest('.filter-chip-row');
      if (!group) return;
      group.querySelectorAll('.filter-chip-item').forEach(l => l.classList.remove('checked'));
      this.closest('.filter-chip-item')?.classList.add('checked');
    });
  });

  drawer.querySelectorAll('input[type="checkbox"]').forEach(box => {
    box.addEventListener('change', function () {
      this.closest('.filter-chip-item')?.classList.toggle('checked', this.checked);
    });
  });
}
pageInit(initFilterDrawer);

// ─── Announcement bar: rotating messages + dismiss ──────────
(function () {
  const bar = document.getElementById('announceBar');
  const closeBtn = document.getElementById('announceClose');
  if (!bar || !closeBtn) return;

  const msgs = Array.from(bar.querySelectorAll('.announce-msg'));
  const status = document.getElementById('announceStatus');
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  let index = 0;
  let timer = null;

  function isDismissed() {
    return document.documentElement.classList.contains('announce-dismissed');
  }
  function show(i) {
    msgs.forEach((m, n) => m.classList.toggle('is-active', n === i));
    index = i;
  }
  function start() {
    if (timer || reduceMotion || isDismissed() || msgs.length < 2) return;
    timer = setInterval(() => show((index + 1) % msgs.length), 5000);
  }
  function stop() {
    if (!timer) return;
    clearInterval(timer);
    timer = null;
  }

  // Hold the current message while the pointer, keyboard focus or tab is active.
  bar.addEventListener('mouseenter', stop);
  bar.addEventListener('mouseleave', start);
  bar.addEventListener('focusin', stop);
  bar.addEventListener('focusout', start);
  document.addEventListener('visibilitychange', () => (document.hidden ? stop() : start()));

  start();

  closeBtn.addEventListener('click', function () {
    stop();
    document.documentElement.classList.add('announce-dismissed');
    try { localStorage.setItem('amal.announce.dismissed', '1'); } catch (e) { /* private mode */ }
    if (status) status.textContent = 'Announcements hidden';
    const brand = document.querySelector('.ac-brand');
    if (brand && typeof brand.focus === 'function') brand.focus();
  });
})();

// ─── Mini Cart Drawer ────────────────────────────────────────
(function () {
  const link = document.getElementById('cartLink');
  const drawer = document.getElementById('mcDrawer');
  const backdrop = document.getElementById('mcBackdrop');
  if (!link || !drawer || !backdrop) return;

  const body = document.getElementById('mcBody');
  const foot = document.getElementById('mcFoot');
  const closeBtn = document.getElementById('mcClose');
  const countEl = document.getElementById('mcCount');

  const money = n => 'PKR ' + Number(n).toLocaleString('en-US', { maximumFractionDigits: 0 });
  const esc = s => { const d = document.createElement('div'); d.textContent = s || ''; return d.innerHTML; };

  let lastFocus = null;

  function openDrawer() {
    lastFocus = document.activeElement;
    drawer.classList.add('open');
    backdrop.classList.add('show');
    drawer.setAttribute('aria-hidden', 'false');
    drawer.removeAttribute('inert');
    loadCart();
    setTimeout(() => closeBtn.focus(), 30);
  }
  function closeDrawer() {
    const wasOpen = drawer.classList.contains('open');
    drawer.classList.remove('open');
    backdrop.classList.remove('show');
    drawer.setAttribute('aria-hidden', 'true');
    drawer.setAttribute('inert', '');
    if (wasOpen && lastFocus && typeof lastFocus.focus === 'function') lastFocus.focus();
  }

  async function loadCart() {
    body.innerHTML = '<div class="mc-loading"><div class="mc-spin"></div></div>';
    foot.style.display = 'none';
    try {
      const res = await fetch('/Cart/Mini');
      const data = await res.json();
      render(data);
    } catch {
      body.innerHTML = '<div class="mc-blocked"><i class="bi bi-exclamation-circle"></i><p>Could not load your cart. Please try again.</p></div>';
    }
  }

  function render(d) {
    if (d.blocked) {
      countEl.textContent = '';
      body.innerHTML = '<div class="mc-blocked"><i class="bi bi-shield-lock"></i><p>Admin accounts cannot shop.</p></div>';
      return;
    }
    countEl.textContent = d.count > 0 ? '(' + d.count + ')' : '';
    if (d.isEmpty || !d.items || d.items.length === 0) {
      body.innerHTML = '<div class="mc-empty"><i class="bi bi-bag-x"></i><p>Your cart is empty.</p><a href="/" class="btn btn-dark btn-sm rounded-pill">Start Shopping</a></div>';
      return;
    }

    let html = '';
    d.items.forEach(it => {
      const meta = [];
      if (it.size) meta.push('Size: ' + esc(it.size));
      if (it.color) meta.push('Color: ' + esc(it.color));
      html += '<div class="mc-item">' +
        '<a href="' + it.url + '"><img src="' + esc(it.image) + '" alt="' + esc(it.name) + '" onerror="this.src=\'/images/placeholder.jpg\'" /></a>' +
        '<div class="mc-item-info">' +
          '<p class="n"><a href="' + it.url + '">' + esc(it.name) + '</a></p>' +
          (meta.length ? '<p class="m">' + meta.join(' &middot; ') + '</p>' : '') +
          '<div class="mc-item-bottom">' +
            '<span class="mc-qty">Qty: ' + it.qty + '</span>' +
            '<span class="mc-line">' + money(it.lineTotal) + '</span>' +
          '</div>' +
        '</div>' +
      '</div>';
    });
    body.innerHTML = html;

    // Free delivery progress
    const freeRow = document.getElementById('mcFreeRow');
    let freeHtml = '';
    if (Number(d.freeThreshold) > 0) {
      if (Number(d.delivery) === 0) {
        freeHtml = '<div class="mc-free"><i class="bi bi-truck"></i> You have free delivery!</div>';
      } else {
        const left = Number(d.freeThreshold) - Number(d.subtotal);
        const pct = Math.min(100, Math.round((Number(d.subtotal) / Number(d.freeThreshold)) * 100));
        freeHtml = '<div class="mc-free"><i class="bi bi-truck"></i> ' + money(left) + ' away from free delivery</div>' +
                   '<div class="mc-free-bar"><span style="width:' + pct + '%"></span></div>';
      }
    }
    freeRow.innerHTML = freeHtml;
    freeRow.style.display = freeHtml ? 'block' : 'none';

    document.getElementById('mcSubtotal').textContent = money(d.subtotal);
    const delRow = document.getElementById('mcDelRow');
    if (Number(d.delivery) > 0) {
      delRow.style.display = 'flex';
      document.getElementById('mcDelivery').textContent = money(d.delivery);
    } else {
      delRow.style.display = 'none';
    }
    document.getElementById('mcGrand').textContent = money(d.grand);
    foot.style.display = 'block';
  }

  link.addEventListener('click', e => { e.preventDefault(); e.stopPropagation(); openDrawer(); });
  closeBtn.addEventListener('click', closeDrawer);
  backdrop.addEventListener('click', closeDrawer);
  const cont = document.getElementById('mcContinue');
  if (cont) cont.addEventListener('click', closeDrawer);
  document.addEventListener('keydown', e => { if (e.key === 'Escape') closeDrawer(); });
  drawer.addEventListener('click', e => { if (e.target.closest('a')) closeDrawer(); });

  // Keep Tab inside the open drawer so keyboard users cannot land behind it.
  drawer.addEventListener('keydown', function (e) {
    if (e.key !== 'Tab') return;
    const items = drawer.querySelectorAll(
      'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (!items.length) return;
    const first = items[0];
    const last = items[items.length - 1];
    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
  });

  // Exposed so add-to-cart can show the fresh cart right after an AJAX add.
  window.__openMiniCart = openDrawer;
})();

// ═══════════ AJAX Add to Cart (page stays put) ═════════════════════
// PDP form, sticky bar and Quick Add links post to /Cart/AddJson so the
// shopper never loses their place: thin top progress bar, badge bump and
// the mini cart drawer with the new item.
(function () {
  const meta = document.querySelector('meta[name="csrf-token"]');
  let bar = null;
  let fill = null;
  let tick = null;

  function token() {
    return (meta && meta.getAttribute('content')) || '';
  }

  function startBar() {
    if (!bar) {
      bar = document.createElement('div');
      bar.className = 'ajax-progress';
      bar.setAttribute('aria-hidden', 'true');
      fill = document.createElement('span');
      bar.appendChild(fill);
      document.body.appendChild(bar);
    }
    clearInterval(tick);
    bar.classList.add('active');
    fill.style.transition = 'none';
    fill.style.width = '8%';
    void fill.offsetWidth;
    fill.style.transition = 'width 0.35s ease';
    fill.style.width = '38%';
    let pct = 38;
    tick = setInterval(function () {
      pct = Math.min(92, pct + Math.random() * 7);
      fill.style.width = pct + '%';
    }, 200);
  }

  function endBar() {
    clearInterval(tick);
    if (!bar) return;
    fill.style.width = '100%';
    setTimeout(function () {
      bar.classList.remove('active');
      setTimeout(function () { fill.style.width = '0%'; }, 260);
    }, 220);
  }

  function toast(message, ok) {
    const old = document.getElementById('ajaxToast');
    if (old) old.remove();
    const el = document.createElement('div');
    el.id = 'ajaxToast';
    el.className = 'toast ' + (ok ? 'toast-success' : 'toast-error');
    el.setAttribute('role', 'status');
    const icon = document.createElement('i');
    icon.className = ok ? 'bi bi-check-circle-fill' : 'bi bi-x-circle';
    el.appendChild(icon);
    el.appendChild(document.createTextNode(' ' + message));
    document.body.appendChild(el);
    setTimeout(function () { if (el.parentNode) el.remove(); }, 4000);
  }

  function bumpBadge(count) {
    const badge = document.getElementById('cartBadge');
    if (!badge) return;
    badge.textContent = count > 0 ? count : '0';
    badge.classList.remove('bump');
    void badge.offsetWidth;
    badge.classList.add('bump');
  }

  async function add(payload, trigger) {
    if (trigger) trigger.classList.add('is-loading');
    startBar();
    try {
      const body = new URLSearchParams();
      body.set('productId', payload.productId || '');
      body.set('quantity', payload.quantity || '1');
      body.set('size', payload.size || '');
      body.set('color', payload.color || '');
      body.set('__RequestVerificationToken', token());

      const res = await fetch('/Cart/AddJson', {
        method: 'POST',
        credentials: 'same-origin',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
          'Accept': 'application/json',
          'X-Requested-With': 'XMLHttpRequest'
        },
        body: body.toString()
      });
      if (!res.ok) throw new Error('HTTP ' + res.status);
      const data = await res.json();

      if (data.ok) {
        bumpBadge(data.cartCount);
        if (window.__openMiniCart) window.__openMiniCart();
        else toast(data.message, true);
      } else {
        toast(data.message || 'Could not add to cart.', false);
      }
      return data;
    } catch (err) {
      toast('Could not add to cart. Please try again.', false);
      return { ok: false };
    } finally {
      endBar();
      if (trigger) trigger.classList.remove('is-loading');
    }
  }

  // PDP form (the sticky bar submits this same form). Registered before
  // ajax-nav, which then backs off because the event is already handled.
  // The inline size/colour check runs first and cancels, so we respect it.
  document.addEventListener('submit', function (e) {
    const form = e.target;
    if (!form || !form.classList || !form.classList.contains('add-cart-form')) return;
    if (e.defaultPrevented) return;
    e.preventDefault();
    if (form.dataset.busy === '1') return;
    form.dataset.busy = '1';

    const data = new FormData(form);
    const btn = e.submitter || form.querySelector('button[type="submit"]');
    add({
      productId: data.get('productId'),
      quantity: data.get('quantity'),
      size: data.get('size'),
      color: data.get('color')
    }, btn).finally(function () { delete form.dataset.busy; });
  });

  // Quick Add links on product cards (non-variant products only).
  document.addEventListener('click', function (e) {
    if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
    const link = e.target.closest ? e.target.closest('a.quick-add[href]') : null;
    if (!link) return;
    let url;
    try { url = new URL(link.href, location.origin); } catch (err) { return; }
    if (url.pathname.replace(/\/+$/, '').toLowerCase() !== '/cart/add') return;

    e.preventDefault();
    if (link.getAttribute('data-busy') === '1') return;
    link.setAttribute('data-busy', '1');
    add({
      productId: url.searchParams.get('productId'),
      quantity: url.searchParams.get('quantity'),
      size: url.searchParams.get('size'),
      color: url.searchParams.get('color')
    }, link).finally(function () { link.removeAttribute('data-busy'); });
  });
})();

// ═══════════ Live Search Suggestions ═══════════════════════════════
// Typeahead under the navbar search boxes: matching products + popular terms.
(function () {
  const forms = document.querySelectorAll('form.ac-search');
  if (!forms.length) return;

  const cache = Object.create(null);
  const MAX_CACHE = 40;

  function esc(s) {
    return String(s == null ? '' : s).replace(/[&<>"']/g, c => (
      { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]
    ));
  }

  function money(n) {
    return 'PKR ' + Math.round(Number(n) || 0).toLocaleString('en-PK');
  }

  function remember(key, data) {
    const keys = Object.keys(cache);
    if (keys.length >= MAX_CACHE) delete cache[keys[0]];
    cache[key] = data;
  }

  forms.forEach(form => {
    const input = form.querySelector('input[type="search"]');
    if (!input) return;

    input.setAttribute('role', 'combobox');
    input.setAttribute('aria-autocomplete', 'list');
    input.setAttribute('aria-expanded', 'false');

    const box = document.createElement('div');
    box.className = 'search-suggest';
    box.setAttribute('role', 'listbox');
    box.setAttribute('aria-label', 'Search suggestions');
    box.hidden = true;
    form.appendChild(box);

    let timer = null;
    let activeIndex = -1;
    let items = [];

    function close() {
      box.hidden = true;
      box.innerHTML = '';
      activeIndex = -1;
      items = [];
      input.setAttribute('aria-expanded', 'false');
    }

    function paint(data, query) {
      const products = (data && data.products) || [];
      const terms = (data && data.terms) || [];
      let html = '';

      if (products.length) {
        html += '<div class="ss-group">Products</div>';
        products.forEach(p => {
          html += '<a class="ss-item" role="option" aria-selected="false" href="/Products/Detail/' + p.id + '">' +
                    '<img src="' + esc(p.image) + '" alt="" loading="lazy" onerror="this.src=\'/images/placeholder.jpg\'" />' +
                    '<span class="ss-text"><span class="ss-name">' + esc(p.name) + '</span>' +
                    '<span class="ss-price">' + money(p.price) + '</span></span>' +
                  '</a>';
        });
      }

      if (terms.length) {
        html += '<div class="ss-group">' + (products.length ? 'More like this' : 'Popular searches') + '</div>';
        html += '<div class="ss-chips">';
        terms.forEach(t => {
          html += '<a class="ss-chip" href="/Products/Search?q=' + encodeURIComponent(t) + '">' + esc(t) + '</a>';
        });
        html += '</div>';
      }

      if (!html) {
        html = '<div class="ss-empty">No matches for &ldquo;' + esc(query) + '&rdquo;. Press Enter to search all products.</div>';
      }

      box.innerHTML = html;
      box.hidden = false;
      input.setAttribute('aria-expanded', 'true');
      items = Array.prototype.slice.call(box.querySelectorAll('.ss-item'));
      activeIndex = -1;
    }

    function load(query) {
      if (cache[query]) { paint(cache[query], query); return; }
      fetch('/Products/Suggest?q=' + encodeURIComponent(query), { headers: { 'Accept': 'application/json' } })
        .then(r => (r.ok ? r.json() : null))
        .then(data => {
          if (!data) return;
          remember(query, data);
          if (input.value.trim() === query) paint(data, query);
        })
        .catch(() => { /* silent — plain search still works */ });
    }

    function schedule() {
      const q = input.value.trim();
      clearTimeout(timer);
      timer = setTimeout(() => load(q.length >= 2 ? q : ''), 170);
    }

    function highlight(idx) {
      items.forEach((el, i) => {
        el.classList.toggle('is-active', i === idx);
        el.setAttribute('aria-selected', i === idx ? 'true' : 'false');
      });
      if (items[idx]) items[idx].scrollIntoView({ block: 'nearest' });
    }

    input.addEventListener('focus', () => {
      load(input.value.trim().length >= 2 ? input.value.trim() : '');
    });
    input.addEventListener('input', schedule);

    input.addEventListener('keydown', e => {
      if (box.hidden) return;
      if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
        if (!items.length) return;
        e.preventDefault();
        activeIndex = e.key === 'ArrowDown'
          ? (activeIndex + 1) % items.length
          : (activeIndex - 1 + items.length) % items.length;
        highlight(activeIndex);
      } else if (e.key === 'Enter') {
        if (activeIndex >= 0 && items[activeIndex]) {
          e.preventDefault();
          window.location.href = items[activeIndex].getAttribute('href');
        } else {
          close();
        }
      } else if (e.key === 'Escape') {
        close();
      }
    });

    form.addEventListener('submit', close);
    document.addEventListener('click', e => {
      if (!form.contains(e.target)) close();
    });
    document.addEventListener('keydown', e => {
      if (e.key === 'Escape' && !box.hidden) close();
    });
  });
})();
