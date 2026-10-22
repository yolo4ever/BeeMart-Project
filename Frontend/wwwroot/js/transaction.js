// Transaction State Management
let currentCart = [];
let currentSelectedProduct = null;
let currentInvoiceNo = '';

// Initialize New Transaction Page
async function initTransactionTab(user) {
  startRealTimeClock(user);
  await loadNextInvoice(user.idUser);
  await loadPaymentMethods();
  renderCart();

  // Search input Enter listener
  const searchInput = document.getElementById('productSearchInput');
  if (searchInput) {
    searchInput.addEventListener('keydown', async (e) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        await searchProducts();
      }
    });
  }
}

// Real-time Clock in Header
function startRealTimeClock(user) {
  const clockEl = document.getElementById('headerClockSession');
  function update() {
    const now = new Date();
    // Format: 4/9/2026, 10:32:46 | Kasir: steven (or Role: Admin | Login: admin)
    const datePart = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()}`;
    const timePart = now.toTimeString().split(' ')[0];
    
    let roleText = '';
    if (user.role.toLowerCase() === 'cashier') {
      roleText = `Kasir: ${user.username}`;
    } else {
      roleText = `Role: ${user.role} | Login: ${user.username}`;
    }

    if (clockEl) {
      clockEl.innerText = `${datePart}, ${timePart} | ${roleText}`;
    }
  }
  update();
  setInterval(update, 1000);
}

// Load Next Invoice Number
async function loadNextInvoice(userId) {
  const res = await apiCall(`/api/v1/transactions/next-invoice?userId=${encodeURIComponent(userId)}`);
  if (res.ok && res.data) {
    currentInvoiceNo = res.data.invoiceNo;
    const invInput = document.getElementById('invoiceNoInput');
    if (invInput) {
      invInput.value = currentInvoiceNo;
    }
  }
}

// Fetch Payment Methods Dynamically
async function loadPaymentMethods() {
  const res = await apiCall('/api/v1/paymentmethods');
  const select = document.getElementById('paymentMethodSelect');
  if (select && res.ok && res.data) {
    select.innerHTML = '';
    res.data.forEach(m => {
      const opt = document.createElement('option');
      opt.value = m.idPaymentMethod;
      opt.innerText = m.methodName;
      select.appendChild(opt);
    });
  }
}

// Search Products (max 10 results)
async function searchProducts() {
  const keyword = document.getElementById('productSearchInput').value.trim();
  const resultsContainer = document.getElementById('searchResultsContainer');
  const resultsBody = document.getElementById('searchResultsBody');

  if (!keyword) {
    resultsContainer.style.display = 'none';
    resultsBody.innerHTML = '';
    return;
  }

  const res = await apiCall(`/api/v1/products/search?keyword=${encodeURIComponent(keyword)}`);
  if (res.ok && res.data) {
    if (res.data.length === 0) {
      showToast('Tidak ada barang yang ditemukan.', 'info');
      resultsContainer.style.display = 'none';
      return;
    }

    resultsBody.innerHTML = '';
    res.data.forEach(p => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${p.name}</td>
        <td>${p.weight} ${p.weight >= 10 ? 'g' : 'Kg'}</td>
        <td>${p.stock}</td>
        <td>${formatRupiah(p.price)}</td>
        <td>${p.discount}%</td>
        <td><button class="btn-select" onclick='selectProduct(${JSON.stringify(p)})'>Pilih</button></td>
      `;
      resultsBody.appendChild(tr);
    });

    resultsContainer.style.display = 'block';
  } else {
    showToast(res.message || 'Gagal mencari barang.', 'error');
  }
}

// Select Product to open Dialog Box
function selectProduct(product) {
  currentSelectedProduct = product;
  const dialogBox = document.getElementById('selectionDialogBox');
  document.getElementById('dialogProductName').innerText = product.name;
  document.getElementById('dialogProductPrice').innerText = formatRupiah(product.price);
  document.getElementById('dialogProductStock').innerText = product.stock;
  
  const qtyInput = document.getElementById('dialogQtyInput');
  qtyInput.value = 1;
  qtyInput.max = product.stock;
  dialogBox.style.display = 'block';
  qtyInput.focus();
}

// Add Selected Product to Cart
function addSelectedProductToCart() {
  if (!currentSelectedProduct) return;

  const qtyInput = document.getElementById('dialogQtyInput');
  const qty = parseInt(qtyInput.value, 10);

  if (isNaN(qty) || qty <= 0) {
    showToast('Jumlah barang harus lebih dari 0.', 'warning');
    return;
  }

  // Stock validation
  if (qty > currentSelectedProduct.stock) {
    showToast('Jumlah barang yang dimasukkan melebihi sisa stok.', 'error');
    return;
  }

  // Check if item already in cart
  const existingIndex = currentCart.findIndex(item => item.idProduct === currentSelectedProduct.idProduct);
  if (existingIndex > -1) {
    const totalQty = currentCart[existingIndex].qty + qty;
    if (totalQty > currentSelectedProduct.stock) {
      showToast('Jumlah barang yang dimasukkan melebihi sisa stok.', 'error');
      return;
    }
    currentCart[existingIndex].qty = totalQty;
    currentCart[existingIndex].subtotal = calculateSubtotal(currentSelectedProduct.price, currentSelectedProduct.discount, totalQty);
  } else {
    currentCart.push({
      idProduct: currentSelectedProduct.idProduct,
      name: currentSelectedProduct.name,
      price: currentSelectedProduct.price,
      discount: currentSelectedProduct.discount,
      qty: qty,
      stock: currentSelectedProduct.stock,
      subtotal: calculateSubtotal(currentSelectedProduct.price, currentSelectedProduct.discount, qty)
    });
  }

  // Hide dialog and reset selection
  document.getElementById('selectionDialogBox').style.display = 'none';
  currentSelectedProduct = null;

  renderCart();
  showToast('Barang berhasil ditambahkan ke keranjang.', 'success');
}

// Calculate item subtotal
function calculateSubtotal(price, discount, qty) {
  const discountedPrice = price - (price * (discount / 100));
  return discountedPrice * qty;
}

// Remove item from cart
function removeFromCart(index) {
  currentCart.splice(index, 1);
  renderCart();
}

// Render Cart Table & Total
function renderCart() {
  const tbody = document.getElementById('cartTableBody');
  const totalDisplay = document.getElementById('totalDisplay');

  if (!tbody) return;

  tbody.innerHTML = '';
  let total = 0;

  if (currentCart.length === 0) {
    tbody.innerHTML = `<tr><td colspan="6" class="empty-state">Keranjang belanja kosong. Silakan cari dan pilih barang.</td></tr>`;
    totalDisplay.innerText = 'Total: Rp 0';
    return;
  }

  currentCart.forEach((item, index) => {
    total += item.subtotal;
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${item.name}</td>
      <td>${formatRupiah(item.price)}</td>
      <td>${item.discount}%</td>
      <td>${item.qty}</td>
      <td>${formatRupiah(item.subtotal)}</td>
      <td><button class="btn-delete" onclick="removeFromCart(${index})">Hapus</button></td>
    `;
    tbody.appendChild(tr);
  });

  totalDisplay.innerText = `Total: ${formatRupiah(total)}`;
}

// Submit Transaction
async function submitTransaction(user) {
  if (currentCart.length === 0) {
    showToast('Keranjang belanja masih kosong! Silakan pilih barang terlebih dahulu.', 'warning');
    return;
  }

  const paymentSelect = document.getElementById('paymentMethodSelect');
  const paymentMethodId = parseInt(paymentSelect.value, 10);

  const payload = {
    invoiceNo: currentInvoiceNo,
    idUser: user.idUser,
    idPaymentMethod: paymentMethodId,
    items: currentCart.map(item => ({
      idProduct: item.idProduct,
      qty: item.qty
    }))
  };

  const btn = document.getElementById('btnSubmitTransaction');
  btn.disabled = true;
  btn.innerText = 'Menyimpan Transaksi...';

  const res = await apiCall('/api/v1/transactions', 'POST', payload);
  btn.disabled = false;
  btn.innerText = 'Submit Transaksi';

  if (!res.ok) {
    showToast(res.message || 'Gagal menyimpan transaksi.', 'error');
    return;
  }

  showToast('Transaksi berhasil disimpan!', 'success');
  alert(`Transaksi berhasil!\nInvoice: ${res.data.invoiceNo}\nTotal: ${formatRupiah(res.data.totalPrice)}`);

  // Reset page to initial state & update to new invoice number
  currentCart = [];
  renderCart();
  document.getElementById('productSearchInput').value = '';
  document.getElementById('searchResultsContainer').style.display = 'none';
  document.getElementById('searchResultsBody').innerHTML = '';
  document.getElementById('selectionDialogBox').style.display = 'none';

  if (res.data && res.data.nextInvoiceNo) {
    currentInvoiceNo = res.data.nextInvoiceNo;
    document.getElementById('invoiceNoInput').value = currentInvoiceNo;
  } else {
    await loadNextInvoice(user.idUser);
  }
}
