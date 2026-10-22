// History Transaction Management
let historyCurrentPage = 1;
const historyPageSize = 20;

function initHistoryTab(user) {
  const subtitleEl = document.getElementById('historySubtitle');
  if (subtitleEl) {
    if (user.role.toLowerCase() === 'admin') {
      subtitleEl.innerText = 'Pencarian Data Transaksi (Menampilkan Semua Kasir)';
    } else {
      subtitleEl.innerText = `Pencarian Data Transaksi (Hanya Kasir: ${user.username})`;
    }
  }

  // Set default date to today or 2026-09-04 if testing
  const dateInput = document.getElementById('historyDateInput');
  if (dateInput && !dateInput.value) {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    dateInput.value = `${yyyy}-${mm}-${dd}`;
  }
}

function setDateAndSearch(targetDate) {
  const dateInput = document.getElementById('historyDateInput');
  if (targetDate === 'today') {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    dateInput.value = `${yyyy}-${mm}-${dd}`;
  } else {
    dateInput.value = targetDate;
  }
  loadHistory(1);
}

async function loadHistory(page = 1) {
  const user = getSession();
  if (!user) return;

  const dateInput = document.getElementById('historyDateInput');
  const selectedDate = dateInput ? dateInput.value : '';

  if (!selectedDate) {
    showToast('Silakan pilih tanggal transaksi terlebih dahulu.', 'warning');
    return;
  }

  historyCurrentPage = page;
  const tbody = document.getElementById('historyTableBody');
  const paginationEl = document.getElementById('historyPagination');
  
  if (tbody) {
    tbody.innerHTML = `<tr><td colspan="6" class="empty-state">Memuat riwayat transaksi...</td></tr>`;
  }

  const endpoint = `/api/v1/transactions/history?date=${encodeURIComponent(selectedDate)}&role=${encodeURIComponent(user.role)}&userId=${encodeURIComponent(user.idUser)}&page=${page}&pageSize=${historyPageSize}`;
  const res = await apiCall(endpoint);

  if (!res.ok) {
    showToast(res.message || 'Gagal memuat riwayat transaksi.', 'error');
    if (tbody) {
      tbody.innerHTML = `<tr><td colspan="6" class="empty-state">${res.message || 'Gagal memuat data.'}</td></tr>`;
    }
    return;
  }

  const pagedData = res.data;
  if (!pagedData || pagedData.items.length === 0) {
    if (tbody) {
      tbody.innerHTML = `<tr><td colspan="6" class="empty-state">Tidak ada transaksi pada tanggal yang dipilih.</td></tr>`;
    }
    if (paginationEl) paginationEl.innerHTML = '';
    return;
  }

  // Render Table Rows
  if (tbody) {
    tbody.innerHTML = '';
    pagedData.items.forEach(t => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${t.invoiceNo}</td>
        <td>${formatDateTime(t.transactionDate)}</td>
        <td>${t.cashierFullName || t.cashierUsername}</td>
        <td>${t.totalItems}</td>
        <td>${t.paymentMethodName}</td>
        <td>${formatRupiah(t.totalPrice)}</td>
      `;
      tbody.appendChild(tr);
    });
  }

  // Render Pagination (max 20 items per page)
  renderPagination(pagedData.pageNumber, pagedData.totalPages);
}

function renderPagination(currentPage, totalPages) {
  const paginationEl = document.getElementById('historyPagination');
  if (!paginationEl) return;

  paginationEl.innerHTML = '';
  if (totalPages <= 1) return;

  for (let i = 1; i <= totalPages; i++) {
    const btn = document.createElement('button');
    btn.className = `page-btn ${i === currentPage ? 'active' : ''}`;
    btn.innerText = i;
    btn.onclick = () => loadHistory(i);
    paginationEl.appendChild(btn);
  }
}
