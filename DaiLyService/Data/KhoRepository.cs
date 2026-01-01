using Microsoft.Data.SqlClient;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Data
{
    public class KhoRepository : IKhoRepository
    {
        private readonly string _connectionString;

        public KhoRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<KhoDTO> GetAll()
        {
            var list = new List<KhoDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Kho", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public KhoDTO? GetById(int maKho)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM Kho WHERE MaKho = @MaKho", conn);

            cmd.Parameters.AddWithValue("@MaKho", maKho);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return MapToDto(reader);
        }

        public List<KhoDTO> GetByMaDaiLy(int maDaiLy)
        {
            var list = new List<KhoDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM Kho WHERE MaDaiLy = @MaDaiLy", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public int Create(KhoCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Kho (LoaiKho, MaDaiLy, MaSieuThi, TenKho, DiaChi)
                OUTPUT INSERTED.MaKho
                VALUES (@LoaiKho, @MaDaiLy, @MaSieuThi, @TenKho, @DiaChi)", conn);

            cmd.Parameters.AddWithValue("@LoaiKho", dto.LoaiKho);
            cmd.Parameters.AddWithValue("@MaDaiLy", (object?)dto.MaDaiLy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaSieuThi", (object?)dto.MaSieuThi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TenKho", dto.TenKho);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int maKho, KhoUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Kho
                SET TenKho = @TenKho,
                    DiaChi = @DiaChi,
                    TrangThai = @TrangThai
                WHERE MaKho = @MaKho", conn);

            cmd.Parameters.AddWithValue("@MaKho", maKho);
            cmd.Parameters.AddWithValue("@TenKho", dto.TenKho);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)dto.TrangThai ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int maKho)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM Kho WHERE MaKho = @MaKho", conn);

            cmd.Parameters.AddWithValue("@MaKho", maKho);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private KhoDTO MapToDto(SqlDataReader reader)
        {
            return new KhoDTO
            {
                MaKho = (int)reader["MaKho"],
                LoaiKho = reader["LoaiKho"].ToString()!,
                MaDaiLy = reader["MaDaiLy"] as int?,
                MaSieuThi = reader["MaSieuThi"] as int?,
                TenKho = reader["TenKho"].ToString()!,
                DiaChi = reader["DiaChi"] as string,
                TrangThai = reader["TrangThai"] as string,
                NgayTao = reader["NgayTao"] as DateTime?
            };
        }
    }
}
