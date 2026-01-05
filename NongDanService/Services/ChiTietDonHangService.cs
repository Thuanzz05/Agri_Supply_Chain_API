using NongDanService.Data;
using NongDanService.Models.DTOs;

namespace NongDanService.Services
{
    public class ChiTietDonHangService : IChiTietDonHangService
    {
        private readonly IChiTietDonHangRepository _repo;

        public ChiTietDonHangService(IChiTietDonHangRepository repo)
        {
            _repo = repo;
        }

        public List<ChiTietDonHangDTO> GetByDonHangId(int maDonHang) => _repo.GetByDonHangId(maDonHang);

        public ChiTietDonHangDTO? GetById(int maDonHang, int maLo) => _repo.GetById(maDonHang, maLo);

        public bool Create(ChiTietDonHangCreateDTO dto) => _repo.Create(dto);

        public bool Update(int maDonHang, int maLo, ChiTietDonHangUpdateDTO dto) => _repo.Update(maDonHang, maLo, dto);

        public bool Delete(int maDonHang, int maLo) => _repo.Delete(maDonHang, maLo);
    }
}
