using Microsoft.Data.SqlClient;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Data
{
    public class KiemDinhRepository : IKiemDinhRepository
    {
        private readonly string _connectionString;

        public KiemDinhRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<KiemDinhDTO> GetAll()
        {
            var list = new List<KiemDinhDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM KiemDinh", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public KiemDinhDTO? GetById(int maKiemDinh)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM KiemDinh WHERE MaKiemDinh = @MaKiemDinh", conn);

            cmd.Parameters.AddWithValue("@MaKiemDinh", maKiemDinh);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return MapToDto(reader);
        }

        public List<KiemDinhDTO> GetByMaDaiLy(int maDaiLy)
        {
            var list = new List<KiemDinhDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT * FROM KiemDinh WHERE MaDaiLy = @MaDaiLy", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public int Create(KiemDinhCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO KiemDinh (MaLo, NguoiKiemDinh, MaDaiLy, MaSieuThi, KetQua, BienBan, ChuKySo, GhiChu)
                OUTPUT INSERTED.MaKiemDinh
                VALUES (@MaLo, @NguoiKiemDinh, @MaDaiLy, @MaSieuThi, @KetQua, @BienBan, @ChuKySo, @GhiChu)", conn);

            cmd.Parameters.AddWithValue("@MaLo", dto.MaLo);
            cmd.Parameters.AddWithValue("@NguoiKiemDinh", (object?)dto.NguoiKiemDinh ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaDaiLy", (object?)dto.MaDaiLy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaSieuThi", (object?)dto.MaSieuThi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KetQua", dto.KetQua);
            cmd.Parameters.AddWithValue("@BienBan", (object?)dto.BienBan ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ChuKySo", (object?)dto.ChuKySo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public bool Update(int maKiemDinh, KiemDinhUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE KiemDinh
                SET NguoiKiemDinh = @NguoiKiemDinh,
                    KetQua = @KetQua,
                    TrangThai = @TrangThai,
                    BienBan = @BienBan,
                    ChuKySo = @ChuKySo,
                    GhiChu = @GhiChu
                WHERE MaKiemDinh = @MaKiemDinh", conn);

            cmd.Parameters.AddWithValue("@MaKiemDinh", maKiemDinh);
            cmd.Parameters.AddWithValue("@NguoiKiemDinh", (object?)dto.NguoiKiemDinh ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KetQua", dto.KetQua);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)dto.TrangThai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BienBan", (object?)dto.BienBan ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ChuKySo", (object?)dto.ChuKySo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int maKiemDinh)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM KiemDinh WHERE MaKiemDinh = @MaKiemDinh", conn);

            cmd.Parameters.AddWithValue("@MaKiemDinh", maKiemDinh);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        private KiemDinhDTO MapToDto(SqlDataReader reader)
        {
            return new KiemDinhDTO
            {
                MaKiemDinh = (int)reader["MaKiemDinh"],
                MaLo = (int)reader["MaLo"],
                NguoiKiemDinh = reader["NguoiKiemDinh"] as string,
                MaDaiLy = reader["MaDaiLy"] as int?,
                MaSieuThi = reader["MaSieuThi"] as int?,
                NgayKiemDinh = reader["NgayKiemDinh"] as DateTime?,
                KetQua = reader["KetQua"].ToString()!,
                TrangThai = reader["TrangThai"] as string,
                BienBan = reader["BienBan"] as string,
                ChuKySo = reader["ChuKySo"] as string,
                GhiChu = reader["GhiChu"] as string
            };
        }
    }
}
