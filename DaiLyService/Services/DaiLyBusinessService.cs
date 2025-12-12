using DaiLyService.Data;
using DaiLyService.Models.Entities;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Services
{
    public class DaiLyBusinessService : IDaiLyService
    {
        private readonly IDaiLyRepository _repository;

        public DaiLyBusinessService(IDaiLyRepository repository)
        {
            _repository = repository;
        }

        // 1. GET ALL
        public async Task<List<DaiLyPhanHoi>> LayTatCaDaiLy()
        {
            return await _repository.GetAllAsync();
        }

        // 2. GET BY ID
        public async Task<DaiLyPhanHoi?> LayDaiLyTheoMa(int maDaiLy)
        {
            return await _repository.GetByIdAsync(maDaiLy);
        }

        // 3. CREATE
        public async Task<DaiLyPhanHoi> TaoMoiDaiLy(DaiLyTaoMoi model)
        {
            // B1: Tạo mới và lấy MaDaiLy
            int maDaiLyMoi = await _repository.CreateAsync(model);

            if (maDaiLyMoi > 0)
            {
                // B2: Lấy lại thông tin đầy đủ
                var daiLyMoi = await _repository.GetByIdAsync(maDaiLyMoi);

                if (daiLyMoi == null)
                {
                    throw new Exception("Tạo mới thành công nhưng không tìm thấy dữ liệu trả về.");
                }

                return daiLyMoi;
            }

            throw new Exception("Tạo mới Đại lý thất bại.");
        }

        // 4. UPDATE
        public async Task<bool> CapNhatDaiLy(int maDaiLy, DaiLyTaoMoi model)
        {
            // Kiểm tra đại lý có tồn tại không
            var existing = await _repository.GetByIdAsync(maDaiLy);
            if (existing == null)
            {
                throw new Exception($"Không tìm thấy đại lý có mã {maDaiLy}");
            }

            // Ánh xạ DTO sang Entity
            var entity = new DaiLy
            {
                TenDaiLy = model.TenDaiLy,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                DiaChi = model.DiaChi
            };

            return await _repository.UpdateAsync(maDaiLy, entity);
        }

        // 5. DELETE
        public async Task<bool> XoaDaiLy(int maDaiLy)
        {
            // Kiểm tra đại lý có tồn tại không
            var existing = await _repository.GetByIdAsync(maDaiLy);
            if (existing == null)
            {
                throw new Exception($"Không tìm thấy đại lý có mã {maDaiLy}");
            }

            return await _repository.DeleteAsync(maDaiLy);
        }

        // 6. SEARCH
        public async Task<List<DaiLyPhanHoi>> TimKiemDaiLy(string? tenDaiLy, string? soDienThoai)
        {
            return await _repository.SearchAsync(tenDaiLy, soDienThoai);
        }
    }
}