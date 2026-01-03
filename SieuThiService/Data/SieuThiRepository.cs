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

        public async Task<UpdateChiTietDonHangResponse?> UpdateChiTietDonHangAsync(UpdateChiTietDonHangRequest request)
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

                // Kiểm tra trạng thái đơn hàng có thể sửa không
                if (donHang.TrangThai == "da_huy")
                {
                    return new UpdateChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        SoLuongCu = 0,
                        SoLuongMoi = 0,
                        NgayCapNhat = DateTime.Now,
                        Message = "Không thể sửa chi tiết đơn hàng đã bị hủy",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "da_giao")
                {
                    return new UpdateChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        SoLuongCu = 0,
                        SoLuongMoi = 0,
                        NgayCapNhat = DateTime.Now,
                        Message = "Không thể sửa chi tiết đơn hàng đã được giao",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "dang_giao")
                {
                    return new UpdateChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        SoLuongCu = 0,
                        SoLuongMoi = 0,
                        NgayCapNhat = DateTime.Now,
                        Message = "Không thể sửa chi tiết đơn hàng đang được giao",
                        Success = false
                    };
                }

                // Tìm chi tiết đơn hàng cần sửa
                var chiTietDonHang = await _context.ChiTietDonHangs
                    .FirstOrDefaultAsync(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo == request.MaLo);

                if (chiTietDonHang == null)
                {
                    return new UpdateChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        SoLuongCu = 0,
                        SoLuongMoi = 0,
                        NgayCapNhat = DateTime.Now,
                        Message = "Không tìm thấy chi tiết đơn hàng với lô này",
                        Success = false
                    };
                }

                // Lưu giá trị cũ
                decimal soLuongCu = chiTietDonHang.SoLuong;
                decimal? donGiaCu = chiTietDonHang.DonGia;
                decimal? thanhTienCu = chiTietDonHang.ThanhTien;

                // Cập nhật giá trị mới
                chiTietDonHang.SoLuong = request.SoLuong;
                chiTietDonHang.DonGia = request.DonGia;
                chiTietDonHang.ThanhTien = request.SoLuong * (request.DonGia ?? 0);

                // Cập nhật tổng số lượng và tổng giá trị của đơn hàng
                var tongSoLuong = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .SumAsync(ct => ct.SoLuong);
                
                var tongGiaTri = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .SumAsync(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong;
                donHang.TongGiaTri = tongGiaTri;

                await _context.SaveChangesAsync();

                return new UpdateChiTietDonHangResponse
                {
                    MaDonHang = request.MaDonHang,
                    MaLo = request.MaLo,
                    SoLuongCu = soLuongCu,
                    SoLuongMoi = chiTietDonHang.SoLuong,
                    DonGiaCu = donGiaCu,
                    DonGiaMoi = chiTietDonHang.DonGia,
                    ThanhTienCu = thanhTienCu,
                    ThanhTienMoi = chiTietDonHang.ThanhTien,
                    NgayCapNhat = DateTime.Now,
                    Message = "Cập nhật chi tiết đơn hàng thành công",
                    Success = true
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<DeleteChiTietDonHangResponse?> DeleteChiTietDonHangAsync(DeleteChiTietDonHangRequest request)
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

                // Kiểm tra trạng thái đơn hàng có thể xóa chi tiết không
                if (donHang.TrangThai == "da_huy")
                {
                    return new DeleteChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        NgayXoa = DateTime.Now,
                        Message = "Không thể xóa chi tiết đơn hàng đã bị hủy",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "da_giao")
                {
                    return new DeleteChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        NgayXoa = DateTime.Now,
                        Message = "Không thể xóa chi tiết đơn hàng đã được giao",
                        Success = false
                    };
                }

                if (donHang.TrangThai == "dang_giao")
                {
                    return new DeleteChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        NgayXoa = DateTime.Now,
                        Message = "Không thể xóa chi tiết đơn hàng đang được giao",
                        Success = false
                    };
                }

                // Tìm chi tiết đơn hàng cần xóa
                var chiTietDonHang = await _context.ChiTietDonHangs
                    .FirstOrDefaultAsync(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo == request.MaLo);

                if (chiTietDonHang == null)
                {
                    return new DeleteChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        NgayXoa = DateTime.Now,
                        Message = "Không tìm thấy chi tiết đơn hàng với lô này",
                        Success = false
                    };
                }

                // Kiểm tra xem đơn hàng có ít nhất 2 chi tiết không (không được xóa hết)
                var soChiTiet = await _context.ChiTietDonHangs
                    .CountAsync(ct => ct.MaDonHang == request.MaDonHang);

                if (soChiTiet <= 1)
                {
                    return new DeleteChiTietDonHangResponse
                    {
                        MaDonHang = request.MaDonHang,
                        MaLo = request.MaLo,
                        NgayXoa = DateTime.Now,
                        Message = "Không thể xóa chi tiết cuối cùng. Đơn hàng phải có ít nhất một sản phẩm",
                        Success = false
                    };
                }

                // Lưu thông tin chi tiết sẽ bị xóa
                string tenSanPham = $"Lô số {request.MaLo}"; // Tạm thời dùng mã lô
                decimal soLuongDaXoa = chiTietDonHang.SoLuong;
                decimal? donGiaDaXoa = chiTietDonHang.DonGia;
                decimal? thanhTienDaXoa = chiTietDonHang.ThanhTien;

                // Xóa chi tiết đơn hàng
                _context.ChiTietDonHangs.Remove(chiTietDonHang);

                // Cập nhật lại tổng số lượng và tổng giá trị của đơn hàng
                var tongSoLuong = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo != request.MaLo)
                    .SumAsync(ct => ct.SoLuong);
                
                var tongGiaTri = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang && ct.MaLo != request.MaLo)
                    .SumAsync(ct => ct.ThanhTien ?? 0);

                donHang.TongSoLuong = tongSoLuong;
                donHang.TongGiaTri = tongGiaTri;

                await _context.SaveChangesAsync();

                return new DeleteChiTietDonHangResponse
                {
                    MaDonHang = request.MaDonHang,
                    MaLo = request.MaLo,
                    TenSanPham = tenSanPham,
                    SoLuongDaXoa = soLuongDaXoa,
                    DonGiaDaXoa = donGiaDaXoa,
                    ThanhTienDaXoa = thanhTienDaXoa,
                    TongSoLuongConLai = tongSoLuong,
                    TongGiaTriConLai = tongGiaTri,
                    NgayXoa = DateTime.Now,
                    Message = "Xóa chi tiết đơn hàng thành công",
                    Success = true
                };
            }
            catch
            {
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

        public async Task<DanhSachKhoSimpleResponse?> GetDanhSachKhoBySieuThiAsync(int maSieuThi)
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

                // Lấy danh sách kho của siêu thị (chỉ thông tin cơ bản)
                var danhSachKho = await _context.Khos
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
                    .ToListAsync();

                return new DanhSachKhoSimpleResponse
                {
                    MaSieuThi = maSieuThi,
                    TenSieuThi = sieuThi.TenSieuThi,
                    DanhSachKho = danhSachKho,
                    TongSoKho = danhSachKho.Count
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

        public async Task<CreateKhoResponse?> CreateKhoAsync(CreateKhoRequest request)
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

                // Kiểm tra tên kho đã tồn tại trong siêu thị chưa
                var existingKho = await _context.Khos
                    .FirstOrDefaultAsync(k => k.MaSieuThi == request.MaSieuThi && k.TenKho == request.TenKho);

                if (existingKho != null)
                {
                    return new CreateKhoResponse
                    {
                        MaKho = 0,
                        MaSieuThi = request.MaSieuThi,
                        TenSieuThi = sieuThi.TenSieuThi ?? "",
                        TenKho = request.TenKho,
                        NgayTao = DateTime.Now,
                        Message = "Tên kho đã tồn tại trong siêu thị này",
                        Success = false
                    };
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
                await _context.SaveChangesAsync();

                return new CreateKhoResponse
                {
                    MaKho = newKho.MaKho,
                    MaSieuThi = request.MaSieuThi,
                    TenSieuThi = sieuThi.TenSieuThi ?? "",
                    TenKho = newKho.TenKho,
                    LoaiKho = newKho.LoaiKho,
                    DiaChi = newKho.DiaChi,
                    TrangThai = newKho.TrangThai,
                    NgayTao = newKho.NgayTao ?? DateTime.Now,
                    Message = "Tạo kho thành công",
                    Success = true
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<UpdateKhoResponse?> UpdateKhoAsync(UpdateKhoRequest request)
        {
            try
            {
                // Tìm kho cần cập nhật
                var kho = await _context.Khos
                    .FirstOrDefaultAsync(k => k.MaKho == request.MaKho);
                
                if (kho == null)
                {
                    return null;
                }

                // Kiểm tra tên kho mới có trùng với kho khác trong cùng siêu thị không
                if (kho.TenKho != request.TenKho)
                {
                    var existingKho = await _context.Khos
                        .FirstOrDefaultAsync(k => k.MaSieuThi == kho.MaSieuThi && k.TenKho == request.TenKho && k.MaKho != request.MaKho);

                    if (existingKho != null)
                    {
                        return new UpdateKhoResponse
                        {
                            MaKho = request.MaKho,
                            TenKhoCu = kho.TenKho,
                            TenKhoMoi = request.TenKho,
                            NgayCapNhat = DateTime.Now,
                            Message = "Tên kho đã tồn tại trong siêu thị này",
                            Success = false
                        };
                    }
                }

                // Lưu thông tin cũ
                string tenKhoCu = kho.TenKho;
                string loaiKhoCu = kho.LoaiKho;
                string? diaChiCu = kho.DiaChi;
                string? trangThaiCu = kho.TrangThai;

                // Cập nhật thông tin mới
                kho.TenKho = request.TenKho;
                kho.LoaiKho = request.LoaiKho;
                kho.DiaChi = request.DiaChi;
                kho.TrangThai = request.TrangThai;

                await _context.SaveChangesAsync();

                return new UpdateKhoResponse
                {
                    MaKho = request.MaKho,
                    TenKhoCu = tenKhoCu,
                    TenKhoMoi = kho.TenKho,
                    LoaiKhoCu = loaiKhoCu,
                    LoaiKhoMoi = kho.LoaiKho,
                    DiaChiCu = diaChiCu,
                    DiaChiMoi = kho.DiaChi,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = kho.TrangThai,
                    NgayCapNhat = DateTime.Now,
                    Message = "Cập nhật kho thành công",
                    Success = true
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<DeleteKhoResponse?> DeleteKhoAsync(int maKho)
        {
            try
            {
                // Tìm kho cần xóa
                var kho = await _context.Khos
                    .Include(k => k.TonKhos)
                    .FirstOrDefaultAsync(k => k.MaKho == maKho);
                
                if (kho == null)
                {
                    return null;
                }

                // Kiểm tra kho có tồn kho không
                if (kho.TonKhos.Any())
                {
                    return new DeleteKhoResponse
                    {
                        MaKho = maKho,
                        TenKho = kho.TenKho,
                        LoaiKho = kho.LoaiKho,
                        SoLuongTonKhoDaXoa = kho.TonKhos.Count,
                        NgayXoa = DateTime.Now,
                        Message = "Không thể xóa kho vì còn tồn kho. Vui lòng xóa hết tồn kho trước",
                        Success = false
                    };
                }

                // Lưu thông tin trước khi xóa
                string tenKho = kho.TenKho;
                string loaiKho = kho.LoaiKho;
                int soLuongTonKho = kho.TonKhos.Count;

                // Xóa kho
                _context.Khos.Remove(kho);
                await _context.SaveChangesAsync();

                return new DeleteKhoResponse
                {
                    MaKho = maKho,
                    TenKho = tenKho,
                    LoaiKho = loaiKho,
                    SoLuongTonKhoDaXoa = soLuongTonKho,
                    NgayXoa = DateTime.Now,
                    Message = "Xóa kho thành công",
                    Success = true
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<DeleteTonKhoResponse?> DeleteTonKhoAsync(DeleteTonKhoRequest request)
        {
            try
            {
                // Kiểm tra kho có tồn tại không
                var kho = await _context.Khos
                    .FirstOrDefaultAsync(k => k.MaKho == request.MaKho);
                
                if (kho == null)
                {
                    return null;
                }

                // Tìm tồn kho cần xóa
                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(tk => tk.MaKho == request.MaKho && tk.MaLo == request.MaLo);

                if (tonKho == null)
                {
                    return new DeleteTonKhoResponse
                    {
                        MaKho = request.MaKho,
                        TenKho = kho.TenKho,
                        MaLo = request.MaLo,
                        SoLuongDaXoa = 0,
                        NgayXoa = DateTime.Now,
                        Message = "Không tìm thấy tồn kho với lô này trong kho",
                        Success = false
                    };
                }

                // Lưu thông tin trước khi xóa
                decimal soLuongDaXoa = tonKho.SoLuong;

                // Xóa tồn kho
                _context.TonKhos.Remove(tonKho);
                await _context.SaveChangesAsync();

                return new DeleteTonKhoResponse
                {
                    MaKho = request.MaKho,
                    TenKho = kho.TenKho,
                    MaLo = request.MaLo,
                    SoLuongDaXoa = soLuongDaXoa,
                    NgayXoa = DateTime.Now,
                    Message = "Xóa tồn kho thành công",
                    Success = true
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
