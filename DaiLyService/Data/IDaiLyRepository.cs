using DaiLyService.Models.Entities;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Data
{
    public interface IDaiLyRepository
    {
        Task<List<DaiLyPhanHoi>> GetAllAsync();
        Task<DaiLyPhanHoi?> GetByIdAsync(int maDaiLy);
        Task<int> CreateAsync(DaiLyTaoMoi model);
        Task<bool> UpdateAsync(int maDaiLy, DaiLy entity);
        Task<bool> DeleteAsync(int maDaiLy);
        Task<List<DaiLyPhanHoi>> SearchAsync(string? tenDaiLy, string? soDienThoai);
    }
}