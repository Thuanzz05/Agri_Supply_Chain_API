using Microsoft.Data.SqlClient;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Data
{
    public class DaiLyRepository : IDaiLyRepository
    {
        private readonly string _connectionString;

        public DaiLyRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<DaiLyPhanHoi> GetAll()
        {
            var list = new List<DaiLyPhanHoi>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT d.MaDaiLy, d.TenDaiLy, d.SoDienThoai, d.Email, d.DiaChi,
                       t.TenDangNhap, t.TrangThai
                FROM DaiLy d
                LEFT JOIN TaiKhoan t ON d.MaTaiKhoan = t.MaTaiKhoan", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public DaiLyPhanHoi? GetById(int maDaiLy)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT d.MaDaiLy, d.TenDaiLy, d.SoDienThoai, d.Email, d.DiaChi,
                       t.TenDangNhap, t.TrangThai
                FROM DaiLy d
                LEFT JOIN TaiKhoan t ON d.MaTaiKhoan = t.MaTaiKhoan
                WHERE d.MaDaiLy = @MaDaiLy", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return MapToDto(reader);
        }

        public int Create(DaiLyTaoMoi dto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Tạo TaiKhoan trước
                using var cmdTaiKhoan = new SqlCommand(@"
                    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, LoaiTaiKhoan)
                    OUTPUT INSERTED.MaTaiKhoan
                    VALUES (@TenDangNhap, @MatKhau, 'daily')", conn, transaction);

                cmdTaiKhoan.Parameters.AddWithValue("@TenDangNhap", dto.TenDangNhap);
                cmdTaiKhoan.Parameters.AddWithValue("@MatKhau", dto.MatKhau);

                int maTaiKhoan = (int)cmdTaiKhoan.ExecuteScalar();

                // 2. Tạo DaiLy với MaTaiKhoan vừa tạo
                using var cmdDaiLy = new SqlCommand(@"
                    INSERT INTO DaiLy (MaTaiKhoan, TenDaiLy, SoDienThoai, Email, DiaChi)
                    OUTPUT INSERTED.MaDaiLy
                    VALUES (@MaTaiKhoan, @TenDaiLy, @SoDienThoai, @Email, @DiaChi)", conn, transaction);

                cmdDaiLy.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                cmdDaiLy.Parameters.AddWithValue("@TenDaiLy", dto.TenDaiLy);
                cmdDaiLy.Parameters.AddWithValue("@SoDienThoai", (object?)dto.SoDienThoai ?? DBNull.Value);
                cmdDaiLy.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
                cmdDaiLy.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);

                int maDaiLy = (int)cmdDaiLy.ExecuteScalar();

                transaction.Commit();
                return maDaiLy;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Update(int maDaiLy, DaiLyUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE DaiLy
                SET TenDaiLy = @TenDaiLy,
                    SoDienThoai = @SoDienThoai,
                    Email = @Email,
                    DiaChi = @DiaChi
                WHERE MaDaiLy = @MaDaiLy", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);
            cmd.Parameters.AddWithValue("@TenDaiLy", dto.TenDaiLy);
            cmd.Parameters.AddWithValue("@SoDienThoai", (object?)dto.SoDienThoai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int maDaiLy)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM DaiLy WHERE MaDaiLy = @MaDaiLy", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<DaiLyPhanHoi> Search(string? tenDaiLy, string? soDienThoai)
        {
            var list = new List<DaiLyPhanHoi>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT d.MaDaiLy, d.TenDaiLy, d.SoDienThoai, d.Email, d.DiaChi,
                       t.TenDangNhap, t.TrangThai
                FROM DaiLy d
                LEFT JOIN TaiKhoan t ON d.MaTaiKhoan = t.MaTaiKhoan
                WHERE (@TenDaiLy IS NULL OR d.TenDaiLy LIKE '%' + @TenDaiLy + '%')
                  AND (@SoDienThoai IS NULL OR d.SoDienThoai LIKE '%' + @SoDienThoai + '%')", conn);

            cmd.Parameters.AddWithValue("@TenDaiLy", (object?)tenDaiLy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SoDienThoai", (object?)soDienThoai ?? DBNull.Value);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        private DaiLyPhanHoi MapToDto(SqlDataReader reader)
        {
            return new DaiLyPhanHoi
            {
                MaDaiLy = (int)reader["MaDaiLy"],
                TenDaiLy = reader["TenDaiLy"] as string,
                SoDienThoai = reader["SoDienThoai"] as string,
                Email = reader["Email"] as string,
                DiaChi = reader["DiaChi"] as string,
                TenDangNhap = reader["TenDangNhap"] as string,
                TrangThai = reader["TrangThai"] as string
            };
        }
    }
}
