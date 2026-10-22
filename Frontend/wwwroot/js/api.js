// BeeMart API Client & Utility Functions
const API_CONFIG = {
  baseUrl: 'http://localhost:5000'
};

// Initialize configuration from frontend server if available
async function initConfig() {
  try {
    const res = await fetch('/api/config');
    if (res.ok) {
      const data = await res.json();
      if (data && data.apiUrl) {
        API_CONFIG.baseUrl = data.apiUrl;
      }
    }
  } catch (err) {
    // Fallback to default localhost:5000
    console.log('Using default backend URL: http://localhost:5000');
  }
}

// Session Management
function getSession() {
  const sessionStr = localStorage.getItem('beeMart_user');
  if (!sessionStr) return null;
  try {
    return JSON.parse(sessionStr);
  } catch (e) {
    return null;
  }
}

function setSession(user) {
  localStorage.setItem('beeMart_user', JSON.stringify(user));
}

function clearSession() {
  localStorage.removeItem('beeMart_user');
}

function requireAuth(redirectUrl = 'login.html') {
  const user = getSession();
  if (!user) {
    window.location.href = redirectUrl;
    return null;
  }
  return user;
}

// Toast Notification System
function showToast(message, type = 'info') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `
    <span>${message}</span>
    <button class="toast-close" onclick="this.parentElement.remove()">✕</button>
  `;

  container.appendChild(toast);

  setTimeout(() => {
    if (toast.parentElement) {
      toast.style.opacity = '0';
      toast.style.transition = 'opacity 0.3s ease';
      setTimeout(() => toast.remove(), 300);
    }
  }, 4000);
}

// Indonesian Currency Formatter (e.g. Rp 35.000)
function formatRupiah(number) {
  if (number === null || number === undefined || isNaN(number)) {
    return 'Rp 0';
  }
  const formatted = new Intl.NumberFormat('id-ID', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(number);
  return `Rp ${formatted}`;
}

// Date Formatter: YYYY-MM-DD HH:mm
function formatDateTime(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  const hours = String(d.getHours()).padStart(2, '0');
  const minutes = String(d.getMinutes()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}`;
}

// Generic API Caller
async function apiCall(endpoint, method = 'GET', body = null) {
  const url = `${API_CONFIG.baseUrl}${endpoint}`;
  const options = {
    method,
    headers: {
      'Content-Type': 'application/json'
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  try {
    const res = await fetch(url, options);
    const data = await res.json().catch(() => null);

    if (!res.ok) {
      const errorMsg = data && data.message ? data.message : `HTTP error: ${res.status}`;
      return { ok: false, status: res.status, message: errorMsg, data: null };
    }

    return { ok: true, status: res.status, message: data?.message || '', data: data?.data };
  } catch (err) {
    return { ok: false, status: 0, message: 'Tidak dapat terhubung ke server backend API.', data: null };
  }
}
