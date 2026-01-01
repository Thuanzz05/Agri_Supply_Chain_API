using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;
using System.Data;

namespace NongDanService.Data
{
    public class LoNongSanRepository : ILoNongSanRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<LoNongSanRepository> _logger;

        public LoNongSanRepository(IConfiguration config, ILogger<LoNongSanRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public List<LoNongSanDTO> GetAll()
        {
            var list = new List<LoNongSanDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT l.MaLo, l.MaTrangTrai, l.MaSanPham, l.SoLuongBanDau, l.SoLuongHienTai,
                           l.SoChungNhanLo, l.MaQR, l.TrangThai, l.NgayTao,
                           t.TenTrangTrai, s.TenSanPham
                    FROM LoNongSan l
                    INNER JOIN TrangTrai t ON l.MaTrangTrai = t.MaTrangTrai
                    INNER JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    ORDER BY l.NgayTao DESC", conn);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} batches from database", list.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting all batches");
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public LoNongSanDTO? GetById(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT l.MaLo, l.MaTrangTrai, l.MaSanPham, l.SoLuongBanDau, l.SoLuongHienTai,
                           l.SoChungNhanLo, l.MaQR, l.TrangThai, l.NgayTao,
                           t.TenTrangTrai, s.TenSanPham
                    FROM LoNongSan l
                    INNER JOIN TrangTrai t ON l.MaTrangTrai = t.MaTrangTrai
                    INNER JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE l.MaLo = @id", conn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    _logger.LogWarning("Batch with ID {BatchId} not found", id);
                    return null;
                }

                return MapToDTO(reader);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting batch with ID {BatchId}", id);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }
        }

        public List<LoNongSanDTO> GetByTrangTraiId(int maTrangTrai)
        {
            var list = new List<LoNongSanDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT l.MaLo, l.MaTrangTrai, l.MaSanPham, l.SoLuongBanDau, l.SoLuongHienTai,
                           l.SoChungNhanLo, l.MaQR, l.TrangThai, l.NgayTao,
                           t.TenTrangTrai, s.TenSanPham
                    FROM LoNongSan l
                    INNER JOIN TrangTrai t ON l.MaTrangTrai = t.MaTrangTrai
                    INNER JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE l.MaTrangTrai = @maTrangTrai
                    ORDER BY l.NgayTao DESC", conn);

                cmd.Parameters.Add("@maTrangTrai", SqlDbType.Int).Value = maTrangTrai;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} batches for farm ID {FarmId}", list.Count, maTrangTrai);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting batches for farm ID {FarmId}", maTrangTrai);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public List<LoNongSanDTO> GetByNongDanId(int maNongDan)
        {
            var list = new List<LoNongSanDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT l.MaLo, l.MaTrangTrai, l.MaSanPham, l.SoLuongBanDau, l.SoLuongHienTai,
                           l.SoChungNhanLo, l.MaQR, l.TrangThai, l.NgayTao,
                           t.TenTrangTrai, s.TenSanPham
                    FROM LoNongSan l
                    INNER JOIN TrangTrai t ON l.MaTrangTrai = t.MaTrangTrai
                    INNER JOIN SanPham s ON l.MaSanPham = s.MaSanPham
                    WHERE t.MaNongDan = @maNongDan
                    ORDER BY l.NgayTao DESC", conn);

                cmd.Parameters.Add("@maNongDan", SqlDbType.Int).Value = maNongDan;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} batches for farmer ID {FarmerId}", list.Count, maNongDan);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting batches for farmer ID {FarmerId}", maNongDan);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }

            return list;
        }

        public int Create(LoNongSanCreateDTO dto)
        {
            try
            {
                // Tạo mã QR tự động
                string maQR = $"LO-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    INSERT INTO LoNongSan (MaTrangTrai, MaSanPham, SoLuongBanDau, SoLuongHienTai, SoChungNhanLo, MaQR, TrangThai, NgayTao)
                    OUTPUT INSERTED.MaLo
                    VALUES (@MaTrangTrai, @MaSanPham, @SoLuongBanDau, @SoLuongHienTai, @SoChungNhanLo, @MaQR, @TrangThai, GETDATE())", conn);

                cmd.Parameters.Add("@MaTrangTrai", SqlDbType.Int).Value = dto.MaTrangTrai;
                cmd.Parameters.Add("@MaSanPham", SqlDbType.Int).Value = dto.MaSanPham;
                cmd.Parameters.Add("@SoLuongBanDau", SqlDbType.Decimal).Value = dto.SoLuongBanDau;
                cmd.Parameters.Add("@SoLuongHienTai", SqlDbType.Decimal).Value = dto.SoLuongBanDau; // Ban đầu = hiện tại
                cmd.Parameters.Add("@SoChungNhanLo", SqlDbType.NVarChar, 50).Value = (object?)dto.SoChungNhanLo ?? DBNull.Value;
                cmd.Parameters.Add("@MaQR", SqlDbType.NVarChar, 255).Value = maQR;
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = "tai_trang_trai";

                conn.Open();
                var newId = (int)cmd.ExecuteScalar()!;

                _logger.LogInformation("Created new batch with ID {BatchId}, QR: {QRCode}", newId, maQR);
                return newId;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while creating batch: {@Batch}", dto);

                if (ex.Number == 547)
                {
                    throw new Exception("Mã trang trại hoặc mã sản phẩm không tồn tại trong hệ thống", ex);
                }

                throw new Exception("Lỗi tạo lô nông sản trong cơ sở dữ liệu", ex);
            }
        }

        public bool Update(int id, LoNongSanUpdateDTO dto)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                
                // Build dynamic update query
                var updates = new List<string>();
                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (dto.SoLuongHienTai.HasValue)
                {
                    updates.Add("SoLuongHienTai = @SoLuongHienTai");
                    cmd.Parameters.Add("@SoLuongHienTai", SqlDbType.Decimal).Value = dto.SoLuongHienTai.Value;
                }

                if (dto.SoChungNhanLo != null)
                {
                    updates.Add("SoChungNhanLo = @SoChungNhanLo");
                    cmd.Parameters.Add("@SoChungNhanLo", SqlDbType.NVarChar, 50).Value = dto.SoChungNhanLo;
                }

                if (dto.TrangThai != null)
                {
                    updates.Add("TrangThai = @TrangThai");
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = dto.TrangThai;
                }

                if (updates.Count == 0)
                {
                    return true; // Nothing to update
                }

                cmd.CommandText = $"UPDATE LoNongSan SET {string.Join(", ", updates)} WHERE MaLo = @Id";
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Updated batch with ID {BatchId}", id);
                    return true;
                }

                _logger.LogWarning("No batch found with ID {BatchId} to update", id);
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating batch with ID {BatchId}", id);
                throw new Exception("Lỗi cập nhật lô nông sản trong cơ sở dữ liệu", ex);
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("DELETE FROM LoNongSan WHERE MaLo = @id", conn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Deleted batch with ID {BatchId}", id);
                    return true;
                }

                _logger.LogWarning("No batch found with ID {BatchId} to delete", id);
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting batch with ID {BatchId}", id);

                if (ex.Number == 547)
                {
                    throw new Exception("Không thể xóa lô nông sản này vì đang có đơn hàng liên quan", ex);
                }

                throw new Exception("Lỗi xóa lô nông sản trong cơ sở dữ liệu", ex);
            }
        }

        private static LoNongSanDTO MapToDTO(SqlDataReader reader)
        {
            return new LoNongSanDTO
            {
                MaLo = reader.GetInt32("MaLo"),
                MaTrangTrai = reader.GetInt32("MaTrangTrai"),
                MaSanPham = reader.GetInt32("MaSanPham"),
                SoLuongBanDau = reader.GetDecimal("SoLuongBanDau"),
                SoLuongHienTai = reader.GetDecimal("SoLuongHienTai"),
                SoChungNhanLo = reader.IsDBNull("SoChungNhanLo") ? null : reader.GetString("SoChungNhanLo"),
                MaQR = reader.IsDBNull("MaQR") ? null : reader.GetString("MaQR"),
                TrangThai = reader.IsDBNull("TrangThai") ? null : reader.GetString("TrangThai"),
                NgayTao = reader.IsDBNull("NgayTao") ? null : reader.GetDateTime("NgayTao"),
                TenTrangTrai = reader.IsDBNull("TenTrangTrai") ? null : reader.GetString("TenTrangTrai"),
                TenSanPham = reader.IsDBNull("TenSanPham") ? null : reader.GetString("TenSanPham")
            };
        }
    }
}