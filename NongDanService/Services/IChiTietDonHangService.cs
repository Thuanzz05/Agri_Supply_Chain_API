using NongDanService.Models.DTOs;

namespace NongDanService.Services
{
    public interface IChiTietDonHangService
    {
        List<ChiTietDonHangDTO> GetByDonHangId(int maDonHang);
        ChiTietDonHangDTO? GetById(int maDonHang, int maLo);
        bool Create(ChiTietDonHangCreateDTO dto);
        bool Update(int maDonHang, int maLo, ChiTietDonHangUpdateDTO dto);
        bool Delete(int maDonHang, int maLo);
    }
}
