using NongDanService.Models.DTOs;

namespace NongDanService.Services
{
    public interface ISanPhamService
    {
        Task<List<SanPhamDTO>> GetAll();
        Task<SanPhamDTO?> GetById(int id);
        Task<int> Create(SanPhamCreateDTO dto);
        Task<bool> Update(int id, SanPhamUpdateDTO dto);
        Task<bool> Delete(int id);
    }

}
