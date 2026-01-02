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

        public async Task<DonHangResponse?> CreateDonHangOnlyAsync(CreateDonHangRequest request)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _context.SieuThis
                    .FirstOrDefaultAsync(st => st.MaSieuThi == request.MaSieuThi);
                
                if (sieuThi == null)
                {
                    return null;
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
                await _context.SaveChangesAsync();

                // Tạo đơn hàng siêu thị
                var donHangSieuThi = new DonHangSieuThi
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = request.MaSieuThi,
                    MaDaiLy = request.MaDaiLy
                };

                _context.DonHangSieuThis.Add(donHangSieuThi);
                await _context.SaveChangesAsync();

                return new DonHangResponse
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSieuThi = request.MaSieuThi,
                    MaDaiLy = request.MaDaiLy,
                    LoaiDon = donHang.LoaiDon,
                    NgayDat = donHang.NgayDat,
                    NgayGiao = donHang.NgayGiao,
                    TrangThai = donHang.TrangThai,
                    TongSoLuong = donHang.TongSoLuong,
                    TongGiaTri = donHang.TongGiaTri,
                    GhiChu = donHang.GhiChu,
                    TenSieuThi = sieuThi.TenSieuThi
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<ChiTietDonHangAddResponse?> AddChiTietDonHangAsync(CreateChiTietDonHangRequest request)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = await _context.DonHangs
                    .FirstOrDefaultAsync(dh => dh.MaDonHang == request.MaDonHang);
                
                if (donHang == null)
                {
                    return null;
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
                var tongSoLuong = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .SumAsync(ct => ct.SoLuong);
                
                var tongGiaTri = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .SumAsync(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong + request.SoLuong;
                donHang.TongGiaTri = tongGiaTri + thanhTien;

                await _context.SaveChangesAsync();

                return new ChiTietDonHangAddResponse
                {
                    MaDonHang = request.MaDonHang,
                    MaLo = request.MaLo,
                    SoLuong = request.SoLuong,
                    DonGia = request.DonGia,
                    ThanhTien = thanhTien
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<NhanHangResponse?> NhanHangAsync(int maDonHang, NhanHangRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = await _context.DonHangs
                    .Include(dh => dh.DonHangSieuThi)
                    .Include(dh => dh.ChiTietDonHangs)
                    .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
                
                if (donHang == null)
                {
                    return null;
                }

                // Kiểm tra trạng thái đơn hàng có thể nhận không
                if (donHang.TrangThai == "da_huy")
                {
                    return new NhanHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = donHang.TrangThai ?? "",
                        TrangThaiMoi = donHang.TrangThai ?? "",
                        NgayNhan = DateTime.Now,
                        GhiChuNhan = request.GhiChuNhan,
                        MaKhoNhan = request.MaKho,
                        Message = "Không thể nhận đơn hàng đã bị hủy",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "da_nhan")
                {
                    return new NhanHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = donHang.TrangThai ?? "",
                        TrangThaiMoi = "da_nhan",
                        NgayNhan = DateTime.Now,
                        GhiChuNhan = request.GhiChuNhan,
                        MaKhoNhan = request.MaKho,
                        Message = "Đơn hàng đã được nhận trước đó",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "da_giao")
                {
                    return new NhanHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = donHang.TrangThai ?? "",
                        TrangThaiMoi = donHang.TrangThai ?? "",
                        NgayNhan = DateTime.Now,
                        GhiChuNhan = request.GhiChuNhan,
                        MaKhoNhan = request.MaKho,
                        Message = "Đơn hàng đã được giao, không thể nhận lại",
                        Success = false
                    };
                }

                // Lưu trạng thái cũ
                string trangThaiCu = donHang.TrangThai ?? "";

                // Kiểm tra kho được chọn có tồn tại và thuộc về siêu thị không
                var khoNhan = await _context.Khos
                    .FirstOrDefaultAsync(k => k.MaKho == request.MaKho && k.MaSieuThi == donHang.DonHangSieuThi!.MaSieuThi);

                if (khoNhan == null)
                {
                    return new NhanHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = trangThaiCu,
                        TrangThaiMoi = trangThaiCu,
                        NgayNhan = DateTime.Now,
                        GhiChuNhan = request.GhiChuNhan,
                        MaKhoNhan = request.MaKho,
                        Message = "Kho không tồn tại hoặc không thuộc về siêu thị này",
                        Success = false
                    };
                }

                // Kiểm tra trạng thái kho
                if (khoNhan.TrangThai != "hoat_dong")
                {
                    return new NhanHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = trangThaiCu,
                        TrangThaiMoi = trangThaiCu,
                        NgayNhan = DateTime.Now,
                        GhiChuNhan = request.GhiChuNhan,
                        MaKhoNhan = request.MaKho,
                        TenKhoNhan = khoNhan.TenKho,
                        Message = $"Kho '{khoNhan.TenKho}' không hoạt động, không thể nhận hàng",
                        Success = false
                    };
                }

                // Cập nhật tồn kho cho từng chi tiết đơn hàng
                var tonKhoCapNhats = new List<TonKhoCapNhat>();
                
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    // Kiểm tra xem lô hàng đã có trong kho chưa
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(tk => tk.MaKho == request.MaKho && tk.MaLo == chiTiet.MaLo);

                    if (tonKho != null)
                    {
                        // Nếu đã có, cộng thêm số lượng
                        decimal soLuongCu = tonKho.SoLuong;
                        tonKho.SoLuong += chiTiet.SoLuong;
                        tonKho.CapNhatCuoi = DateTime.Now;

                        tonKhoCapNhats.Add(new TonKhoCapNhat
                        {
                            MaLo = chiTiet.MaLo,
                            SoLuongThem = chiTiet.SoLuong,
                            SoLuongTonMoi = tonKho.SoLuong,
                            TrangThai = "cap_nhat"
                        });
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

                        tonKhoCapNhats.Add(new TonKhoCapNhat
                        {
                            MaLo = chiTiet.MaLo,
                            SoLuongThem = chiTiet.SoLuong,
                            SoLuongTonMoi = chiTiet.SoLuong,
                            TrangThai = "tao_moi"
                        });
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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new NhanHangResponse
                {
                    MaDonHang = maDonHang,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = "da_nhan",
                    NgayNhan = DateTime.Now,
                    GhiChuNhan = request.GhiChuNhan,
                    MaKhoNhan = request.MaKho,
                    TenKhoNhan = khoNhan.TenKho,
                    Message = $"Nhận hàng thành công vào kho '{khoNhan.TenKho}' và đã cập nhật tồn kho",
                    Success = true,
                    TonKhoCapNhats = tonKhoCapNhats
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<HuyDonHangResponse?> HuyDonHangAsync(int maDonHang)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var donHang = await _context.DonHangs
                    .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
                
                if (donHang == null)
                {
                    return null;
                }

                // Kiểm tra trạng thái đơn hàng có thể hủy không
                if (donHang.TrangThai == "da_huy")
                {
                    return new HuyDonHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = donHang.TrangThai ?? "",
                        TrangThaiMoi = "da_huy",
                        NgayHuy = DateTime.Now,
                        Message = "Đơn hàng đã được hủy trước đó",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "da_giao" || donHang.TrangThai == "dang_giao")
                {
                    return new HuyDonHangResponse
                    {
                        MaDonHang = maDonHang,
                        TrangThaiCu = donHang.TrangThai ?? "",
                        TrangThaiMoi = donHang.TrangThai ?? "",
                        NgayHuy = DateTime.Now,
                        Message = "Không thể hủy đơn hàng đã giao hoặc đang giao",
                        Success = false
                    };
                }

                // Lưu trạng thái cũ
                string trangThaiCu = donHang.TrangThai ?? "";

                // Cập nhật trạng thái đơn hàng thành "da_huy"
                donHang.TrangThai = "da_huy";
                await _context.SaveChangesAsync();

                return new HuyDonHangResponse
                {
                    MaDonHang = maDonHang,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = "da_huy",
                    NgayHuy = DateTime.Now,
                    Message = "Hủy đơn hàng thành công",
                    Success = true
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<DonHangSieuThiResponse?> CreateDonHangAsync(CreateDonHangSieuThiRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _context.SieuThis
                    .FirstOrDefaultAsync(st => st.MaSieuThi == request.MaSieuThi);
                
                if (sieuThi == null)
                {
                    return null;
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
                await _context.SaveChangesAsync();

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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Trả về thông tin đơn hàng đã tạo
                return await GetDonHangByIdAsync(donHang.MaDonHang);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<DonHangSieuThiResponse?> GetDonHangByIdAsync(int maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(dh => dh.DonHangSieuThi)
                    .ThenInclude(dhst => dhst!.MaSieuThiNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

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

        public async Task<List<DonHangSieuThiResponse>> GetDonHangsBySieuThiAsync(int maSieuThi)
        {
            var donHangs = await _context.DonHangs
                .Include(dh => dh.DonHangSieuThi)
                    .ThenInclude(dhst => dhst!.MaSieuThiNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                .Where(dh => dh.DonHangSieuThi != null && dh.DonHangSieuThi.MaSieuThi == maSieuThi)
                .OrderByDescending(dh => dh.NgayDat)
                .ToListAsync();

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

        public async Task<DanhSachKhoResponse?> GetKhoHangBySieuThiAsync(int maSieuThi)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _context.SieuThis
                    .FirstOrDefaultAsync(st => st.MaSieuThi == maSieuThi);
                
                if (sieuThi == null)
                {
                    return null;
                }

                // Lấy danh sách kho của siêu thị
                var danhSachKho = await _context.Khos
                    .Include(k => k.TonKhos)
                    .Where(k => k.MaSieuThi == maSieuThi)
                    .ToListAsync();

                var khoHangResponses = new List<KhoHangResponse>();
                decimal tongSoLuongTatCaKho = 0;

                foreach (var kho in danhSachKho)
                {
                    var tonKhoResponses = kho.TonKhos.Select(tk => new TonKhoResponse
                    {
                        MaLo = tk.MaLo,
                        SoLuong = tk.SoLuong,
                        CapNhatCuoi = tk.CapNhatCuoi,
                        TenSanPham = $"Sản phẩm lô {tk.MaLo}", // Tạm thời, sau này có thể join với service khác
                        DonViTinh = "kg", // Tạm thời
                        TrangThaiLo = tk.SoLuong > 0 ? "con_hang" : "het_hang"
                    }).ToList();

                    decimal tongSoLuongKho = kho.TonKhos.Sum(tk => tk.SoLuong);
                    tongSoLuongTatCaKho += tongSoLuongKho;

                    khoHangResponses.Add(new KhoHangResponse
                    {
                        MaKho = kho.MaKho,
                        TenKho = kho.TenKho,
                        LoaiKho = kho.LoaiKho,
                        DiaChi = kho.DiaChi,
                        TrangThai = kho.TrangThai,
                        NgayTao = kho.NgayTao,
                        TonKhos = tonKhoResponses,
                        TongSoLoHang = kho.TonKhos.Count,
                        TongSoLuong = tongSoLuongKho
                    });
                }

                return new DanhSachKhoResponse
                {
                    MaSieuThi = maSieuThi,
                    TenSieuThi = sieuThi.TenSieuThi,
                    DanhSachKho = khoHangResponses,
                    TongSoKho = danhSachKho.Count,
                    TongSoLuongTatCaKho = tongSoLuongTatCaKho
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<KhoHangResponse?> GetKhoHangByIdAsync(int maKho)
        {
            try
            {
                var kho = await _context.Khos
                    .Include(k => k.TonKhos)
                    .Include(k => k.MaSieuThiNavigation)
                    .FirstOrDefaultAsync(k => k.MaKho == maKho);

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
                throw;
            }
        }

        public async Task<SieuThi?> GetSieuThiByIdAsync(int maSieuThi)
        {
            return await _context.SieuThis
                .FirstOrDefaultAsync(st => st.MaSieuThi == maSieuThi);
        }
    }
}
