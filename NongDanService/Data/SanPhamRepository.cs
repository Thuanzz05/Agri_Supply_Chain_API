using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;

namespace NongDanService.Data
{
    public class SanPhamRepository : ISanPhamRepository
    {
        private readonly string _connectionString;

        public SanPhamRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public List<SanPhamDTO> GetAll()
        {
            var list = new List<SanPhamDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM SanPham", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new SanPhamDTO
                {
                    MaSanPham = (int)reader["MaSanPham"],
                    TenSanPham = reader["TenSanPham"].ToString()!,
                    DonViTinh = reader["DonViTinh"].ToString()!,
                    MoTa = reader["MoTa"] as string,
                    NgayTao = reader["NgayTao"] as DateTime?
                });
            }

            return list;
        }

        public SanPhamDTO? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM SanPham WHERE MaSanPham=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new SanPhamDTO
            {
                MaSanPham = (int)reader["MaSanPham"],
                TenSanPham = reader["TenSanPham"].ToString()!,
                DonViTinh = reader["DonViTinh"].ToString()!,
                MoTa = reader["MoTa"] as string,
                NgayTao = reader["NgayTao"] as DateTime?
            };
        }

        public int Create(SanPhamCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO SanPham (TenSanPham, DonViTinh, MoTa)
                OUTPUT INSERTED.MaSanPham
                VALUES (@Ten, @DonVi, @MoTa)", conn);

            cmd.Parameters.AddWithValue("@Ten", dto.TenSanPham);
            cmd.Parameters.AddWithValue("@DonVi", dto.DonViTinh);
            cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int id, SanPhamUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE SanPham
                SET TenSanPham=@Ten, DonViTinh=@DonVi, MoTa=@MoTa
                WHERE MaSanPham=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Ten", dto.TenSanPham);
            cmd.Parameters.AddWithValue("@DonVi", dto.DonViTinh);
            cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM SanPham WHERE MaSanPham=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
