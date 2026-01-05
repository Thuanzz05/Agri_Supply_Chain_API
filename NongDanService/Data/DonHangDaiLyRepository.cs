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
                using var cmd = new SqlCommand("sp_DonHangDaiLy_GetAll", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting all orders");
                throw;
            }
            return list;
        }

        public DonHangDaiLyDTO? GetById(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_GetById", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = id;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapToDTO(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting order by ID {Id}", id);
                throw;
            }
        }

        public List<DonHangDaiLyDTO> GetByNongDanId(int maNongDan)
        {
            var list = new List<DonHangDaiLyDTO>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_GetByNongDan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MaNongDan", SqlDbType.Int).Value = maNongDan;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting orders by farmer ID {Id}", maNongDan);
                throw;
            }
            return list;
        }

        public List<DonHangDaiLyDTO> GetByDaiLyId(int maDaiLy)
        {
            var list = new List<DonHangDaiLyDTO>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_GetByDaiLy", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MaDaiLy", SqlDbType.Int).Value = maDaiLy;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting orders by agency ID {Id}", maDaiLy);
                throw;
            }
            return list;
        }

        public int Create(DonHangDaiLyCreateDTO dto)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_Create", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MaDaiLy", SqlDbType.Int).Value = dto.MaDaiLy;
                cmd.Parameters.Add("@MaNongDan", SqlDbType.Int).Value = dto.MaNongDan;
                cmd.Parameters.Add("@LoaiDon", SqlDbType.NVarChar, 50).Value = (object?)dto.LoaiDon ?? "dai_ly";
                cmd.Parameters.Add("@NgayGiao", SqlDbType.DateTime).Value = (object?)dto.NgayGiao ?? DBNull.Value;
                cmd.Parameters.Add("@TongSoLuong", SqlDbType.Decimal).Value = (object?)dto.TongSoLuong ?? DBNull.Value;
                cmd.Parameters.Add("@TongGiaTri", SqlDbType.Decimal).Value = (object?)dto.TongGiaTri ?? DBNull.Value;
                cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (object?)dto.GhiChu ?? DBNull.Value;
                
                var outputParam = cmd.Parameters.Add("@MaDonHang", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
                
                var maDonHang = (int)outputParam.Value;
                _logger.LogInformation("Created order with ID {Id}", maDonHang);
                return maDonHang;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        public bool Update(int id, DonHangDaiLyUpdateDTO dto)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var updates = new List<string>();
                var cmd = new SqlCommand { Connection = conn };

                if (dto.LoaiDon != null)
                {
                    updates.Add("LoaiDon = @LoaiDon");
                    cmd.Parameters.Add("@LoaiDon", SqlDbType.NVarChar, 50).Value = dto.LoaiDon;
                }
                if (dto.NgayGiao.HasValue)
                {
                    updates.Add("NgayGiao = @NgayGiao");
                    cmd.Parameters.Add("@NgayGiao", SqlDbType.DateTime).Value = dto.NgayGiao.Value;
                }
                if (dto.TrangThai != null)
                {
                    updates.Add("TrangThai = @TrangThai");
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = dto.TrangThai;
                }
                if (dto.TongSoLuong.HasValue)
                {
                    updates.Add("TongSoLuong = @TongSoLuong");
                    cmd.Parameters.Add("@TongSoLuong", SqlDbType.Decimal).Value = dto.TongSoLuong.Value;
                }
                if (dto.TongGiaTri.HasValue)
                {
                    updates.Add("TongGiaTri = @TongGiaTri");
                    cmd.Parameters.Add("@TongGiaTri", SqlDbType.Decimal).Value = dto.TongGiaTri.Value;
                }
                if (dto.GhiChu != null)
                {
                    updates.Add("GhiChu = @GhiChu");
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = dto.GhiChu;
                }

                if (updates.Count == 0) return true;

                cmd.CommandText = $"UPDATE DonHang SET {string.Join(", ", updates)} WHERE MaDonHang = @Id";
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error updating order {Id}", id);
                throw;
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_UpdateTrangThai", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader.GetInt32(0) > 0;
                }
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error updating order status");
                throw;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DonHangDaiLy_Delete", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MaDonHang", SqlDbType.Int).Value = id;

                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader.GetInt32(0) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {Id}", id);
                throw;
            }
        }

        private static DonHangDaiLyDTO MapToDTO(SqlDataReader reader)
        {
            return new DonHangDaiLyDTO
            {
                MaDonHang = reader.GetInt32(reader.GetOrdinal("MaDonHang")),
                MaDaiLy = reader.IsDBNull(reader.GetOrdinal("MaDaiLy")) ? null : reader.GetInt32(reader.GetOrdinal("MaDaiLy")),
                MaNongDan = reader.IsDBNull(reader.GetOrdinal("MaNongDan")) ? null : reader.GetInt32(reader.GetOrdinal("MaNongDan")),
                LoaiDon = reader.IsDBNull(reader.GetOrdinal("LoaiDon")) ? null : reader.GetString(reader.GetOrdinal("LoaiDon")),
                NgayDat = reader.IsDBNull(reader.GetOrdinal("NgayDat")) ? null : reader.GetDateTime(reader.GetOrdinal("NgayDat")),
                NgayGiao = reader.IsDBNull(reader.GetOrdinal("NgayGiao")) ? null : reader.GetDateTime(reader.GetOrdinal("NgayGiao")),
                TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? null : reader.GetString(reader.GetOrdinal("TrangThai")),
                TongSoLuong = reader.IsDBNull(reader.GetOrdinal("TongSoLuong")) ? null : reader.GetDecimal(reader.GetOrdinal("TongSoLuong")),
                TongGiaTri = reader.IsDBNull(reader.GetOrdinal("TongGiaTri")) ? null : reader.GetDecimal(reader.GetOrdinal("TongGiaTri")),
                GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
                TenNongDan = reader.IsDBNull(reader.GetOrdinal("TenNongDan")) ? null : reader.GetString(reader.GetOrdinal("TenNongDan")),
                TenDaiLy = reader.IsDBNull(reader.GetOrdinal("TenDaiLy")) ? null : reader.GetString(reader.GetOrdinal("TenDaiLy"))
            };
        }
    }
}
