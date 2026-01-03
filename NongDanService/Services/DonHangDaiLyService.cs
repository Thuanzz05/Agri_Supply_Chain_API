using NongDanService.Data;
using NongDanService.Models.DTOs;

namespace NongDanService.Services
{
    public class DonHangDaiLyService : IDonHangDaiLyService
    {
        private readonly IDonHangDaiLyRepository _repo;

        public DonHangDaiLyService(IDonHangDaiLyRepository repo)
        {
            _repo = repo;
        }

        public List<DonHangDaiLyDTO> GetAll() => _repo.GetAll();

        public DonHangDaiLyDTO? GetById(int id) => _repo.GetById(id);

        public List<DonHangDaiLyDTO> GetByNongDanId(int maNongDan) => _repo.GetByNongDanId(maNongDan);

        public List<DonHangDaiLyDTO> GetByDaiLyId(int maDaiLy) => _repo.GetByDaiLyId(maDaiLy);

        public int Create(DonHangDaiLyCreateDTO dto) => _repo.Create(dto);

        public bool Update(int id, DonHangDaiLyUpdateDTO dto) => _repo.Update(id, dto);

        public bool XacNhanDon(int id) => _repo.UpdateTrangThai(id, "da_xac_nhan");

        public bool XuatDon(int id) => _repo.UpdateTrangThai(id, "da_xuat");

        public bool HuyDon(int id) => _repo.UpdateTrangThai(id, "da_huy");

        public bool Delete(int id) => _repo.Delete(id);
    }
}