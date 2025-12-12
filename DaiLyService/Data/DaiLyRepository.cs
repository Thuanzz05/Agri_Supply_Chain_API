using DaiLyService.Models.Entities;
using DaiLyService.Models.DTOs;
using System.Data;
using DbHelper;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace DaiLyService.Data
{
    public class DaiLyRepository : IDaiLyRepository
    {
        // SỬA: Dùng ILegacyDbHelper để gọi hàm đồng bộ ExecuteSProcedureReturnDataTable
        private readonly ILegacyDbHelper _dbHelper;

        // Khai báo constructor mới
        public DaiLyRepository(ILegacyDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Ánh xạ DataTable sang DTO (Giữ nguyên)
        private List<DaiLyPhanHoi> MapDataTableToDaiLyPhanHoi(DataTable? table)
        {
            var results = new List<DaiLyPhanHoi>();
            if (table == null || table.Rows.Count == 0) return results;

            foreach (DataRow row in table.Rows)
            {
                results.Add(new DaiLyPhanHoi
                {
                    MaDaiLy = row["MaDaiLy"] != DBNull.Value ? Convert.ToInt32(row["MaDaiLy"]) : 0,
                    TenDaiLy = row["TenDaiLy"] != DBNull.Value ? row["TenDaiLy"].ToString() : null,
                    SoDienThoai = row["SoDienThoai"] != DBNull.Value ? row["SoDienThoai"].ToString() : null,
                    Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                    DiaChi = row["DiaChi"] != DBNull.Value ? row["DiaChi"].ToString() : null,
                    TenDangNhap = row["TenDangNhap"] != DBNull.Value ? row["TenDangNhap"].ToString() : null,
                    TrangThai = row["TrangThai"] != DBNull.Value ? row["TrangThai"].ToString() : null
                });
            }
            return results;
        }

        // Helper async wrapper
        private Task<DataTable> ExecuteSProcedureReturnDataTableAsync(
            string sprocedureName,
            params object[] paramObjects)
        {
            return Task.Run(() =>
            {
                string msgError = "";

                // GỌI HÀM TRỰC TIẾP TỪ ILegacyDbHelper
                DataTable dt = _dbHelper.ExecuteSProcedureReturnDataTable(out msgError, sprocedureName, paramObjects);

                if (!string.IsNullOrEmpty(msgError))
                {
                    throw new Exception($"Lỗi thực thi SP '{sprocedureName}': {msgError}");
                }

                // Trả về DataTable (có thể là null)
                return dt ?? new DataTable();
            });
        }

        // 1. GET ALL
        public async Task<List<DaiLyPhanHoi>> GetAllAsync()
        {
            DataTable dt = await ExecuteSProcedureReturnDataTableAsync("sp_DaiLy_GetAll");
            return MapDataTableToDaiLyPhanHoi(dt);
        }

        // 2. GET BY ID
        public async Task<DaiLyPhanHoi?> GetByIdAsync(int maDaiLy)
        {
            DataTable dt = await ExecuteSProcedureReturnDataTableAsync(
                "sp_DaiLy_GetById",
                "@MaDaiLy", maDaiLy
            );
            return MapDataTableToDaiLyPhanHoi(dt).FirstOrDefault();
        }

        // 3. CREATE
        public async Task<int> CreateAsync(DaiLyTaoMoi model)
        {
            DataTable dtResult = await ExecuteSProcedureReturnDataTableAsync(
                "sp_DaiLy_Create",
                "@MaTaiKhoan", model.MaTaiKhoan,
                "@TenDaiLy", model.TenDaiLy,
                "@SoDienThoai", model.SoDienThoai,
                "@Email", model.Email,
                "@DiaChi", model.DiaChi
            );

            if (dtResult.Rows.Count > 0)
            {
                var row = dtResult.Rows[0];
                var status = row["Status"] != DBNull.Value ? row["Status"].ToString() : null;

                if (status == "SUCCESS")
                {
                    return row["MaDaiLy"] != DBNull.Value ? Convert.ToInt32(row["MaDaiLy"]) : 0;
                }
                else
                {
                    string message = row["Message"] != DBNull.Value ? row["Message"].ToString() ?? "Lỗi không xác định" : "Lỗi không xác định";
                    throw new Exception($"Lỗi nghiệp vụ khi tạo: {message}");
                }
            }
            throw new Exception("Không nhận được kết quả từ stored procedure");
        }

        // 4. UPDATE
        public async Task<bool> UpdateAsync(int maDaiLy, DaiLy entity)
        {
            DataTable dtResult = await ExecuteSProcedureReturnDataTableAsync(
                "sp_DaiLy_Update",
                "@MaDaiLy", maDaiLy,
                "@TenDaiLy", entity.TenDaiLy,
                "@SoDienThoai", entity.SoDienThoai,
                "@Email", entity.Email,
                "@DiaChi", entity.DiaChi
            );

            if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["Status"].ToString() == "ERROR")
            {
                string message = dtResult.Rows[0]["Message"] != DBNull.Value ? dtResult.Rows[0]["Message"].ToString() ?? "Lỗi không xác định" : "Lỗi không xác định";
                throw new Exception($"Lỗi Repository Update: {message}");
            }
            return true;
        }

        // 5. DELETE
        public async Task<bool> DeleteAsync(int maDaiLy)
        {
            DataTable dtResult = await ExecuteSProcedureReturnDataTableAsync(
                "sp_DaiLy_Delete",
                "@MaDaiLy", maDaiLy
            );

            if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["Status"].ToString() == "ERROR")
            {
                string message = dtResult.Rows[0]["Message"] != DBNull.Value ? dtResult.Rows[0]["Message"].ToString() ?? "Lỗi không xác định" : "Lỗi không xác định";
                throw new Exception($"Lỗi Repository Delete: {message}");
            }
            return true;
        }

        // 6. SEARCH
        public async Task<List<DaiLyPhanHoi>> SearchAsync(string? tenDaiLy, string? soDienThoai)
        {
            DataTable dt = await ExecuteSProcedureReturnDataTableAsync(
                "sp_DaiLy_Search",
                "@TenDaiLy", tenDaiLy,
                "@SoDienThoai", soDienThoai
            );

            return MapDataTableToDaiLyPhanHoi(dt);
        }
    }
}