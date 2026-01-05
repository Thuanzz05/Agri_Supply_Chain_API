using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;
using System.Data;

namespace NongDanService.Data
{
    public class ChiTietDonHangRepository : IChiTietDonHangRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ChiTietDonHangRepository> _logger;

        public ChiTietDonHangRepository(IConfiguration config, ILogger<ChiTietDonHangRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public List<ChiTietDonHangDTO> GetByDonHangId(int maDonHang)
        {
            var list = new List<ChiTietDonHangDTO>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT ct.MaDonHang, ct.MaLo, ct.SoLuong, ct.DonGia, ct.ThanhTien,
                           sp.TenSanPham, lo.MaQR, tt.TenTrangTrai
                    FROM ChiTietDonHang ct
                    INNER JOIN LoNongSan lo ON ct.MaLo = lo.MaLo
                    INNER JOIN SanPham sp ON lo.MaSanPham = sp.MaSanPham
                    INNER JOIN TrangTrai tt ON lo.MaTrangTrai = tt.MaTrangTrai
                    WHERE ct.MaDonHang = @MaDonHang", conn);

                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting order details for order {Id}", maDonHang);
                throw;
            }
            return list;
        }

        public ChiTietDonHangDTO? GetById(int maDonHang, int maLo)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    SELECT ct.MaDonHang, ct.MaLo, ct.SoLuong, ct.DonGia, ct.ThanhTien,
                           sp.TenSanPham, lo.MaQR, tt.TenTrangTrai
                    FROM ChiTietDonHang ct
                    INNER JOIN LoNongSan lo ON ct.MaLo = lo.MaLo
                    INNER JOIN SanPham sp ON lo.MaSanPham = sp.MaSanPham
                    INNER JOIN TrangTrai tt ON lo.MaTrangTrai = tt.MaTrangTrai
                    WHERE ct.MaDonHang = @MaDonHang AND ct.MaLo = @MaLo", conn);

                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;
                cmd.Parameters.Add("@MaLo", SqlDbType.Int).Value = maLo;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapToDTO(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting order detail");
                throw;
            }
        }

        public bool Create(ChiTietDonHangCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                decimal thanhTien = dto.SoLuong * dto.DonGia;

                // Insert chi tiết
                using var cmd = new SqlCommand(@"
                    INSERT INTO ChiTietDonHang (MaDonHang, MaLo, SoLuong, DonGia, ThanhTien)
                    VALUES (@MaDonHang, @MaLo, @SoLuong, @DonGia, @ThanhTien)", conn, transaction);

                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = dto.MaDonHang;
                cmd.Parameters.Add("@MaLo", SqlDbType.Int).Value = dto.MaLo;
                cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = dto.SoLuong;
                cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = dto.DonGia;
                cmd.Parameters.Add("@ThanhTien", SqlDbType.Decimal).Value = thanhTien;
                cmd.ExecuteNonQuery();

                // Cập nhật tổng trong DonHang
                UpdateDonHangTotals(dto.MaDonHang, conn, transaction);

                transaction.Commit();
                _logger.LogInformation("Created order detail for order {OrderId}, batch {BatchId}", dto.MaDonHang, dto.MaLo);
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error creating order detail");
                throw;
            }
        }

        public bool Update(int maDonHang, int maLo, ChiTietDonHangUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Lấy giá trị hiện tại
                decimal soLuong = dto.SoLuong ?? 0;
                decimal donGia = dto.DonGia ?? 0;

                if (!dto.SoLuong.HasValue || !dto.DonGia.HasValue)
                {
                    using var cmdGet = new SqlCommand(
                        "SELECT SoLuong, DonGia FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang AND MaLo = @MaLo", conn, transaction);
                    cmdGet.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;
                    cmdGet.Parameters.Add("@MaLo", SqlDbType.Int).Value = maLo;
                    using var reader = cmdGet.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!dto.SoLuong.HasValue) soLuong = reader.GetDecimal(0);
                        if (!dto.DonGia.HasValue) donGia = reader.GetDecimal(1);
                    }
                    reader.Close();
                }

                if (dto.SoLuong.HasValue) soLuong = dto.SoLuong.Value;
                if (dto.DonGia.HasValue) donGia = dto.DonGia.Value;

                decimal thanhTien = soLuong * donGia;

                // Update chi tiết
                using var cmd = new SqlCommand(@"
                    UPDATE ChiTietDonHang 
                    SET SoLuong = @SoLuong, DonGia = @DonGia, ThanhTien = @ThanhTien
                    WHERE MaDonHang = @MaDonHang AND MaLo = @MaLo", conn, transaction);

                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;
                cmd.Parameters.Add("@MaLo", SqlDbType.Int).Value = maLo;
                cmd.Parameters.Add("@SoLuong", SqlDbType.Decimal).Value = soLuong;
                cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = donGia;
                cmd.Parameters.Add("@ThanhTien", SqlDbType.Decimal).Value = thanhTien;

                var result = cmd.ExecuteNonQuery() > 0;

                // Cập nhật tổng trong DonHang
                UpdateDonHangTotals(maDonHang, conn, transaction);

                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error updating order detail");
                throw;
            }
        }

        public bool Delete(int maDonHang, int maLo)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = new SqlCommand(
                    "DELETE FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang AND MaLo = @MaLo", conn, transaction);
                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;
                cmd.Parameters.Add("@MaLo", SqlDbType.Int).Value = maLo;

                var result = cmd.ExecuteNonQuery() > 0;

                // Cập nhật tổng trong DonHang
                UpdateDonHangTotals(maDonHang, conn, transaction);

                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error deleting order detail");
                throw;
            }
        }

        public bool DeleteByDonHang(int maDonHang)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(
                    "DELETE FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang", conn);
                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error deleting all order details for order {Id}", maDonHang);
                throw;
            }
        }

        private void UpdateDonHangTotals(int maDonHang, SqlConnection conn, SqlTransaction transaction)
        {
            using var cmd = new SqlCommand(@"
                UPDATE DonHang 
                SET TongSoLuong = ISNULL((SELECT SUM(SoLuong) FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang), 0),
                    TongGiaTri = ISNULL((SELECT SUM(ThanhTien) FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang), 0)
                WHERE MaDonHang = @MaDonHang", conn, transaction);
            cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = maDonHang;
            cmd.ExecuteNonQuery();
        }

        private static ChiTietDonHangDTO MapToDTO(SqlDataReader reader)
        {
            return new ChiTietDonHangDTO
            {
                MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                MaLo = reader.GetInt32(reader.GetOrdinal("MaLo")),
                SoLuong = reader.GetDecimal(reader.GetOrdinal("SoLuong")),
                DonGia = reader.GetDecimal(reader.GetOrdinal("DonGia")),
                ThanhTien = reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                TenSanPham = reader.IsDBNull(reader.GetOrdinal("TenSanPham")) ? null : reader.GetString(reader.GetOrdinal("TenSanPham")),
                MaQR = reader.IsDBNull(reader.GetOrdinal("MaQR")) ? null : reader.GetString(reader.GetOrdinal("MaQR")),
                TenTrangTrai = reader.IsDBNull(reader.GetOrdinal("TenTrangTrai")) ? null : reader.GetString(reader.GetOrdinal("TenTrangTrai"))
            };
        }
    }
}
