using Microsoft.EntityFrameworkCore;
using NongDanService.Models.Entities;

namespace NongDanService.Data
{
    public class SanPhamRepository : ISanPhamRepository
    {
        private readonly BtlHdv1Context _context;

        public SanPhamRepository(BtlHdv1Context context)
        {
            _context = context;
        }

        public async Task<List<SanPham>> GetAll()
        {
            return await _context.SanPhams.ToListAsync();
        }

        public async Task<SanPham?> GetById(int id)
        {
            return await _context.SanPhams.FindAsync(id);
        }

        public async Task Create(SanPham sanPham)
        {
            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task Update(SanPham sanPham)
        {
            _context.SanPhams.Update(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(SanPham sanPham)
        {
            _context.SanPhams.Remove(sanPham);
            await _context.SaveChangesAsync();
        }
    }
}
