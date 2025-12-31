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

        public async Task<SieuThi?> GetSieuThiByIdAsync(int maSieuThi)
        {
            return await _context.SieuThis
                .FirstOrDefaultAsync(st => st.MaSieuThi == maSieuThi);
        }
    }
}
