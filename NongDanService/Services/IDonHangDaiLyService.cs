using NongDanService.Models.DTOs;

namespace NongDanService.Services
{
    public interface IDonHangDaiLyService
    {
        List<DonHangDaiLyDTO> GetAll();
        DonHangDaiLyDTO? GetById(int id);
        List<DonHangDaiLyDTO> GetByNongDanId(int maNongDan);
        List<DonHangDaiLyDTO> GetByDaiLyId(int maDaiLy);
        int Create(DonHangDaiLyCreateDTO dto);
        bool Update(int id, DonHangDaiLyUpdateDTO dto);
        bool XacNhanDon(int id);
        bool XuatDon(int id);
        bool HuyDon(int id);
        bool Delete(int id);
    }
}