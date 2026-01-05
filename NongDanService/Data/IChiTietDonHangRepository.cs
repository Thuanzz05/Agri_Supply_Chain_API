using NongDanService.Models.DTOs;

namespace NongDanService.Data
{
    public interface IChiTietDonHangRepository
    {
        List<ChiTietDonHangDTO> GetByDonHangId(int maDonHang);
        ChiTietDonHangDTO? GetById(int maDonHang, int maLo);
        bool Create(ChiTietDonHangCreateDTO dto);
        bool Update(int maDonHang, int maLo, ChiTietDonHangUpdateDTO dto);
        bool Delete(int maDonHang, int maLo);
        bool DeleteByDonHang(int maDonHang);
    }
}
