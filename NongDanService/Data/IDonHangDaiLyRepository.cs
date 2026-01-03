using NongDanService.Models.DTOs;

namespace NongDanService.Data
{
    public interface IDonHangDaiLyRepository
    {
        List<DonHangDaiLyDTO> GetAll();
        DonHangDaiLyDTO? GetById(int id);
        List<DonHangDaiLyDTO> GetByNongDanId(int maNongDan);
        List<DonHangDaiLyDTO> GetByDaiLyId(int maDaiLy);
        int Create(DonHangDaiLyCreateDTO dto);
        bool Update(int id, DonHangDaiLyUpdateDTO dto);
        bool UpdateTrangThai(int id, string trangThai);
        bool Delete(int id);
    }
}