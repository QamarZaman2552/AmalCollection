// ─── Mobile Hamburger Menu ──────────────────────────────
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

// ─── Cart Badge ────────────────────────────────────────
async function updateCartBadge() {
  try {
    const res = await fetch('/Cart/Count');
    const count = await res.json();
    const badge = document.getElementById('cartBadge');
    if (badge) badge.textContent = count > 0 ? count : '0';
  } catch {}
}
updateCartBadge();

// ─── Wishlist Badge ─────────────────────────────────────
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


// ─── Auto-dismiss Toast ─────────────────────────────────
const toast = document.getElementById('toast') || document.getElementById('toast-success') || document.getElementById('toast-error');
if (toast) setTimeout(() => toast.remove(), 4000);

// ─── Chatbot Widget ─────────────────────────────────────
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
    addMsg('⚠️ Connection error. Please try again.', 'bot');
  } finally {
    chatSend.disabled = false;
    chatInput.focus();
  }
}

// ─── Hero Slider ─────────────────────────────────────────
(function () {
  const slider    = document.getElementById('heroSlider');
  if (!slider) return;

  const slides    = slider.querySelectorAll('.hero-slide');
  const dots      = slider.querySelectorAll('.hs-dot');
  const prevBtn   = document.getElementById('hsPrev');
  const nextBtn   = document.getElementById('hsNext');
  const progFill  = document.getElementById('hsProgress');

  const DURATION  = 5000; // ms per slide
  let   current   = 0;
  let   autoTimer = null;
  let   paused    = false;

  /* ── Go to a specific slide ── */
  function goTo(index) {
    const total = slides.length;
    const next  = ((index % total) + total) % total;
    if (next === current) return;

    slides[current].classList.remove('active');
    dots[current].classList.remove('active');
    dots[current].setAttribute('aria-selected', 'false');

    current = next;
    slides[current].classList.add('active');
    dots[current].classList.add('active');
    dots[current].setAttribute('aria-selected', 'true');

    resetProgressBar();
  }

  /* ── Progress bar animation ── */
  function resetProgressBar() {
    if (!progFill) return;
    progFill.style.transition = 'none';
    progFill.style.width      = '0%';
    // force reflow so transition resets
    void progFill.offsetWidth;
    progFill.style.transition = `width ${DURATION}ms linear`;
    progFill.style.width      = '100%';
  }

  /* ── Auto-play ── */
  function startAuto() {
    if (paused) return;
    stopAuto();
    autoTimer = setInterval(() => goTo(current + 1), DURATION);
    resetProgressBar();
  }

  function stopAuto() {
    clearInterval(autoTimer);
    autoTimer = null;
    if (progFill) {
      progFill.style.transition = 'none';
      progFill.style.width      = '0%';
    }
  }

  function resetAuto() { stopAuto(); startAuto(); }

  /* ── Controls ── */
  if (prevBtn) prevBtn.addEventListener('click', () => { goTo(current - 1); resetAuto(); });
  if (nextBtn) nextBtn.addEventListener('click', () => { goTo(current + 1); resetAuto(); });

  dots.forEach(dot => {
    dot.addEventListener('click', () => {
      goTo(parseInt(dot.dataset.goto, 10));
      resetAuto();
    });
  });

  /* ── Pause on hover ── */
  slider.addEventListener('mouseenter', () => { paused = true;  stopAuto(); });
  slider.addEventListener('mouseleave', () => { paused = false; startAuto(); });

  /* ── Keyboard navigation ── */
  slider.setAttribute('tabindex', '0');
  slider.addEventListener('keydown', e => {
    if (e.key === 'ArrowLeft')  { goTo(current - 1); resetAuto(); }
    if (e.key === 'ArrowRight') { goTo(current + 1); resetAuto(); }
  });

  /* ── Touch / swipe support ── */
  let touchX = 0;
  slider.addEventListener('touchstart', e => {
    touchX = e.changedTouches[0].clientX;
  }, { passive: true });
  slider.addEventListener('touchend', e => {
    const diff = touchX - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 50) {
      diff > 0 ? goTo(current + 1) : goTo(current - 1);
      resetAuto();
    }
  }, { passive: true });

  /* ── Start ── */
  startAuto();
})();

// ─── rn-Slider (Homepage Hero Banner) ────────────────────
(function () {
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
})();// ─── Scroll Reveal ────────────────────────────────────────
(function () {
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
})();

// ─── Card Mouse-Glow Tracker ──────────────────────────────
document.addEventListener('mousemove', (e) => {
  document.querySelectorAll('.product-card').forEach(card => {
    const rect = card.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width  * 100).toFixed(1) + '%';
    const y = ((e.clientY - rect.top)  / rect.height * 100).toFixed(1) + '%';
    card.style.setProperty('--mx', x);
    card.style.setProperty('--my', y);
  });
});

// ─── Button Ripple Effect ─────────────────────────────────
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

// ─── Navbar scroll shadow ─────────────────────────────────
window.addEventListener('scroll', () => {
  const nav = document.querySelector('.navbar');
  if (!nav) return;
  if (window.scrollY > 10) {
    nav.style.boxShadow = '0 4px 24px rgba(0,0,0,0.35)';
    nav.style.background = 'rgba(10,13,20,0.96)';
  } else {
    nav.style.boxShadow = '';
    nav.style.background = 'rgba(10,13,20,0.85)';
  }
}, { passive: true });

// ─── Password: show/hide + strength meter + match ────────
(function () {
  function scorePassword(pw) {
    if (!pw) return 0;
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[a-z]/.test(pw) && /[A-Z]/.test(pw)) score++;
    if (/\d/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    if (pw.length >= 12) score++;
    return score; // 0–5
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
        toggle.textContent = show ? '🙈' : '👁️';
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
        hint.textContent = ok ? '✓ Passwords match' : '✗ Passwords do not match';
        hint.className = 'pw-match ' + (ok ? 'ok' : 'no');
      }
    }
    m.addEventListener('input', sync);
    target.addEventListener('input', sync);
  });
})();

// ─── Auth Pages: prevent zoom + shake on mobile ──────
(function () {
  var authPage = document.querySelector('.auth-page');
  if (!authPage) return;

  // Prevent pinch zoom on auth pages
  document.addEventListener('gesturestart', function (e) { e.preventDefault(); });

  // Prevent rubber-band scroll shake on iOS/Android
  var startX = 0, startY = 0;
  authPage.addEventListener('touchstart', function (e) {
    startX = e.touches[0].clientX;
    startY = e.touches[0].clientY;
  }, { passive: true });

  authPage.addEventListener('touchmove', function (e) {
    var dx = Math.abs(e.touches[0].clientX - startX);
    var dy = Math.abs(e.touches[0].clientY - startY);
    // If mostly horizontal swipe, prevent default to stop page shake
    if (dx > dy && dx > 10) {
      e.preventDefault();
    }
  }, { passive: false });
})();

// ─── Light/Dark Theme Toggle ───────────────────────
(function () {
  var html = document.documentElement;
  var KEY = 'baazwix_theme';

  function setTheme(theme) {
    html.setAttribute('data-theme', theme);
    localStorage.setItem(KEY, theme);
  }

  function toggleTheme() {
    var current = html.getAttribute('data-theme');
    setTheme(current === 'light' ? 'dark' : 'light');
  }

  var btn = document.getElementById('themeToggle');
  var mobileBtn = document.getElementById('mobileThemeToggle');
  if (btn) btn.addEventListener('click', toggleTheme);
  if (mobileBtn) mobileBtn.addEventListener('click', toggleTheme);
})();

