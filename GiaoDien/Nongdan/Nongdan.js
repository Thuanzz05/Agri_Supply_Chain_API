// =============================================
// Nông Dân Dashboard - Kết nối API
// =============================================

let currentUser = null;
let maNongDan = null;

// Database từ API
const DB = {
    farms: [],      // Trang trại
    batches: [],    // Lô nông sản
    orders: [],     // Đơn hàng đại lý
    sanPham: []     // Danh sách sản phẩm
};

// =============================================
// Authentication
// =============================================

function loadCurrentUser() {
    if (typeof AuthHelper !== 'undefined') {
        currentUser = AuthHelper.getCurrentUser();
    } else {
        const stored = sessionStorage.getItem('currentUser');
        currentUser = stored ? JSON.parse(stored) : null;
    }
    
    if (!currentUser) {
        window.location.href = '../Dangnhap/Dangnhap.html';
        return null;
    }
    
    // Lấy maNongDan từ currentUser
    maNongDan = currentUser.maNongDan || currentUser.maNong || currentUser.id;
    return currentUser;
}

// =============================================
// API Helper Functions
// =============================================

async function apiCall(url, method = 'GET', data = null) {
    try {
        const options = {
            method,
            headers: { 'Content-Type': 'application/json' }
        };
        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }
        const response = await fetch(url, options);
        const result = await response.json();
        if (!response.ok) throw new Error(result.message || 'Có lỗi xảy ra');
        return result;
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// =============================================
// Load Data từ API
// =============================================

async function loadDB() {
    if (!maNongDan) {
        console.warn('Không có maNongDan');
        return;
    }
    
    try {
        // Load trang trại theo nông dân
        const farmsRes = await apiCall(API.trangTrai.getByNongDan(maNongDan));
        DB.farms = farmsRes.data || [];
        
        // Load lô nông sản theo nông dân
        const batchesRes = await apiCall(API.loNongSan.getByNongDan(maNongDan));
        DB.batches = batchesRes.data || [];
        
        // Load đơn hàng theo nông dân
        const ordersRes = await apiCall(API.donHangNongDan.getByNongDan(maNongDan));
        DB.orders = ordersRes.data || [];
        
        // Load danh sách sản phẩm
        const sanPhamRes = await apiCall(API.sanPham.getAll);
        DB.sanPham = sanPhamRes.data || [];
        
        console.log('Loaded data from API:', DB);
    } catch (error) {
        console.error('Error loading data:', error);
        alert('Không thể kết nối đến server. Vui lòng kiểm tra Gateway đang chạy.');
    }
}

// =============================================
// Status Helpers
// =============================================

function statusDisplay(status) {
    const map = {
        'cho_xu_ly': 'Chờ xử lý',
        'da_xac_nhan': 'Đã xác nhận',
        'dang_chuan_bi': 'Đang chuẩn bị',
        'da_xuat': 'Đã xuất',
        'da_nhan': 'Đã nhận',
        'da_huy': 'Đã hủy',
        'tai_trang_trai': 'Tại trang trại',
        'dang_van_chuyen': 'Đang vận chuyển',
        'da_giao': 'Đã giao',
        'da_ban': 'Đã bán'
    };
    return map[status] || status || '';
}

function getExpiryStatus(expiry) {
    if (!expiry) return 'ok';
    const now = new Date();
    const d = new Date(expiry);
    const diffDays = Math.ceil((d - now) / (1000*60*60*24));
    if (diffDays < 0) return 'expired';
    if (diffDays <= 7) return 'warning';
    return 'ok';
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

// =============================================
// Render Functions
// =============================================

function renderKPIs() {
    document.getElementById('kpi-farms').textContent = DB.farms.length;
    document.getElementById('kpi-batches').textContent = DB.batches.length;
    document.getElementById('kpi-orders').textContent = DB.orders.length;
    
    // Đếm cảnh báo hạn dùng
    const alerts = DB.batches.filter(b => {
        const status = getExpiryStatus(b.hanSuDung);
        return status === 'warning' || status === 'expired';
    });
    document.getElementById('kpi-alerts').textContent = alerts.length;
}

function renderFarms() {
    const tbody = document.querySelector('#table-farms tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    DB.farms.forEach(f => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${f.maTrangTrai}</td>
            <td>${f.tenTrangTrai}</td>
            <td>${f.diaChi || '-'}</td>
            <td>${f.chungNhan || '-'}</td>
            <td>
                <button class="btn small" onclick="editFarm(${f.maTrangTrai})">Sửa</button>
                <button class="btn small btn-danger" onclick="deleteFarm(${f.maTrangTrai})">Xóa</button>
            </td>`;
        tbody.appendChild(tr);
    });
}

function renderBatches() {
    const tbody = document.querySelector('#table-batches tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    DB.batches.forEach(b => {
        const status = getExpiryStatus(b.hanSuDung);
        const tr = document.createElement('tr');
        tr.className = status === 'expired' ? 'critical' : (status === 'warning' ? 'warning' : '');
        
        tr.innerHTML = `
            <td>${b.maLoNongSan}</td>
            <td>${b.tenTrangTrai || '-'}</td>
            <td>${b.tenSanPham || '-'}</td>
            <td>${b.soLuong} ${b.donVi || ''}</td>
            <td>${formatDate(b.hanSuDung)}</td>
            <td>${statusDisplay(b.trangThai)}</td>
            <td>
                <button class="btn small" onclick="editBatch(${b.maLoNongSan})">Sửa</button>
                <button class="btn small btn-danger" onclick="deleteBatch(${b.maLoNongSan})">Xóa</button>
            </td>`;
        tbody.appendChild(tr);
    });
}

function renderOrders() {
    const tbody = document.querySelector('#table-incoming-orders tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    DB.orders.forEach(o => {
        const tr = document.createElement('tr');
        const status = o.trangThai || 'cho_xu_ly';
        
        let actions = '';
        if (status === 'cho_xu_ly') {
            actions = `
                <button class="btn small" onclick="xacNhanDon(${o.maDonHang})">Xác nhận</button>
                <button class="btn small btn-danger" onclick="huyDon(${o.maDonHang})">Hủy</button>`;
        } else if (status === 'da_xac_nhan') {
            actions = `<button class="btn small" onclick="xuatDon(${o.maDonHang})">Xuất đơn</button>`;
        } else {
            actions = `<span>${statusDisplay(status)}</span>`;
        }
        
        tr.innerHTML = `
            <td>${o.maDonHang}</td>
            <td>${o.loaiDon || '-'}</td>
            <td>${o.tongSoLuong || 0}</td>
            <td>${o.tenDaiLy || '-'}</td>
            <td>${formatDate(o.ngayDat)}</td>
            <td>${statusDisplay(status)}</td>
            <td>${actions}</td>`;
        tbody.appendChild(tr);
    });
}

function renderKhoNhap() {
    const tbody = document.querySelector('#table-kho-nhap tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    const inStock = DB.batches.filter(b => b.trangThai === 'tai_trang_trai' || !b.trangThai);
    inStock.forEach(b => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${b.maLoNongSan}</td>
            <td>${b.tenSanPham || '-'}</td>
            <td>${b.soLuong} ${b.donVi || ''}</td>
            <td>${b.tenTrangTrai || '-'}</td>
            <td>${formatDate(b.hanSuDung)}</td>
            <td>${b.soLuong > 0 ? 'Còn hàng' : 'Hết'}</td>`;
        tbody.appendChild(tr);
    });
}

function renderKhoXuat() {
    const tbody = document.querySelector('#table-kho-xuat tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    const shipped = DB.orders.filter(o => o.trangThai === 'da_xuat' || o.trangThai === 'da_nhan');
    shipped.forEach(o => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${o.maDonHang}</td>
            <td>${o.loaiDon || '-'}</td>
            <td>${o.tongSoLuong || 0}</td>
            <td>${o.tenDaiLy || '-'}</td>
            <td>${formatDate(o.ngayGiao)}</td>`;
        tbody.appendChild(tr);
    });
}

function renderAlerts() {
    const tbody = document.querySelector('#table-alerts tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    DB.batches.forEach(b => {
        const status = getExpiryStatus(b.hanSuDung);
        if (status === 'ok') return;
        
        const now = new Date();
        const d = new Date(b.hanSuDung);
        const diffDays = Math.ceil((d - now) / (1000*60*60*24));
        
        const tr = document.createElement('tr');
        tr.className = status === 'expired' ? 'critical' : 'warning';
        tr.innerHTML = `
            <td>${b.maLoNongSan}</td>
            <td>${b.tenSanPham || '-'}</td>
            <td>${formatDate(b.hanSuDung)}</td>
            <td>${diffDays} ngày</td>
            <td>${status === 'expired' ? 'Đã hết hạn' : 'Sắp hết hạn'}</td>`;
        tbody.appendChild(tr);
    });
}

function renderReports() {
    const totalProduction = DB.batches.reduce((sum, b) => sum + (parseFloat(b.soLuong) || 0), 0);
    const shippedOrders = DB.orders.filter(o => o.trangThai === 'da_xuat' || o.trangThai === 'da_nhan');
    const totalShipped = shippedOrders.reduce((sum, o) => sum + (parseFloat(o.tongSoLuong) || 0), 0);
    
    document.getElementById('report-production').textContent = totalProduction + ' đơn vị';
    document.getElementById('report-shipped').textContent = totalShipped + ' đơn vị';
    document.getElementById('report-stock').textContent = Math.max(0, totalProduction - totalShipped) + ' đơn vị';
}

function refreshAll() {
    renderFarms();
    renderBatches();
    renderOrders();
    renderKhoNhap();
    renderKhoXuat();
    renderAlerts();
    renderKPIs();
    renderReports();
}

// =============================================
// Modal
// =============================================

function openModal(html) {
    document.getElementById('modal-body').innerHTML = html;
    document.getElementById('modal').classList.remove('hidden');
}

function closeModal() {
    document.getElementById('modal').classList.add('hidden');
}

// =============================================
// Trang Trại CRUD
// =============================================

document.getElementById('btn-new-farm')?.addEventListener('click', () => {
    openModal(`
        <h3>Thêm trang trại mới</h3>
        <label>Tên trang trại</label><input id="farm-name" />
        <label>Địa chỉ</label><input id="farm-address" />
        <label>Diện tích (ha)</label><input id="farm-area" type="number" step="0.1" />
        <label>Chứng nhận</label><input id="farm-cert" />
        <div style="margin-top:10px">
            <button onclick="saveFarm()" class="btn">Tạo</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
});

window.editFarm = function(id) {
    const farm = DB.farms.find(f => f.maTrangTrai === id);
    if (!farm) return;
    openModal(`
        <h3>Sửa trang trại</h3>
        <label>Tên trang trại</label><input id="farm-name" value="${farm.tenTrangTrai || ''}" />
        <label>Địa chỉ</label><input id="farm-address" value="${farm.diaChi || ''}" />
        <label>Diện tích (ha)</label><input id="farm-area" type="number" step="0.1" value="${farm.dienTich || ''}" />
        <label>Chứng nhận</label><input id="farm-cert" value="${farm.chungNhan || ''}" />
        <div style="margin-top:10px">
            <button onclick="saveFarm(${id})" class="btn">Lưu</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
};

window.saveFarm = async function(id = null) {
    const data = {
        maNongDan: maNongDan,
        tenTrangTrai: document.getElementById('farm-name').value,
        diaChi: document.getElementById('farm-address').value,
        dienTich: parseFloat(document.getElementById('farm-area').value) || 0,
        chungNhan: document.getElementById('farm-cert').value
    };
    
    if (!data.tenTrangTrai) { alert('Vui lòng nhập tên trang trại'); return; }
    
    try {
        if (id) {
            await apiCall(API.trangTrai.update(id), 'PUT', data);
        } else {
            await apiCall(API.trangTrai.create, 'POST', data);
        }
        await loadDB();
        refreshAll();
        closeModal();
        alert(id ? 'Cập nhật thành công!' : 'Thêm trang trại thành công!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

window.deleteFarm = async function(id) {
    if (!confirm('Xác nhận xóa trang trại này?')) return;
    try {
        await apiCall(API.trangTrai.delete(id), 'DELETE');
        await loadDB();
        refreshAll();
        alert('Xóa thành công!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

// =============================================
// Lô Nông Sản CRUD
// =============================================

document.querySelectorAll('#btn-new-batch').forEach(btn => btn.addEventListener('click', () => {
    const farmOptions = DB.farms.map(f => `<option value="${f.maTrangTrai}">${f.tenTrangTrai}</option>`).join('');
    const productOptions = DB.sanPham.map(p => `<option value="${p.maSanPham}">${p.tenSanPham}</option>`).join('');
    
    openModal(`
        <h3>Đăng ký lô nông sản</h3>
        <label>Trang trại</label>
        <select id="batch-farm">${farmOptions || '<option value="">Chưa có trang trại</option>'}</select>
        <label>Sản phẩm</label>
        <select id="batch-product">${productOptions || '<option value="">Chưa có sản phẩm</option>'}</select>
        <label>Số lượng</label><input id="batch-qty" type="number" />
        <label>Đơn vị</label><input id="batch-unit" value="kg" />
        <label>Ngày thu hoạch</label><input id="batch-harvest" type="date" />
        <label>Hạn sử dụng</label><input id="batch-expiry" type="date" />
        <div style="margin-top:10px">
            <button onclick="saveBatch()" class="btn">Tạo lô</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
}));

window.editBatch = function(id) {
    const batch = DB.batches.find(b => b.maLoNongSan === id);
    if (!batch) return;
    
    const farmOptions = DB.farms.map(f => 
        `<option value="${f.maTrangTrai}" ${f.maTrangTrai === batch.maTrangTrai ? 'selected' : ''}>${f.tenTrangTrai}</option>`
    ).join('');
    const productOptions = DB.sanPham.map(p => 
        `<option value="${p.maSanPham}" ${p.maSanPham === batch.maSanPham ? 'selected' : ''}>${p.tenSanPham}</option>`
    ).join('');
    
    openModal(`
        <h3>Sửa lô nông sản</h3>
        <label>Trang trại</label>
        <select id="batch-farm">${farmOptions}</select>
        <label>Sản phẩm</label>
        <select id="batch-product">${productOptions}</select>
        <label>Số lượng</label><input id="batch-qty" type="number" value="${batch.soLuong || ''}" />
        <label>Đơn vị</label><input id="batch-unit" value="${batch.donVi || 'kg'}" />
        <label>Ngày thu hoạch</label><input id="batch-harvest" type="date" value="${batch.ngayThuHoach ? batch.ngayThuHoach.split('T')[0] : ''}" />
        <label>Hạn sử dụng</label><input id="batch-expiry" type="date" value="${batch.hanSuDung ? batch.hanSuDung.split('T')[0] : ''}" />
        <div style="margin-top:10px">
            <button onclick="saveBatch(${id})" class="btn">Lưu</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
};

window.saveBatch = async function(id = null) {
    const data = {
        maTrangTrai: parseInt(document.getElementById('batch-farm').value),
        maSanPham: parseInt(document.getElementById('batch-product').value),
        soLuong: parseFloat(document.getElementById('batch-qty').value) || 0,
        donVi: document.getElementById('batch-unit').value || 'kg',
        ngayThuHoach: document.getElementById('batch-harvest').value || null,
        hanSuDung: document.getElementById('batch-expiry').value || null,
        trangThai: 'tai_trang_trai'
    };
    
    if (!data.maTrangTrai || !data.maSanPham || !data.soLuong) {
        alert('Vui lòng chọn trang trại, sản phẩm và nhập số lượng');
        return;
    }
    
    try {
        if (id) {
            await apiCall(API.loNongSan.update(id), 'PUT', data);
        } else {
            await apiCall(API.loNongSan.create, 'POST', data);
        }
        await loadDB();
        refreshAll();
        closeModal();
        alert(id ? 'Cập nhật thành công!' : 'Tạo lô nông sản thành công!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

window.deleteBatch = async function(id) {
    if (!confirm('Xác nhận xóa lô nông sản này?')) return;
    try {
        await apiCall(API.loNongSan.delete(id), 'DELETE');
        await loadDB();
        refreshAll();
        alert('Xóa thành công!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

// =============================================
// Đơn Hàng Actions
// =============================================

window.xacNhanDon = async function(id) {
    try {
        await apiCall(API.donHangNongDan.xacNhan(id), 'PUT');
        await loadDB();
        refreshAll();
        alert('Đã xác nhận đơn hàng!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

window.xuatDon = async function(id) {
    try {
        await apiCall(API.donHangNongDan.xuatDon(id), 'PUT');
        await loadDB();
        refreshAll();
        alert('Đã xuất đơn hàng!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

window.huyDon = async function(id) {
    if (!confirm('Xác nhận hủy đơn hàng này?')) return;
    try {
        await apiCall(API.donHangNongDan.huyDon(id), 'PUT');
        await loadDB();
        refreshAll();
        alert('Đã hủy đơn hàng!');
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
};

// =============================================
// Navigation
// =============================================

document.querySelectorAll('.menu-link[data-section]').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        document.querySelectorAll('.menu-link[data-section]').forEach(l => l.classList.remove('active'));
        document.querySelectorAll('.page').forEach(p => p.classList.remove('active-page'));
        link.classList.add('active');
        document.getElementById(link.dataset.section)?.classList.add('active-page');
    });
});

// =============================================
// Initialize
// =============================================

window.addEventListener('DOMContentLoaded', async () => {
    loadCurrentUser();
    
    // Hiển thị tên user
    const userDisplay = document.getElementById('current-user');
    if (userDisplay && currentUser) {
        userDisplay.textContent = currentUser.fullName || currentUser.hoTen || 'Nông dân';
    }
    
    // Load data từ API
    await loadDB();
    refreshAll();
});

// Event listener cho modal close
document.addEventListener('click', (e) => {
    if (e.target.matches('.modal-close')) closeModal();
});
