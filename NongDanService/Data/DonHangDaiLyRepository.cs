using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;
using System.Data;

namespace NongDanService.Data
{
    public class DonHangDaiLyRepository : IDonHangDaiLyRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<DonHangDaiLyRepository> _logger;

        public DonHangDaiLyRepository(IConfiguration config, ILogger<DonHangDaiLyRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public List<DonHangDaiLyDTO> GetAll()
        {
            var list = new List<DonHangDaiLyDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT d.MaDonHang, d.MaNongDan, d.MaDaiLy, d.MaLo, d.SoLuong, d.DonGia, d.TongTien,
                           d.TrangThai, d.NgayTao, d.GhiChu,
                           n.HoTen AS TenNongDan,
                           l.MaQR, s.TenSanPham
                    FROM DonHangDaiLy d
                    LEFT JOIN NongDan n ON d.MaNongDan = n.MaNongDan
                    LEFT JOIN LoNongSan l ON d.MaLo = l.MaLo
                    LEFT JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    ORDER BY d.NgayTao DESC", conn);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} orders from database", list.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting all orders");
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public DonHangDaiLyDTO? GetById(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT d.MaDonHang, d.MaNongDan, d.MaDaiLy, d.MaLo, d.SoLuong, d.DonGia, d.TongTien,
                           d.TrangThai, d.NgayTao, d.GhiChu,
                           n.HoTen AS TenNongDan,
                           l.MaQR, s.TenSanPham
                    FROM DonHangDaiLy d
                    LEFT JOIN NongDan n ON d.MaNongDan = n.MaNongDan
                    LEFT JOIN LoNongSan l ON d.MaLo = l.MaLo
                    LEFT JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE d.MaDonHang = @id", conn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", id);
                    return null;
                }

                return MapToDTO(reader);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting order with ID {OrderId}", id);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }
        }

        public List<DonHangDaiLyDTO> GetByNongDanId(int maNongDan)
        {
            var list = new List<DonHangDaiLyDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT d.MaDonHang, d.MaNongDan, d.MaDaiLy, d.MaLo, d.SoLuong, d.DonGia, d.TongTien,
                           d.TrangThai, d.NgayTao, d.GhiChu,
                           n.HoTen AS TenNongDan,
                           l.MaQR, s.TenSanPham
                    FROM DonHangDaiLy d
                    LEFT JOIN NongDan n ON d.MaNongDan = n.MaNongDan
                    LEFT JOIN LoNongSan l ON d.MaLo = l.MaLo
                    LEFT JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE d.MaNongDan = @maNongDan
                    ORDER BY d.NgayTao DESC", conn);

                cmd.Parameters.Add("@maNongDan", SqlDbType.Int).Value = maNongDan;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} orders for farmer ID {FarmerId}", list.Count, maNongDan);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting orders for farmer ID {FarmerId}", maNongDan);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public List<DonHangDaiLyDTO> GetByDaiLyId(int maDaiLy)
        {
            var list = new List<DonHangDaiLyDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT d.MaDonHang, d.MaNongDan, d.MaDaiLy, d.MaLo, d.SoLuong, d.DonGia, d.TongTien,
                           d.TrangThai, d.NgayTao, d.GhiChu,
                           n.HoTen AS TenNongDan,
                           l.MaQR, s.TenSanPham
                    FROM DonHangDaiLy d
                    LEFT JOIN NongDan n ON d.MaNongDan = n.MaNongDan
                    LEFT JOIN LoNongSan l ON d.MaLo = l.MaLo
                    LEFT JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE d.MaDaiLy = @maDaiLy
                    ORDER BY d.NgayTao DESC", conn);

                cmd.Parameters.Add("@maDaiLy", SqlDbType.Int).Value = maDaiLy;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} orders for agency ID {AgencyId}", list.Count, maDaiLy);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting orders for agency ID {AgencyId}", maDaiLy);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public int Create(DonHangDaiLyCreateDTO dto)
        {
            try
            {
                decimal tongTien = dto.SoLuong * (dto.DonGia ?? 0);

                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    INSERT INTO DonHangDaiLy (MaNongDan, MaDaiLy, MaLo, SoLuong, DonGia, TongTien, TrangThai, NgayTao, GhiChu)
                    OUTPUT INSERTED.MaDonHang
                    VALUES (@MaNongDan, @MaDaiLy, @MaLo, @SoLuong, @DonGia, @TongTien, @TrangThai, GETDATE(), @GhiChu)", conn);

                cmd.Parameters.Add("@MaNongDan", SqlDbType.Int).Value = dto.MaNongDan;
                cmd.Parameters.Add("@MaDaiLy", SqlDbType.Int).Value = (object?)dto.MaDaiLy ?? DBNull.Value;
                cmd.Parameters.Add("@MaLo", SqlDbType.Int).Value = dto.MaLo;
                cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = dto.SoLuong;
                cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = (object?)dto.DonGia ?? DBNull.Value;
                cmd.Parameters.Add("@TongTien", SqlDbType.Decimal).Value = tongTien;
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = "cho_xu_ly";
                cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (object?)dto.GhiChu ?? DBNull.Value;

                conn.Open();
                var newId = (int)cmd.ExecuteScalar()!;

                _logger.LogInformation("Created new order with ID {OrderId}", newId);
                return newId;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while creating order: {@Order}", dto);

                if (ex.Number == 547)
                {
                    throw new Exception("Mã nông dân hoặc mã lô không tồn tại trong hệ thống", ex);
                }

                throw new Exception("Lỗi tạo đơn hàng trong cơ sở dữ liệu", ex);
            }
        }

        public bool Update(int id, DonHangDaiLyUpdateDTO dto)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                
                var updates = new List<string>();
                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (dto.SoLuong.HasValue)
                {
                    updates.Add("SoLuong = @SoLuong");
                    cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = dto.SoLuong.Value;
                }

                if (dto.DonGia.HasValue)
                {
                    updates.Add("DonGia = @DonGia");
                    cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = dto.DonGia.Value;
                }

                if (dto.TrangThai != null)
                {
                    updates.Add("TrangThai = @TrangThai");
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = dto.TrangThai;
                }

                if (dto.GhiChu != null)
                {
                    updates.Add("GhiChu = @GhiChu");
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = dto.GhiChu;
                }

                // Recalculate TongTien if SoLuong or DonGia changed
                if (dto.SoLuong.HasValue || dto.DonGia.HasValue)
                {
                    updates.Add("TongTien = ISNULL(@SoLuong, SoLuong) * ISNULL(@DonGia, DonGia)");
                }

                if (updates.Count == 0)
                {
                    return true;
                }

                cmd.CommandText = $"UPDATE DonHangDaiLy SET {string.Join(", ", updates)} WHERE MaDonHang = @Id";
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Updated order with ID {OrderId}", id);
                    return true;
                }

                _logger.LogWarning("No order found with ID {OrderId} to update", id);
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating order with ID {OrderId}", id);
                throw new Exception("Lỗi cập nhật đơn hàng trong cơ sở dữ liệu", ex);
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(
                    "UPDATE DonHangDaiLy SET TrangThai = @TrangThai WHERE MaDonHang = @Id", conn);

                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Updated status of order {OrderId} to {Status}", id, trangThai);
                    return true;
                }

                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating order status");
                throw new Exception("Lỗi cập nhật trạng thái đơn hàng", ex);
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("DELETE FROM DonHangDaiLy WHERE MaDonHang = @id", conn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Deleted order with ID {OrderId}", id);
                    return true;
                }

                _logger.LogWarning("No order found with ID {OrderId} to delete", id);
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting order with ID {OrderId}", id);
                throw new Exception("Lỗi xóa đơn hàng trong cơ sở dữ liệu", ex);
            }
        }

        private static DonHangDaiLyDTO MapToDTO(SqlDataReader reader)
        {
            return new DonHangDaiLyDTO
            {
                MaDonHang = reader.GetInt32("MaDonHang"),
                MaNongDan = reader.GetInt32("MaNongDan"),
                MaDaiLy = reader.IsDBNull("MaDaiLy") ? null : reader.GetInt32("MaDaiLy"),
                MaLo = reader.IsDBNull("MaLo") ? null : reader.GetInt32("MaLo"),
                SoLuong = reader.IsDBNull("SoLuong") ? null : reader.GetDecimal("SoLuong"),
                DonGia = reader.IsDBNull("DonGia") ? null : reader.GetDecimal("DonGia"),
                TongTien = reader.IsDBNull("TongTien") ? null : reader.GetDecimal("TongTien"),
                TrangThai = reader.IsDBNull("TrangThai") ? null : reader.GetString("TrangThai"),
                NgayTao = reader.IsDBNull("NgayTao") ? null : reader.GetDateTime("NgayTao"),
                GhiChu = reader.IsDBNull("GhiChu") ? null : reader.GetString("GhiChu"),
                TenNongDan = reader.IsDBNull("TenNongDan") ? null : reader.GetString("TenNongDan"),
                TenSanPham = reader.IsDBNull("TenSanPham") ? null : reader.GetString("TenSanPham"),
                MaQR = reader.IsDBNull("MaQR") ? null : reader.GetString("MaQR")
            };
        }
    }
}