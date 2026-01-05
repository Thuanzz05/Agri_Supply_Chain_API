// ========== DAILY.JS - KẾT NỐI API ==========
var app = angular.module('DaiLyApp', []);

app.controller("DaiLyCtrl", function ($scope, $http) {
    // ========== BIẾN DỮ LIỆU ==========
    $scope.currentUser = null;
    $scope.listKho = [];
    $scope.listKiemDinh = [];
    $scope.listDonHangDaiLy = [];
    $scope.listDonHangSieuThi = [];
    $scope.listNongDan = [];
    $scope.listLoNongSan = [];

    // ========== KHỞI TẠO ==========
    $scope.init = function () {
        // Lấy thông tin user đã đăng nhập
        var stored = localStorage.getItem('currentUser');
        if (!stored) {
            window.location.href = '../Dangnhap/Dangnhap.html';
            return;
        }
        $scope.currentUser = JSON.parse(stored);
        
        // Hiển thị thông tin user
        document.getElementById('current-user').textContent = $scope.currentUser.TenDangNhap || '';
        document.getElementById('current-role').textContent = 'Đại lý';

        // Load dữ liệu
        $scope.loadKho();
        $scope.loadKiemDinh();
        $scope.loadDonHangDaiLy();
        $scope.loadDonHangSieuThi();
        $scope.loadNongDan();
        $scope.loadLoNongSan();
    };

    // ========== LOAD KHO ==========
    $scope.loadKho = function () {
        $http({
            method: 'GET',
            url: API.kho.getAll
        }).then(function (response) {
            $scope.listKho = response.data;
            $scope.updateKPIs();
        }).catch(function (error) {
            console.error('Lỗi load kho:', error);
        });
    };

    // ========== LOAD KIỂM ĐỊNH ==========
    $scope.loadKiemDinh = function () {
        $http({
            method: 'GET',
            url: API.kiemdinh.getAll
        }).then(function (response) {
            $scope.listKiemDinh = response.data;
            $scope.updateKPIs();
        }).catch(function (error) {
            console.error('Lỗi load kiểm định:', error);
        });
    };

    // ========== LOAD ĐƠN HÀNG ĐẠI LÝ ==========
    $scope.loadDonHangDaiLy = function () {
        $http({
            method: 'GET',
            url: API.donHangDaiLy.getAll
        }).then(function (response) {
            $scope.listDonHangDaiLy = response.data;
            $scope.updateKPIs();
        }).catch(function (error) {
            console.error('Lỗi load đơn hàng đại lý:', error);
        });
    };

    // ========== LOAD ĐƠN HÀNG SIÊU THỊ ==========
    $scope.loadDonHangSieuThi = function () {
        $http({
            method: 'GET',
            url: API.donHangSieuThiDaily.getAll
        }).then(function (response) {
            $scope.listDonHangSieuThi = response.data;
        }).catch(function (error) {
            console.error('Lỗi load đơn hàng siêu thị:', error);
        });
    };

    // ========== LOAD NÔNG DÂN ==========
    $scope.loadNongDan = function () {
        $http({
            method: 'GET',
            url: API.nongdan.getAll
        }).then(function (response) {
            $scope.listNongDan = response.data;
        }).catch(function (error) {
            console.error('Lỗi load nông dân:', error);
        });
    };

    // ========== LOAD LÔ NÔNG SẢN ==========
    $scope.loadLoNongSan = function () {
        $http({
            method: 'GET',
            url: API.loNongSan.getAll
        }).then(function (response) {
            $scope.listLoNongSan = response.data;
        }).catch(function (error) {
            console.error('Lỗi load lô nông sản:', error);
        });
    };

    // ========== CẬP NHẬT KPIs ==========
    $scope.updateKPIs = function () {
        document.getElementById('kpi-orders').textContent = $scope.listDonHangDaiLy.length || 0;
        document.getElementById('kpi-inventory').textContent = $scope.listKho.length || 0;
        
        // Đếm cảnh báo chất lượng (kiểm định không đạt)
        var canhBao = $scope.listKiemDinh.filter(function(k) {
            return k.KetQua && k.KetQua.toLowerCase().includes('khong');
        }).length;
        document.getElementById('kpi-quality').textContent = canhBao;
    };

    // ========== THÊM KHO ==========
    $scope.newKho = {};
    $scope.themKho = function () {
        if (!$scope.newKho.TenKho) {
            alert('Vui lòng nhập tên kho');
            return;
        }
        
        var data = {
            LoaiKho: 'daily',
            MaDaiLy: $scope.currentUser.MaTaiKhoan,
            TenKho: $scope.newKho.TenKho,
            DiaChi: $scope.newKho.DiaChi || '',
            TrangThai: 'hoat_dong'
        };

        $http({
            method: 'POST',
            url: API.kho.create,
            data: data,
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Thêm kho thành công!');
            $scope.newKho = {};
            $scope.loadKho();
            closeModal();
        }).catch(function (error) {
            console.error('Lỗi thêm kho:', error);
            alert('Lỗi thêm kho: ' + (error.data || 'Không xác định'));
        });
    };

    // ========== SỬA KHO ==========
    $scope.editKho = {};
    $scope.openEditKho = function (kho) {
        $scope.editKho = angular.copy(kho);
        openModalWithTemplate('warehouse-template');
    };

    $scope.capNhatKho = function () {
        var data = {
            TenKho: $scope.editKho.TenKho,
            DiaChi: $scope.editKho.DiaChi,
            TrangThai: $scope.editKho.TrangThai || 'hoat_dong'
        };

        $http({
            method: 'PUT',
            url: API.kho.update($scope.editKho.MaKho),
            data: data,
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Cập nhật kho thành công!');
            $scope.loadKho();
            closeModal();
        }).catch(function (error) {
            console.error('Lỗi cập nhật kho:', error);
            alert('Lỗi cập nhật kho');
        });
    };

    // ========== XÓA KHO ==========
    $scope.xoaKho = function (maKho) {
        if (!confirm('Xác nhận xóa kho này?')) return;

        $http({
            method: 'DELETE',
            url: API.kho.delete(maKho)
        }).then(function (response) {
            alert('Xóa kho thành công!');
            $scope.loadKho();
        }).catch(function (error) {
            console.error('Lỗi xóa kho:', error);
            alert('Lỗi xóa kho');
        });
    };

    // ========== THÊM KIỂM ĐỊNH ==========
    $scope.newKiemDinh = {};
    $scope.themKiemDinh = function () {
        if (!$scope.newKiemDinh.MaLo) {
            alert('Vui lòng nhập mã lô');
            return;
        }

        var data = {
            MaLo: parseInt($scope.newKiemDinh.MaLo),
            NguoiKiemDinh: $scope.newKiemDinh.NguoiKiemDinh || '',
            MaDaiLy: $scope.currentUser.MaTaiKhoan,
            KetQua: $scope.newKiemDinh.KetQua || 'dat',
            TrangThai: 'hoan_thanh',
            GhiChu: $scope.newKiemDinh.GhiChu || ''
        };

        $http({
            method: 'POST',
            url: API.kiemdinh.create,
            data: data,
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Thêm kiểm định thành công!');
            $scope.newKiemDinh = {};
            $scope.loadKiemDinh();
            closeModal();
        }).catch(function (error) {
            console.error('Lỗi thêm kiểm định:', error);
            alert('Lỗi thêm kiểm định: ' + (error.data || 'Không xác định'));
        });
    };

    // ========== XÓA KIỂM ĐỊNH ==========
    $scope.xoaKiemDinh = function (maKiemDinh) {
        if (!confirm('Xác nhận xóa kiểm định này?')) return;

        $http({
            method: 'DELETE',
            url: API.kiemdinh.delete(maKiemDinh)
        }).then(function (response) {
            alert('Xóa kiểm định thành công!');
            $scope.loadKiemDinh();
        }).catch(function (error) {
            console.error('Lỗi xóa kiểm định:', error);
            alert('Lỗi xóa kiểm định');
        });
    };

    // ========== THÊM ĐƠN HÀNG ĐẠI LÝ ==========
    $scope.newDonHang = {};
    $scope.themDonHangDaiLy = function () {
        if (!$scope.newDonHang.MaNongDan || !$scope.newDonHang.MaLo) {
            alert('Vui lòng chọn nông dân và mã lô');
            return;
        }

        var data = {
            MaDaiLy: $scope.currentUser.MaTaiKhoan,
            MaNongDan: parseInt($scope.newDonHang.MaNongDan),
            MaLo: parseInt($scope.newDonHang.MaLo),
            SoLuong: parseFloat($scope.newDonHang.SoLuong) || 0,
            DonGia: parseFloat($scope.newDonHang.DonGia) || 0,
            GhiChu: $scope.newDonHang.GhiChu || ''
        };

        $http({
            method: 'POST',
            url: API.donHangDaiLy.create,
            data: data,
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Tạo đơn hàng thành công!');
            $scope.newDonHang = {};
            $scope.loadDonHangDaiLy();
            closeModal();
        }).catch(function (error) {
            console.error('Lỗi tạo đơn hàng:', error);
            alert('Lỗi tạo đơn hàng: ' + (error.data || 'Không xác định'));
        });
    };

    // ========== CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG ==========
    $scope.capNhatTrangThai = function (maDonHang, trangThai) {
        $http({
            method: 'PUT',
            url: API.donHangDaiLy.updateTrangThai(maDonHang),
            data: { TrangThai: trangThai },
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Cập nhật trạng thái thành công!');
            $scope.loadDonHangDaiLy();
        }).catch(function (error) {
            console.error('Lỗi cập nhật trạng thái:', error);
            alert('Lỗi cập nhật trạng thái');
        });
    };

    // ========== XÓA ĐƠN HÀNG ==========
    $scope.xoaDonHang = function (maDonHang) {
        if (!confirm('Xác nhận xóa đơn hàng này?')) return;

        $http({
            method: 'DELETE',
            url: API.donHangDaiLy.delete(maDonHang)
        }).then(function (response) {
            alert('Xóa đơn hàng thành công!');
            $scope.loadDonHangDaiLy();
        }).catch(function (error) {
            console.error('Lỗi xóa đơn hàng:', error);
            alert('Lỗi xóa đơn hàng');
        });
    };

    // ========== CẬP NHẬT TRẠNG THÁI ĐƠN SIÊU THỊ ==========
    $scope.capNhatTrangThaiSieuThi = function (maDonHang, trangThai) {
        $http({
            method: 'PUT',
            url: API.donHangSieuThiDaily.updateTrangThai(maDonHang),
            data: { TrangThai: trangThai },
            headers: { 'Content-Type': 'application/json' }
        }).then(function (response) {
            alert('Cập nhật trạng thái thành công!');
            $scope.loadDonHangSieuThi();
        }).catch(function (error) {
            console.error('Lỗi cập nhật trạng thái:', error);
            alert('Lỗi cập nhật trạng thái');
        });
    };

    // ========== ĐĂNG XUẤT ==========
    $scope.logout = function () {
        localStorage.removeItem('currentUser');
        sessionStorage.removeItem('currentUser');
        window.location.href = '../Dangnhap/Dangnhap.html';
    };

    // ========== RENDER BÁO CÁO ==========
    $scope.renderReports = function () {
        var totalOrders = $scope.listDonHangDaiLy.length;
        var totalShipped = $scope.listDonHangDaiLy.filter(function(d) {
            return d.TrangThai && d.TrangThai.toLowerCase().includes('hoan_thanh');
        }).length;
        var totalStock = $scope.listKho.length;
        var totalChecks = $scope.listKiemDinh.length;
        var passed = $scope.listKiemDinh.filter(function(k) {
            return k.KetQua && k.KetQua.toLowerCase().includes('dat');
        }).length;
        var passPercent = totalChecks ? Math.round((passed / totalChecks) * 100) : 0;

        var elOrders = document.getElementById('report-orders');
        var elShipped = document.getElementById('report-shipped');
        var elStock = document.getElementById('report-stock');
        var elQuality = document.getElementById('report-quality');

        if (elOrders) elOrders.textContent = totalOrders + ' đơn';
        if (elShipped) elShipped.textContent = totalShipped + ' đơn';
        if (elStock) elStock.textContent = totalStock + ' kho';
        if (elQuality) elQuality.textContent = passed + '/' + totalChecks + ' (' + passPercent + '%)';
    };

    // Khởi tạo
    $scope.init();
});

// ========== MODAL FUNCTIONS ==========
var modal = document.getElementById('modal');
var modalBody = document.getElementById('modal-body');

function openModalWithTemplate(templateId) {
    var tpl = document.getElementById(templateId);
    if (!tpl) return;
    modalBody.innerHTML = '';
    modalBody.appendChild(tpl.content.cloneNode(true));
    modal.classList.remove('hidden');
}

function closeModal() {
    modal.classList.add('hidden');
    modalBody.innerHTML = '';
}

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    var modalCloseBtn = document.querySelector('.modal-close');
    if (modalCloseBtn) {
        modalCloseBtn.addEventListener('click', closeModal);
    }
    
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) closeModal();
        });
    }

    // Menu navigation
    document.querySelectorAll('.menu-link').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            document.querySelectorAll('.menu-link').forEach(function(b) {
                b.classList.remove('active');
            });
            document.querySelectorAll('.page').forEach(function(p) {
                p.classList.remove('active-page');
            });
            
            btn.classList.add('active');
            var sectionId = btn.dataset.section;
            if (sectionId) {
                document.getElementById(sectionId).classList.add('active-page');
            }
        });
    });

    // Tab toggle for orders
    var tabImport = document.getElementById('tab-orders-import');
    var tabRetail = document.getElementById('tab-orders-retail');
    var panelImport = document.getElementById('orders-import');
    var panelRetail = document.getElementById('orders-retail');

    if (tabImport && tabRetail) {
        tabImport.addEventListener('click', function() {
            tabImport.classList.add('active');
            tabRetail.classList.remove('active');
            panelImport.style.display = 'block';
            panelRetail.style.display = 'none';
        });

        tabRetail.addEventListener('click', function() {
            tabRetail.classList.add('active');
            tabImport.classList.remove('active');
            panelRetail.style.display = 'block';
            panelImport.style.display = 'none';
        });
    }
});
