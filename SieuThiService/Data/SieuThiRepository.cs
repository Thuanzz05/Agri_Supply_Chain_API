using Microsoft.EntityFrameworkCore;
using SieuThiService.Models.DTOs;
using SieuThiService.Models.Entities;

namespace SieuThiService.Data
{
    public class SieuThiRepository : ISieuThiRepository
    {
        private readonly BtlHdv1Context _context;

        public SieuThiRepository(BtlHdv1Context context)
        {
            _context = context;
        }

        public bool CreateDonHangOnly(CreateDonHangRequest request)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = _context.SieuThis
                    .FirstOrDefault(st => st.MaSieuThi == request.MaSieuThi);
                
                if (sieuThi == null)
                {
                    return false;
                }

                // Tạo đơn hàng chính (chưa có chi tiết)
                var donHang = new DonHang
                {
                    LoaiDon = "sieu_thi",
                    NgayDat = DateTime.Now,
                    NgayGiao = request.NgayGiao,
                    TrangThai = "chua_nhan",
                    TongSoLuong = 0,
                    TongGiaTri = 0,
                    GhiChu = request.GhiChu
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                // Tạo đơn hàng siêu thị
                var donHangSieuThi = new DonHangSieuThi
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = request.MaSieuThi,
                    MaDaiLy = request.MaDaiLy
                };

                _context.DonHangSieuThis.Add(donHangSieuThi);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddChiTietDonHang(CreateChiTietDonHangRequest request)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = _context.DonHangs
                    .FirstOrDefault(dh => dh.MaDonHang == request.MaDonHang);
                
                if (donHang == null)
                {
                    return false;
                }

                // Tính thành tiền
                decimal thanhTien = request.SoLuong * (request.DonGia ?? 0);

                // Tạo chi tiết đơn hàng
                var chiTietDonHang = new ChiTietDonHang
                {
                    MaDonHang = request.MaDonHang,
                    MaLo = request.MaLo,
                    SoLuong = request.SoLuong,
                    DonGia = request.DonGia,
                    ThanhTien = thanhTien
                };

                _context.ChiTietDonHangs.Add(chiTietDonHang);

                // Cập nhật tổng số lượng và tổng giá trị của đơn hàng
                var tongSoLuong = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .Sum(ct => ct.SoLuong);
                
                var tongGiaTri = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .Sum(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong + request.SoLuong;
                donHang.TongGiaTri = tongGiaTri + thanhTien;

                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NhanHang(int maDonHang, NhanHangRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = _context.DonHangs
                    .Include(dh => dh.DonHangSieuThi)
                    .Include(dh => dh.ChiTietDonHangs)
                    .FirstOrDefault(dh => dh.MaDonHang == maDonHang);
                
                if (donHang == null)
                {
                    return false;
                }

                // Kiểm tra trạng thái đơn hàng có thể nhận không
                if (donHang.TrangThai == "da_huy" || donHang.TrangThai == "da_nhan" || donHang.TrangThai == "da_giao")
                {
                    return false;
                }

                // Kiểm tra kho được chọn có tồn tại và thuộc về siêu thị không
                var khoNhan = _context.Khos
                    .FirstOrDefault(k => k.MaKho == request.MaKho && k.MaSieuThi == donHang.DonHangSieuThi!.MaSieuThi);

                if (khoNhan == null || khoNhan.TrangThai != "hoat_dong")
                {
                    return false;
                }

                // Cập nhật tồn kho cho từng chi tiết đơn hàng
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    // Kiểm tra xem lô hàng đã có trong kho chưa
                    var tonKho = _context.TonKhos
                        .FirstOrDefault(tk => tk.MaKho == request.MaKho && tk.MaLo == chiTiet.MaLo);

                    if (tonKho != null)
                    {
                        // Nếu đã có, cộng thêm số lượng
                        tonKho.SoLuong += chiTiet.SoLuong;
                        tonKho.CapNhatCuoi = DateTime.Now;
                    }
                    else
                    {
                        // Nếu chưa có, tạo mới
                        var tonKhoMoi = new TonKho
                        {
                            MaKho = request.MaKho,
                            MaLo = chiTiet.MaLo,
                            SoLuong = chiTiet.SoLuong,
                            CapNhatCuoi = DateTime.Now
                        };
                        _context.TonKhos.Add(tonKhoMoi);
                    }
                }

                // Cập nhật trạng thái đơn hàng thành "da_nhan"
                donHang.TrangThai = "da_nhan";
                
                // Cập nhật ghi chú nếu có
                if (!string.IsNullOrEmpty(request.GhiChuNhan))
                {
                    donHang.GhiChu = string.IsNullOrEmpty(donHang.GhiChu) 
                        ? $"Ghi chú nhận hàng: {request.GhiChuNhan}"
                        : $"{donHang.GhiChu}. Ghi chú nhận hàng: {request.GhiChuNhan}";
                }

                _context.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool UpdateChiTietDonHang(UpdateChiTietDonHangRequest request)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = _context.DonHangs
                    .FirstOrDefault(dh => dh.MaDonHang == request.MaDonHang);
                
                if (donHang == null)
                {
                    return false;
                }

                // Kiểm tra trạng thái đơn hàng có thể sửa không
                if (donHang.TrangThai == "da_huy" || donHang.TrangThai == "da_giao" || donHang.TrangThai == "dang_giao")
                {
                    return false;
                }

                // Tìm chi tiết đơn hàng cần sửa
                var chiTietDonHang = _context.ChiTietDonHangs
                    .FirstOrDefault(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo == request.MaLo);

                if (chiTietDonHang == null)
                {
                    return false;
                }

                // Cập nhật giá trị mới
                chiTietDonHang.SoLuong = request.SoLuong;
                chiTietDonHang.DonGia = request.DonGia;
                chiTietDonHang.ThanhTien = request.SoLuong * (request.DonGia ?? 0);

                // Cập nhật tổng số lượng và tổng giá trị của đơn hàng
                var tongSoLuong = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .Sum(ct => ct.SoLuong);
                
                var tongGiaTri = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .Sum(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong;
                donHang.TongGiaTri = tongGiaTri;

                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteChiTietDonHang(DeleteChiTietDonHangRequest request)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = _context.DonHangs
                    .FirstOrDefault(dh => dh.MaDonHang == request.MaDonHang);
                
                if (donHang == null)
                {
                    return false;
                }

                // Kiểm tra trạng thái đơn hàng có thể xóa chi tiết không
                if (donHang.TrangThai == "da_huy" || donHang.TrangThai == "da_giao" || donHang.TrangThai == "dang_giao")
                {
                    return false;
                }

                // Tìm chi tiết đơn hàng cần xóa
                var chiTietDonHang = _context.ChiTietDonHangs
                    .FirstOrDefault(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo == request.MaLo);

                if (chiTietDonHang == null)
                {
                    return false;
                }

                // Kiểm tra xem đơn hàng có ít nhất 2 chi tiết không (không được xóa hết)
                var soChiTiet = _context.ChiTietDonHangs
                    .Count(ct => ct.MaDonHang == request.MaDonHang);

                if (soChiTiet <= 1)
                {
                    return false;
                }

                // Xóa chi tiết đơn hàng
                _context.ChiTietDonHangs.Remove(chiTietDonHang);

                // Cập nhật lại tổng số lượng và tổng giá trị của đơn hàng
                var tongSoLuong = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo != request.MaLo)
                    .Sum(ct => ct.SoLuong);
                
                var tongGiaTri = _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo != request.MaLo)
                    .Sum(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong;
                donHang.TongGiaTri = tongGiaTri;

                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HuyDonHang(int maDonHang)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = _context.DonHangs
                    .FirstOrDefault(dh => dh.MaDonHang == maDonHang);
                
                if (donHang == null)
                {
                    return false;
                }

                // Kiểm tra trạng thái đơn hàng có thể hủy không
                if (donHang.TrangThai == "da_huy" || donHang.TrangThai == "da_giao" || donHang.TrangThai == "dang_giao")
                {
                    return false;
                }

                // Cập nhật trạng thái đơn hàng thành "da_huy"
                donHang.TrangThai = "da_huy";
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateDonHang(CreateDonHangSieuThiRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = _context.SieuThis
                    .FirstOrDefault(st => st.MaSieuThi == request.MaSieuThi);
                
                if (sieuThi == null)
                {
                    return false;
                }

                // Tính tổng số lượng và tổng giá trị
                decimal tongSoLuong = request.ChiTietDonHangs.Sum(ct => ct.SoLuong);
                decimal tongGiaTri = request.ChiTietDonHangs.Sum(ct => ct.SoLuong * (ct.DonGia ?? 0));

                // Tạo đơn hàng chính
                var donHang = new DonHang
                {
                    LoaiDon = "sieu_thi",
                    NgayDat = DateTime.Now,
                    NgayGiao = request.NgayGiao,
                    TrangThai = "chua_nhan",
                    TongSoLuong = tongSoLuong,
                    TongGiaTri = tongGiaTri,
                    GhiChu = request.GhiChu
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                // Tạo đơn hàng siêu thị
                var donHangSieuThi = new DonHangSieuThi
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = request.MaSieuThi,
                    MaDaiLy = request.MaDaiLy
                };

                _context.DonHangSieuThis.Add(donHangSieuThi);

                // Tạo chi tiết đơn hàng
                foreach (var chiTiet in request.ChiTietDonHangs)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaLo = chiTiet.MaLo,
                        SoLuong = chiTiet.SoLuong,
                        DonGia = chiTiet.DonGia,
                        ThanhTien = chiTiet.SoLuong * (chiTiet.DonGia ?? 0)
                    };

                    _context.ChiTietDonHangs.Add(chiTietDonHang);
                }

                _context.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public DonHangSieuThiResponse? GetDonHangById(int maDonHang)
        {
            try
            {
                var donHang = _context.DonHangs
                    .Include(dh => dh.DonHangSieuThi)
                        .ThenInclude(dhst => dhst!.MaSieuThiNavigation)
                    .Include(dh => dh.ChiTietDonHangs)
                    .FirstOrDefault(dh => dh.MaDonHang == maDonHang);

                if (donHang?.DonHangSieuThi == null)
                    return null;

                return new DonHangSieuThiResponse
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = donHang.DonHangSieuThi.MaSieuThi,
                    MaDaiLy = donHang.DonHangSieuThi.MaDaiLy,
                    LoaiDon = donHang.LoaiDon,
                    NgayDat = donHang.NgayDat,
                    NgayGiao = donHang.NgayGiao,
                    TrangThai = donHang.TrangThai,
                    TongSoLuong = donHang.TongSoLuong,
                    TongGiaTri = donHang.TongGiaTri,
                    GhiChu = donHang.GhiChu,
                    TenSieuThi = donHang.DonHangSieuThi.MaSieuThiNavigation?.TenSieuThi,
                    ChiTietDonHangs = donHang.ChiTietDonHangs.Select(ct => new ChiTietDonHangResponse
                    {
                        MaLo = ct.MaLo,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        ThanhTien = ct.ThanhTien
                    }).ToList()
                };
            }
            catch
            {
                return null;
            }
        }

        public List<DonHangSieuThiResponse> GetDonHangsBySieuThi(int maSieuThi)
        {
            try
            {
                var donHangs = _context.DonHangs
                    .Include(dh => dh.DonHangSieuThi)
                        .ThenInclude(dhst => dhst!.MaSieuThiNavigation)
                    .Include(dh => dh.ChiTietDonHangs)
                    .Where(dh => dh.DonHangSieuThi != null && dh.DonHangSieuThi.MaSieuThi == maSieuThi)
                    .OrderByDescending(dh => dh.NgayDat)
                    .ToList();

                return donHangs.Select(donHang => new DonHangSieuThiResponse
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = donHang.DonHangSieuThi!.MaSieuThi,
                    MaDaiLy = donHang.DonHangSieuThi.MaDaiLy,
                    LoaiDon = donHang.LoaiDon,
                    NgayDat = donHang.NgayDat,
                    NgayGiao = donHang.NgayGiao,
                    TrangThai = donHang.TrangThai,
                    TongSoLuong = donHang.TongSoLuong,
                    TongGiaTri = donHang.TongGiaTri,
                    GhiChu = donHang.GhiChu,
                    TenSieuThi = donHang.DonHangSieuThi.MaSieuThiNavigation?.TenSieuThi,
                    ChiTietDonHangs = donHang.ChiTietDonHangs.Select(ct => new ChiTietDonHangResponse
                    {
                        MaLo = ct.MaLo,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        ThanhTien = ct.ThanhTien
                    }).ToList()
                }).ToList();
            }
            catch
            {
                return new List<DonHangSieuThiResponse>();
            }
        }

        public List<KhoSimpleInfo> GetDanhSachKhoBySieuThi(int maSieuThi)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = _context.SieuThis
                    .FirstOrDefault(st => st.MaSieuThi == maSieuThi);
                
                if (sieuThi == null)
                {
                    return new List<KhoSimpleInfo>();
                }

                // Lấy danh sách kho của siêu thị (chỉ thông tin cơ bản)
                var danhSachKho = _context.Khos
                    .Where(k => k.MaSieuThi == maSieuThi)
                    .Select(k => new KhoSimpleInfo
                    {
                        MaKho = k.MaKho,
                        TenKho = k.TenKho,
                        LoaiKho = k.LoaiKho,
                        DiaChi = k.DiaChi,
                        TrangThai = k.TrangThai,
                        NgayTao = k.NgayTao,
                        TongSoLoHang = k.TonKhos.Count(),
                        TongSoLuong = k.TonKhos.Sum(tk => tk.SoLuong)
                    })
                    .ToList();

                return danhSachKho;
            }
            catch
            {
                return new List<KhoSimpleInfo>();
            }
        }

        public KhoHangResponse? GetKhoHangById(int maKho)
        {
            try
            {
                var kho = _context.Khos
                    .Include(k => k.TonKhos)
                    .Include(k => k.MaSieuThiNavigation)
                    .FirstOrDefault(k => k.MaKho == maKho);

                if (kho == null)
                {
                    return null;
                }

                var tonKhoResponses = kho.TonKhos.Select(tk => new TonKhoResponse
                {
                    MaLo = tk.MaLo,
                    SoLuong = tk.SoLuong,
                    CapNhatCuoi = tk.CapNhatCuoi,
                    TenSanPham = $"Sản phẩm lô {tk.MaLo}", // Tạm thời
                    DonViTinh = "kg", // Tạm thời
                    TrangThaiLo = tk.SoLuong > 0 ? "con_hang" : "het_hang"
                }).ToList();

                return new KhoHangResponse
                {
                    MaKho = kho.MaKho,
                    TenKho = kho.TenKho,
                    LoaiKho = kho.LoaiKho,
                    DiaChi = kho.DiaChi,
                    TrangThai = kho.TrangThai,
                    NgayTao = kho.NgayTao,
                    TonKhos = tonKhoResponses,
                    TongSoLoHang = kho.TonKhos.Count,
                    TongSoLuong = kho.TonKhos.Sum(tk => tk.SoLuong)
                };
            }
            catch
            {
                return null;
            }
        }

        public bool CreateKho(CreateKhoRequest request)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = _context.SieuThis
                    .FirstOrDefault(st => st.MaSieuThi == request.MaSieuThi);
                
                if (sieuThi == null)
                {
                    return false;
                }

                // Kiểm tra tên kho đã tồn tại trong siêu thị chưa
                var existingKho = _context.Khos
                    .FirstOrDefault(k => k.MaSieuThi == request.MaSieuThi && k.TenKho == request.TenKho);

                if (existingKho != null)
                {
                    return false;
                }

                // Tạo kho mới
                var newKho = new Kho
                {
                    MaSieuThi = request.MaSieuThi,
                    TenKho = request.TenKho,
                    LoaiKho = request.LoaiKho,
                    DiaChi = request.DiaChi,
                    TrangThai = request.TrangThai ?? "hoat_dong",
                    NgayTao = DateTime.Now
                };

                _context.Khos.Add(newKho);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateKho(UpdateKhoRequest request)
        {
            try
            {
                // Tìm kho cần cập nhật
                var kho = _context.Khos
                    .FirstOrDefault(k => k.MaKho == request.MaKho);
                
                if (kho == null)
                {
                    return false;
                }

                // Kiểm tra tên kho mới có trùng với kho khác trong cùng siêu thị không
                if (kho.TenKho != request.TenKho)
                {
                    var existingKho = _context.Khos
                        .FirstOrDefault(k => k.MaSieuThi == kho.MaSieuThi && k.TenKho == request.TenKho && k.MaKho != request.MaKho);

                    if (existingKho != null)
                    {
                        return false;
                    }
                }

                // Cập nhật thông tin mới
                kho.TenKho = request.TenKho;
                kho.LoaiKho = request.LoaiKho;
                kho.DiaChi = request.DiaChi;
                kho.TrangThai = request.TrangThai;

                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteKho(int maKho)
        {
            try
            {
                // Tìm kho cần xóa
                var kho = _context.Khos
                    .Include(k => k.TonKhos)
                    .FirstOrDefault(k => k.MaKho == maKho);
                
                if (kho == null)
                {
                    return false;
                }

                // Kiểm tra kho có tồn kho không
                if (kho.TonKhos.Any())
                {
                    return false;
                }

                // Xóa kho
                _context.Khos.Remove(kho);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTonKho(DeleteTonKhoRequest request)
        {
            try
            {
                // Kiểm tra kho có tồn tại không
                var kho = _context.Khos
                    .FirstOrDefault(k => k.MaKho == request.MaKho);
                
                if (kho == null)
                {
                    return false;
                }

                // Tìm tồn kho cần xóa
                var tonKho = _context.TonKhos
                    .FirstOrDefault(tk => tk.MaKho == request.MaKho && tk.MaLo == request.MaLo);

                if (tonKho == null)
                {
                    return false;
                }

                // Xóa tồn kho
                _context.TonKhos.Remove(tonKho);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetSieuThiById(int maSieuThi)
        {
            try
            {
                return _context.SieuThis
                    .FirstOrDefault(st => st.MaSieuThi == maSieuThi) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}