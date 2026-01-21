// ArchiX Diagnostic Helpers (Development Only)
// Source: src/ArchiX.Library.Web/wwwroot/js/archix/diagnostics.js

(() => {
  'use strict';

  if (!window.ArchiX) window.ArchiX = {};

  // Production guard: all functions check Debug flag
  function isDebugEnabled() {
    return window.ArchiX.Debug === true;
  }

  function log(message, data) {
    if (!isDebugEnabled()) return;
    const timestamp = new Date().toISOString();
    console.log(`[ArchiX Debug ${timestamp}]`, message, data || '');
  }

  // Diagnose Tab: HTML + Computed Styles + Extract Info
  window.ArchiX.diagnoseTab = function(tabId) {
    if (!isDebugEnabled()) {
      console.warn('ArchiX.Debug is disabled. Set ArchiX.Debug = true to enable diagnostics.');
      return;
    }

    const pane = document.querySelector(`.tab-pane[data-tab-id="${CSS.escape(tabId)}"]`);
    if (!pane) {
      log('diagnoseTab: Tab pane not found', { tabId });
      return { error: 'Tab pane not found', tabId };
    }

    const content = pane.querySelector('.archix-tab-content');
    if (!content) {
      log('diagnoseTab: .archix-tab-content not found', { tabId });
      return { error: '.archix-tab-content not found', tabId };
    }

    const computed = window.getComputedStyle(content);
    const result = {
      tabId,
      html: content.innerHTML.substring(0, 500) + '...',
      htmlLength: content.innerHTML.length,
      computedStyles: {
        width: computed.width,
        height: computed.height,
        maxWidth: computed.maxWidth,
        marginLeft: computed.marginLeft,
        marginRight: computed.marginRight,
        paddingLeft: computed.paddingLeft,
        paddingRight: computed.paddingRight,
        display: computed.display,
        overflow: computed.overflow
      },
      domNodeCount: content.querySelectorAll('*').length
    };

    log('diagnoseTab result:', result);
    return result;
  };

  // Dump Extract Chain: Test which selector wins
  window.ArchiX.dumpExtractChain = async function(url) {
    if (!isDebugEnabled()) {
      console.warn('ArchiX.Debug is disabled. Set ArchiX.Debug = true to enable diagnostics.');
      return;
    }

    log('dumpExtractChain: Fetching', { url });

    try {
      const response = await fetch(url, {
        headers: {
          'X-ArchiX-Tab': '1',
          'X-Requested-With': 'XMLHttpRequest'
        }
      });

      const html = await response.text();
      const parser = new DOMParser();
      const doc = parser.parseFromString(html, 'text/html');

      const chain = [];

      // Priority 1: #tab-main
      const tabMain = doc.querySelector('#tab-main');
      if (tabMain) {
        chain.push({
          selector: '#tab-main',
          found: true,
          htmlLength: tabMain.innerHTML.length
        });
      } else {
        chain.push({ selector: '#tab-main', found: false });
      }

      // Priority 2: .archix-work-area
      const workArea = doc.querySelector('.archix-work-area');
      if (workArea) {
        chain.push({
          selector: '.archix-work-area',
          found: true,
          htmlLength: workArea.innerHTML.length
        });
      } else {
        chain.push({ selector: '.archix-work-area', found: false });
      }

      // Priority 3: main
      const main = doc.querySelector('main.archix-shell-main') || doc.querySelector('main[role="main"]');
      if (main) {
        chain.push({
          selector: 'main',
          found: true,
          htmlLength: main.innerHTML.length
        });
      } else {
        chain.push({ selector: 'main', found: false });
      }

      const result = {
        url,
        responseSize: html.length,
        extractChain: chain,
        winner: chain.find(c => c.found) || null
      };

      log('dumpExtractChain result:', result);
      return result;
    } catch (error) {
      log('dumpExtractChain error:', error);
      return { error: error.message, url };
    }
  };

  // CSS Debug Mode: Add borders + log specificity
  window.ArchiX.cssDebugMode = function() {
    if (!isDebugEnabled()) {
      console.warn('ArchiX.Debug is disabled. Set ArchiX.Debug = true to enable diagnostics.');
      return;
    }

    // Add debug borders
    const style = document.createElement('style');
    style.id = 'archix-css-debug';
    style.textContent = `
      #archix-tabhost-panes { border: 3px solid red !important; }
      .archix-tab-content { border: 3px solid blue !important; }
      .archix-tab-content .container { border: 3px solid green !important; }
      .archix-tab-content .row { border: 2px dashed orange !important; }
      .archix-tab-content .col-md-3,
      .archix-tab-content .col-md-4,
      .archix-tab-content .col-md-8,
      .archix-tab-content .col-md-9,
      .archix-tab-content .col-md-12 { border: 1px dotted purple !important; }
    `;
    document.head.appendChild(style);

    // Log specificity chain
    const specificity = [
      { selector: 'Bootstrap .container', value: '(0,0,1)', rule: 'margin: auto' },
      { selector: 'modern/main.css', value: '(0,0,2)', rule: 'general styles' },
      { selector: '#archix-tabhost-panes .archix-tab-content .container', value: '(1,0,3)', rule: 'margin: 0 !important (WINS)' }
    ];

    log('CSS Specificity Chain (low to high):', specificity);
    log('CSS Debug Mode enabled. Borders added: red=panes, blue=tab-content, green=container, orange=row, purple=col');

    return { enabled: true, specificity };
  };

  // Disable CSS Debug Mode
  window.ArchiX.cssDebugModeOff = function() {
    const style = document.getElementById('archix-css-debug');
    if (style) {
      style.remove();
      log('CSS Debug Mode disabled.');
    }
  };

  log('Diagnostic helpers loaded. Available commands: diagnoseTab(tabId), dumpExtractChain(url), cssDebugMode(), cssDebugModeOff()');
})();
