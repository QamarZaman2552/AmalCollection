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

if (bubble && chatWin) {
  bubble.addEventListener('click', () => {
    chatWin.classList.toggle('open');
    bubble.style.display = chatWin.classList.contains('open') ? 'none' : 'flex';
    if (chatWin.classList.contains('open')) chatInput.focus();
  });

  chatClose.addEventListener('click', () => { chatWin.classList.remove('open'); bubble.style.display = 'flex'; });
  document.addEventListener('click', (e) => {
    if (chatWin.classList.contains('open') && !chatWin.contains(e.target) && !bubble.contains(e.target)) {
      chatWin.classList.remove('open');
      bubble.style.display = 'flex';
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
    sizeBtn.parentElement?.querySelectorAll('.size-opt').forEach(x => x.classList.remove('active'));
    sizeBtn.classList.add('active');
  }
  const colorBtn = e.target.closest('.color-opt');
  if (colorBtn) {
    colorBtn.parentElement?.querySelectorAll('.color-opt').forEach(x => x.classList.remove('active'));
    colorBtn.classList.add('active');
  }
});

// â”€â”€â”€ Filter Drawer (Offcanvas) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function initFilterDrawer() {
  const toggle   = document.getElementById('filterDrawerToggle');
  const drawer   = document.getElementById('filterDrawer');
  const backdrop = document.getElementById('filterBackdrop');
  const closeBtn = document.getElementById('filterDrawerClose');
  if (!toggle || !drawer) return;

  function openDrawer() {
    drawer.classList.add('open');
    if (backdrop) backdrop.classList.add('open');
    document.body.style.overflowY = 'hidden';
    toggle.setAttribute('aria-expanded', 'true');
  }
  function closeDrawer() {
    drawer.classList.remove('open');
    if (backdrop) backdrop.classList.remove('open');
    document.body.style.overflowY = '';
    toggle.setAttribute('aria-expanded', 'false');
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
}
pageInit(initFilterDrawer);
