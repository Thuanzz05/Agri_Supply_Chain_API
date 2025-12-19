using NongDanService.Models.Entities;

namespace NongDanService.Data
{
    public interface ISanPhamRepository
    {
        Task<List<SanPham>> GetAll();
        Task<SanPham?> GetById(int id);
        Task Create(SanPham sanPham);
        Task Update(SanPham sanPham);
        Task Delete(SanPham sanPham);
    }
}
