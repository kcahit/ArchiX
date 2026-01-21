// Session Guard: Logout sonrası link/geri tuşu ile erişimi engeller
// Development'ta da çalışır (AllowAnonymous olsa bile)
(() => {
  const SESSION_KEY = 'archix-session-active';
  const currentPath = window.location.pathname.toLowerCase();

  // Skip Login and Logout pages
  if (currentPath.startsWith('/login') || currentPath.startsWith('/logout')) {
    return;
  }

  // Skip static files
  if (currentPath.startsWith('/_content') || 
      currentPath.startsWith('/css') || 
      currentPath.startsWith('/js') || 
      currentPath.startsWith('/lib')) {
    return;
  }

  // Check if session is active
  const sessionActive = sessionStorage.getItem(SESSION_KEY);
  
  if (!sessionActive) {
    // No active session, redirect to login
    window.location.replace('/Login?reason=session-expired');
    return;
  }

  // Mark session as active (for new tabs/windows from same session)
  sessionStorage.setItem(SESSION_KEY, 'true');

  // Listen for storage events (logout from another tab)
  window.addEventListener('storage', (e) => {
    if (e.key === SESSION_KEY && !e.newValue) {
      window.location.replace('/Login?reason=session-expired');
    }
  });
})();
