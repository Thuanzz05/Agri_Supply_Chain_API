using DaiLyService.Models.DTOs;

namespace DaiLyService.Services
{
    public interface IDaiLyService
    {
        Task<List<DaiLyPhanHoi>> LayTatCaDaiLy();
        Task<DaiLyPhanHoi?> LayDaiLyTheoMa(int maDaiLy);
        Task<DaiLyPhanHoi> TaoMoiDaiLy(DaiLyTaoMoi model);
        Task<bool> CapNhatDaiLy(int maDaiLy, DaiLyTaoMoi model);
        Task<bool> XoaDaiLy(int maDaiLy);
        Task<List<DaiLyPhanHoi>> TimKiemDaiLy(string? tenDaiLy, string? soDienThoai);
    }
}