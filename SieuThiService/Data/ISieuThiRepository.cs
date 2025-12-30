using SieuThiService.Models.DTOs;
using SieuThiService.Models.Entities;

namespace SieuThiService.Data
{
    public interface ISieuThiRepository
    {
        Task<List<DonHangSieuThiResponse>> GetDonHangsBySieuThiAsync(int maSieuThi);
        Task<SieuThi?> GetSieuThiByIdAsync(int maSieuThi);
    }
}
