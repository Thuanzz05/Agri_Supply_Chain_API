using SieuThiService.Models.DTOs;
using SieuThiService.Models.Entities;

namespace SieuThiService.Data
{
    public interface ISieuThiRepository
    {
        // API gộp (hiện tại)
        Task<DonHangSieuThiResponse?> CreateDonHangAsync(CreateDonHangSieuThiRequest request);
        Task<DonHangSieuThiResponse?> GetDonHangByIdAsync(int maDonHang);
        Task<List<DonHangSieuThiResponse>> GetDonHangsBySieuThiAsync(int maSieuThi);
        
        // API riêng biệt cho quản lý
        Task<DonHangResponse?> CreateDonHangOnlyAsync(CreateDonHangRequest request);
        Task<ChiTietDonHangAddResponse?> AddChiTietDonHangAsync(CreateChiTietDonHangRequest request);
        
        Task<SieuThi?> GetSieuThiByIdAsync(int maSieuThi);
    }
}
