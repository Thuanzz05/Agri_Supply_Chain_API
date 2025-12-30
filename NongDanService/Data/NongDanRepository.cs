using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;

namespace NongDanService.Data
{
    public class NongDanRepository : INongDanRepository
    {
        private readonly string _connectionString;

        public NongDanRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public List<NongDanDTO> GetAll()
        {
            var list = new List<NongDanDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM NongDan", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new NongDanDTO
                {
                    MaNongDan = (int)reader["MaNongDan"],
                    MaTaiKhoan = (int)reader["MaTaiKhoan"],
                    HoTen = reader["HoTen"] as string,
                    SoDienThoai = reader["SoDienThoai"] as string,
                    Email = reader["Email"] as string,
                    DiaChi = reader["DiaChi"] as string
                });
            }

            return list;
        }

        public NongDanDTO? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM NongDan WHERE MaNongDan=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new NongDanDTO
            {
                MaNongDan = (int)reader["MaNongDan"],
                MaTaiKhoan = (int)reader["MaTaiKhoan"],
                HoTen = reader["HoTen"] as string,
                SoDienThoai = reader["SoDienThoai"] as string,
                Email = reader["Email"] as string,
                DiaChi = reader["DiaChi"] as string
            };
        }

        public int Create(NongDanCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO NongDan (MaTaiKhoan, HoTen, SoDienThoai, Email, DiaChi)
                OUTPUT INSERTED.MaNongDan
                VALUES (@MaTaiKhoan, @HoTen, @SoDienThoai, @Email, @DiaChi)", conn);

            cmd.Parameters.AddWithValue("@MaTaiKhoan", dto.MaTaiKhoan);
            cmd.Parameters.AddWithValue("@HoTen", (object?)dto.HoTen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SoDienThoai", (object?)dto.SoDienThoai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);

            conn.Open();
            return (int)cmd.ExecuteScalar()!;
        }

        public bool Update(int id, NongDanUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE NongDan
                SET HoTen=@HoTen, SoDienThoai=@SoDienThoai, Email=@Email, DiaChi=@DiaChi
                WHERE MaNongDan=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@HoTen", (object?)dto.HoTen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SoDienThoai", (object?)dto.SoDienThoai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM NongDan WHERE MaNongDan=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
