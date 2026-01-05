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
    }
}

// =============================================
// Helper Functions
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
    return map[status] || status || '-';
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

function formatNumber(num) {
    if (!num && num !== 0) return '-';
    return new Intl.NumberFormat('vi-VN').format(num);
}

// =============================================
// Render Functions
// =============================================

function renderKPIs() {
    document.getElementById('kpi-farms').textContent = DB.farms.length;
    document.getElementById('kpi-batches').textContent = DB.batches.length;
    document.getElementById('kpi-orders').textContent = DB.orders.length;
    document.getElementById('kpi-alerts').textContent = 0;
}

function renderFarms() {
    const tbody = document.querySelector('#table-farms tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    if (DB.farms.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:#888;">Chưa có trang trại nào</td></tr>';
        return;
    }
    
    DB.farms.forEach(f => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${f.maTrangTrai}</td>
            <td>${f.tenTrangTrai || '-'}</td>
            <td>${f.diaChi || '-'}</td>
            <td>${f.soChungNhan || '-'}</td>
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
    
    if (DB.batches.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:#888;">Chưa có lô nông sản nào</td></tr>';
        return;
    }
    
    DB.batches.forEach(b => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${b.maLo}</td>
            <td>${b.tenTrangTrai || '-'}</td>
            <td>${b.tenSanPham || '-'}</td>
            <td>${formatNumber(b.soLuongHienTai)}</td>
            <td>${b.maQR || '-'}</td>
            <td>${statusDisplay(b.trangThai)}</td>
            <td>
                <button class="btn small" onclick="editBatch(${b.maLo})">Sửa</button>
                <button class="btn small btn-danger" onclick="deleteBatch(${b.maLo})">Xóa</button>
            </td>`;
        tbody.appendChild(tr);
    });
}

function renderOrders() {
    const tbody = document.querySelector('#table-incoming-orders tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    if (DB.orders.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:#888;">Chưa có đơn hàng nào</td></tr>';
        return;
    }
    
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
            actions = `<span style="color:#888;">${statusDisplay(status)}</span>`;
        }
        
        tr.innerHTML = `
            <td>${o.maDonHang}</td>
            <td>${o.loaiDon || '-'}</td>
            <td>${formatNumber(o.tongSoLuong)}</td>
            <td>${o.tenDaiLy || '-'}</td>
            <td>${formatDate(o.ngayDat)}</td>
            <td><span class="status-badge status-${status}">${statusDisplay(status)}</span></td>
            <td>${actions}</td>`;
        tbody.appendChild(tr);
    });
}

function renderKhoNhap() {
    const tbody = document.querySelector('#table-kho-nhap tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    const inStock = DB.batches.filter(b => b.soLuongHienTai > 0);
    
    if (inStock.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#888;">Không có hàng tồn kho</td></tr>';
        return;
    }
    
    inStock.forEach(b => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${b.maLo}</td>
            <td>${b.tenSanPham || '-'}</td>
            <td>${formatNumber(b.soLuongHienTai)}</td>
            <td>${b.tenTrangTrai || '-'}</td>
            <td>${b.maQR || '-'}</td>
            <td><span style="color:green;">Còn hàng</span></td>`;
        tbody.appendChild(tr);
    });
}

function renderKhoXuat() {
    const tbody = document.querySelector('#table-kho-xuat tbody');
    if (!tbody) return;
    tbody.innerHTML = '';
    
    const shipped = DB.orders.filter(o => o.trangThai === 'da_xuat' || o.trangThai === 'da_nhan');
    
    if (shipped.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:#888;">Chưa có đơn xuất nào</td></tr>';
        return;
    }
    
    shipped.forEach(o => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${o.maDonHang}</td>
            <td>${o.loaiDon || '-'}</td>
            <td>${formatNumber(o.tongSoLuong)}</td>
            <td>${o.tenDaiLy || '-'}</td>
            <td>${formatDate(o.ngayGiao)}</td>`;
        tbody.appendChild(tr);
    });
}

function renderReports() {
    const totalProduction = DB.batches.reduce((sum, b) => sum + (parseFloat(b.soLuongBanDau) || 0), 0);
    const currentStock = DB.batches.reduce((sum, b) => sum + (parseFloat(b.soLuongHienTai) || 0), 0);
    const totalShipped = totalProduction - currentStock;
    
    document.getElementById('report-production').textContent = formatNumber(totalProduction) + ' đơn vị';
    document.getElementById('report-shipped').textContent = formatNumber(totalShipped) + ' đơn vị';
    document.getElementById('report-stock').textContent = formatNumber(currentStock) + ' đơn vị';
}

function refreshAll() {
    renderFarms();
    renderBatches();
    renderOrders();
    renderKhoNhap();
    renderKhoXuat();
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
        <label>Số chứng nhận</label><input id="farm-cert" />
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
        <label>Số chứng nhận</label><input id="farm-cert" value="${farm.soChungNhan || ''}" />
        <div style="margin-top:10px">
            <button onclick="saveFarm(${id})" class="btn">Lưu</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
};

window.saveFarm = async function(id = null) {
    const data = {
        maNongDan: parseInt(maNongDan),
        tenTrangTrai: document.getElementById('farm-name').value,
        diaChi: document.getElementById('farm-address').value,
        soChungNhan: document.getElementById('farm-cert').value
    };
    
    if (!data.tenTrangTrai) { alert('Vui lòng nhập tên trang trại'); return; }
    
    try {
        if (id) {
            await apiCall(API.trangTrai.update(id), 'PUT', data);
            alert('Cập nhật thành công!');
        } else {
            await apiCall(API.trangTrai.create, 'POST', data);
            alert('Thêm trang trại thành công!');
        }
        await loadDB();
        refreshAll();
        closeModal();
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
    
    if (DB.farms.length === 0) {
        alert('Vui lòng tạo trang trại trước!');
        return;
    }
    
    openModal(`
        <h3>Đăng ký lô nông sản</h3>
        <label>Trang trại</label>
        <select id="batch-farm">${farmOptions}</select>
        <label>Sản phẩm</label>
        <select id="batch-product">${productOptions || '<option value="">Chưa có sản phẩm</option>'}</select>
        <label>Số lượng ban đầu</label><input id="batch-qty" type="number" />
        <label>Số chứng nhận lô</label><input id="batch-cert" />
        <div style="margin-top:10px">
            <button onclick="saveBatch()" class="btn">Tạo lô</button>
            <button onclick="closeModal()" class="btn" style="background:#ccc;color:#333;">Hủy</button>
        </div>
    `);
}));

window.editBatch = function(id) {
    const batch = DB.batches.find(b => b.maLo === id);
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
        <label>Số lượng hiện tại</label><input id="batch-qty" type="number" value="${batch.soLuongHienTai || ''}" />
        <label>Số chứng nhận lô</label><input id="batch-cert" value="${batch.soChungNhanLo || ''}" />
        <label>Trạng thái</label>
        <select id="batch-status">
            <option value="tai_trang_trai" ${batch.trangThai === 'tai_trang_trai' ? 'selected' : ''}>Tại trang trại</option>
            <option value="dang_van_chuyen" ${batch.trangThai === 'dang_van_chuyen' ? 'selected' : ''}>Đang vận chuyển</option>
            <option value="da_giao" ${batch.trangThai === 'da_giao' ? 'selected' : ''}>Đã giao</option>
            <option value="da_ban" ${batch.trangThai === 'da_ban' ? 'selected' : ''}>Đã bán</option>
        </select>
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
        soLuongBanDau: parseFloat(document.getElementById('batch-qty').value) || 0,
        soChungNhanLo: document.getElementById('batch-cert')?.value || ''
    };
    
    // Nếu đang edit, thêm trạng thái
    if (id) {
        data.soLuongHienTai = data.soLuongBanDau;
        data.trangThai = document.getElementById('batch-status')?.value || 'tai_trang_trai';
    }
    
    if (!data.maTrangTrai || !data.maSanPham || !data.soLuongBanDau) {
        alert('Vui lòng điền đầy đủ thông tin');
        return;
    }
    
    try {
        if (id) {
            await apiCall(API.loNongSan.update(id), 'PUT', data);
            alert('Cập nhật thành công!');
        } else {
            await apiCall(API.loNongSan.create, 'POST', data);
            alert('Tạo lô nông sản thành công!');
        }
        await loadDB();
        refreshAll();
        closeModal();
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
    if (!confirm('Xác nhận đơn hàng này?')) return;
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
    if (!confirm('Xuất đơn hàng này?')) return;
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
