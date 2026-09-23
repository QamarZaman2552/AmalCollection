/* Amal Collection UI Prototype – interactions (Bootstrap 5) */

document.addEventListener('DOMContentLoaded', () => {
  /* ─── Close mobile sidebar on nav link / search click ─── */
  const mainNav = document.getElementById('mainNav');
  if (mainNav) {
    const closeNav = () => {
      const el = bootstrap.Collapse.getInstance(mainNav);
      if (el && mainNav.classList.contains('show')) {
        el.hide();
      }
    };
    mainNav.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', closeNav);
    });
    mainNav.querySelectorAll('form.ac-search').forEach(form => {
      form.addEventListener('submit', closeNav);
    });
  }

  /* ─── Hero Slider ─────────────────────────────── */
  const hero = document.getElementById('heroSlider');
  if (hero) {
    const slides = hero.querySelectorAll('.hero-slide');
    const dots = hero.querySelectorAll('.hero-dot');
    const num = document.getElementById('heroNum');
    const bar = document.getElementById('heroBar');
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
  }

  /* ─── Filter chips ────────────────────────────── */
  document.querySelectorAll('.size-filter').forEach(group => {
    group.querySelectorAll('.size-chip').forEach(chip => {
      chip.addEventListener('click', () => {
        group.querySelectorAll('.size-chip').forEach(c => c.classList.remove('active'));
        chip.classList.add('active');
      });
    });
  });

  /* ─── Quick add feedback ──────────────────────── */
  document.querySelectorAll('.quick-add').forEach(btn => {
    btn.addEventListener('click', e => {
      e.preventDefault();
      e.stopPropagation();
      const old = btn.innerHTML;
      btn.innerHTML = '<i class="bi bi-check-lg"></i> Added';
      btn.style.background = 'var(--green)';
      setTimeout(() => {
        btn.innerHTML = old;
        btn.style.background = '';
      }, 1500);
    });
  });

  /* ─── Wishlist heart toggle (Bootstrap Icons) ─── */
  document.querySelectorAll('.wish-btn, .pd-wish').forEach(btn => {
    btn.addEventListener('click', e => {
      e.preventDefault();
      e.stopPropagation();
      const icon = btn.querySelector('i');
      if (!icon) return;
      const active = icon.classList.contains('bi-heart-fill');
      icon.classList.toggle('bi-heart-fill', !active);
      icon.classList.toggle('bi-heart', active);
      btn.classList.toggle('is-active', !active);
      if (btn.classList.contains('pd-wish')) {
        btn.style.color = !active ? 'var(--red)' : '';
        btn.style.borderColor = !active ? 'var(--red)' : '';
      }
    });
  });

  /* ─── Mobile search submit → close sidebar ────── */
  document.querySelectorAll('.ac-mobile-search form').forEach(form => {
    form.addEventListener('submit', e => {
      e.preventDefault();
      const nav = document.getElementById('mainNav');
      if (nav && nav.classList.contains('show')) {
        bootstrap.Collapse.getOrCreateInstance(nav).hide();
      }
    });
  });
});
