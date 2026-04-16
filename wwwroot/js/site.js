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

// ─── Auto-dismiss Toast ─────────────────────────────────
const toast = document.getElementById('toast');
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
    if (chatWin.classList.contains('open')) chatInput.focus();
  });

  chatClose.addEventListener('click', () => chatWin.classList.remove('open'));

  chatSend.addEventListener('click', sendMessage);
  chatInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
  });
}

function addMsg(text, type) {
  const msg = document.createElement('div');
  msg.className = `chat-msg ${type}`;
  const bubble_el = document.createElement('div');
  bubble_el.className = 'chat-bubble';
  bubble_el.textContent = text;
  msg.appendChild(bubble_el);
  chatMsgs.appendChild(msg);
  chatMsgs.scrollTop = chatMsgs.scrollHeight;
  return msg;
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
    const res = await fetch('/Chatbot/Chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message: text })
    });
    const data = await res.json();
    hideTyping();
    addMsg(data.reply, 'bot');
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
})();



