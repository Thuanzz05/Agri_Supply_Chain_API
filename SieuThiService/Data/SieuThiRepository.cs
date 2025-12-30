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
