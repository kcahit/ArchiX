// UI options bootstrap.
// Single place to decide navigation mode (Tabbed vs FullPage) for the client.
//
// Current approach (minimal): default to Tabbed (matches DB seed), but allow
// turning it off via a querystring for troubleshooting.
//
// Example: /Dashboard?nav=fullpage
(() => {
  const params = new URLSearchParams(window.location.search);
  const nav = (params.get('nav') || '').toLowerCase();

  window.ArchiX = window.ArchiX || {};
  window.ArchiX.UiOptions = window.ArchiX.UiOptions || {};

  if (nav === 'fullpage' || nav === 'normal') {
    window.ArchiX.UiOptions.navigationMode = 'FullPage';
    return;
  }

  // Default (spec/#42 + DB seed): Tabbed.
  if (!window.ArchiX.UiOptions.navigationMode) {
    window.ArchiX.UiOptions.navigationMode = 'Tabbed';
  }
})();
