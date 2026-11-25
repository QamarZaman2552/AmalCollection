// Footer newsletter signup: posts in the background and reports inline.
(function () {
  'use strict';

  function showMessage(form, message, ok) {
    var msg = form.querySelector('.footer-newsletter__msg');
    if (!msg) return;
    msg.textContent = message;
    msg.classList.toggle('is-ok', ok);
    msg.classList.toggle('is-error', !ok);
  }

  document.addEventListener('submit', function (event) {
    var form = event.target.closest('form[data-newsletter]');
    if (!form) return;
    event.preventDefault();

    var input = form.querySelector('input[name="email"]');
    var button = form.querySelector('button[type="submit"]');
    if (!input || !button || button.disabled) return;

    if (!input.value.trim()) {
      showMessage(form, 'Enter your email address.', false);
      input.focus();
      return;
    }

    var label = button.textContent;
    button.disabled = true;
    button.textContent = 'Joining…';
    form.setAttribute('aria-busy', 'true');
    showMessage(form, '', true);

    fetch(form.action, {
      method: 'POST',
      body: new FormData(form),
      headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'Accept': 'application/json'
      },
      credentials: 'same-origin'
    })
      .then(function (response) {
        return response.json().catch(function () {
          return {
            ok: response.ok,
            message: response.ok ? "Thanks! You're on the list." : 'Something went wrong. Please try again.'
          };
        });
      })
      .then(function (data) {
        showMessage(form, data.message || 'Something went wrong. Please try again.', !!data.ok);
        if (data.ok) form.reset();
      })
      .catch(function () {
        showMessage(form, 'Network error. Please try again.', false);
      })
      .finally(function () {
        button.disabled = false;
        button.textContent = label;
        form.removeAttribute('aria-busy');
      });
  });
})();
