// Double-submit guard ------------------------------------------------
// Opt-in per form: <form data-submit-guard data-guard-label="Placing order...">
// Quiet forms add data-guard-quiet to disable their icon button without a spinner.
// Lives in its own file so the admin layout gets it too (site.js is storefront only).
// Delegated on document, so it also works after AJAX navigation.
(function () {
  if (window.__submitGuardReady) return;
  window.__submitGuardReady = true;

  var UNLOCK_MS = 20000;

  function spinnerHtml() {
    return '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>';
  }

  function isInput(btn) { return btn.tagName === 'INPUT'; }

  function lockButton(btn, label) {
    if (!btn || btn.dataset.guardLocked === '1') return;
    // Quiet forms (icon-only admin buttons) get disabled without a spinner
    var ownerForm = btn.closest ? btn.closest('form') : null;
    var quiet = ownerForm && ownerForm.hasAttribute('data-guard-quiet');
    btn.dataset.guardLocked = '1';
    btn.dataset.guardOriginal = isInput(btn) ? btn.value : btn.innerHTML;
    if (!quiet) {
      if (label) {
        if (isInput(btn)) btn.value = label;
        else btn.innerHTML = spinnerHtml() + label;
      } else if (!isInput(btn)) {
        btn.insertAdjacentHTML('afterbegin', spinnerHtml());
      }
    }
    btn.classList.add('is-submitting');
    btn.setAttribute('aria-disabled', 'true');
    // Disable on the next tick: disabling during the submit event can cancel the POST in some browsers
    window.setTimeout(function () { btn.disabled = true; }, 0);
  }

  function unlockButton(btn) {
    if (!btn || btn.dataset.guardLocked !== '1') return;
    btn.disabled = false;
    btn.classList.remove('is-submitting');
    btn.removeAttribute('aria-disabled');
    if (isInput(btn)) btn.value = btn.dataset.guardOriginal || '';
    else btn.innerHTML = btn.dataset.guardOriginal || '';
    btn.dataset.guardLocked = '';
  }

  function releaseForm(form, btn) {
    form.dataset.submitting = '';
    form.removeAttribute('aria-busy');
    form.querySelectorAll('[data-guard-hidden]').forEach(function (h) { h.remove(); });
    unlockButton(btn);
    document.querySelectorAll('[data-guard-proxy]').forEach(unlockButton);
  }

  document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!form || form.tagName !== 'FORM' || !form.hasAttribute('data-submit-guard')) return;
    // Inline onsubmit="return confirm(...)" runs before this document listener, so a
    // cancelled confirm must not leave the button locked
    if (e.defaultPrevented) return;

    // Already in flight: swallow the repeat click / Enter press
    if (form.dataset.submitting === '1') {
      e.preventDefault();
      return;
    }
    form.dataset.submitting = '1';
    form.setAttribute('aria-busy', 'true');

    var btn = e.submitter || form.querySelector('button[type="submit"], input[type="submit"]');
    var label = form.getAttribute('data-guard-label');

    // A disabled button is not submitted, so mirror its name/value into a hidden field
    if (btn && btn.name && form.contains(btn)) {
      var hidden = document.createElement('input');
      hidden.type = 'hidden';
      hidden.name = btn.name;
      hidden.value = btn.value || '';
      hidden.setAttribute('data-guard-hidden', '');
      form.appendChild(hidden);
    }

    lockButton(btn, label);
    document.querySelectorAll('[data-guard-proxy]').forEach(function (proxy) {
      lockButton(proxy, proxy.getAttribute('data-guard-label') || label);
    });

    // If the response never navigates (network drop, download), give the form back
    window.setTimeout(function () { releaseForm(form, btn); }, UNLOCK_MS);

    // Back/forward cache: restore the button when the user returns
    window.addEventListener('pageshow', function onShow(ev) {
      if (!ev.persisted) return;
      window.removeEventListener('pageshow', onShow);
      releaseForm(form, btn);
    });
  });
})();

