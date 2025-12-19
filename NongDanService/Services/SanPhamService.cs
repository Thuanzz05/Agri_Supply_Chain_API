using NongDanService.Data;
using NongDanService.Models.DTOs;
using NongDanService.Models.Entities;

namespace NongDanService.Services
{
    public class SanPhamService : ISanPhamService
    {
        private readonly ISanPhamRepository _repo;

        public SanPhamService(ISanPhamRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SanPhamDTO>> GetAll()
        {
            var list = await _repo.GetAll();
            return list.Select(x => new SanPhamDTO
            {
                MaSanPham = x.MaSanPham,
                TenSanPham = x.TenSanPham,
                DonViTinh = x.DonViTinh,
                MoTa = x.MoTa
            }).ToList();
        }

        public async Task<SanPhamDTO?> GetById(int id)
        {
            var sp = await _repo.GetById(id);
            if (sp == null) return null;

            return new SanPhamDTO
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                DonViTinh = sp.DonViTinh,
                MoTa = sp.MoTa
            };
        }

        public async Task<int> Create(SanPhamCreateDTO dto)
        {
            var sp = new SanPham
            {
                TenSanPham = dto.TenSanPham,
                DonViTinh = dto.DonViTinh,
                MoTa = dto.MoTa,
                NgayTao = DateTime.Now
            };

            await _repo.Create(sp);
            return sp.MaSanPham;
        }

        public async Task<bool> Update(int id, SanPhamUpdateDTO dto)
        {
            var sp = await _repo.GetById(id);
            if (sp == null) return false;

            sp.TenSanPham = dto.TenSanPham;
            sp.DonViTinh = dto.DonViTinh;
            sp.MoTa = dto.MoTa;

            await _repo.Update(sp);
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var sp = await _repo.GetById(id);
            if (sp == null) return false;

            await _repo.Delete(sp);
            return true;
        }
    }
    
}
