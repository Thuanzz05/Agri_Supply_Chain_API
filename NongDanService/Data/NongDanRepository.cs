using Microsoft.Data.SqlClient;
using NongDanService.Models.DTOs;
using System.Data;

namespace NongDanService.Data
{
    public class NongDanRepository : INongDanRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<NongDanRepository> _logger;

        public NongDanRepository(IConfiguration config, ILogger<NongDanRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public List<NongDanDTO> GetAll()
        {
            var list = new List<NongDanDTO>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("SELECT MaNongDan, MaTaiKhoan, HoTen, SoDienThoai, Email, DiaChi FROM NongDan ORDER BY MaNongDan", conn);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(MapToDTO(reader));
                }

                _logger.LogInformation("Retrieved {Count} farmers from database", list.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting all farmers");
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting all farmers");
                throw;
            }

            return list;
        }

        public NongDanDTO? GetById(int id)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(
                    "SELECT MaNongDan, MaTaiKhoan, HoTen, SoDienThoai, Email, DiaChi FROM NongDan WHERE MaNongDan = @id", conn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    _logger.LogWarning("Farmer with ID {FarmerId} not found", id);
                    return null;
                }

                var result = MapToDTO(reader);
                _logger.LogInformation("Retrieved farmer with ID {FarmerId}", id);
                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting farmer with ID {FarmerId}", id);
                throw new Exception("Lỗi truy vấn cơ sở dữ liệu", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting farmer with ID {FarmerId}", id);
                throw;
            }
        }

        public int Create(NongDanCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            
            // Sử dụng transaction để đảm bảo tạo cả TaiKhoan và NongDan
            using var transaction = conn.BeginTransaction();
            
            try
            {
                // Bước 1: Tạo tài khoản trong bảng TaiKhoan
                int maTaiKhoan;
                using (var cmdTaiKhoan = new SqlCommand(@"
                    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, LoaiTaiKhoan, TrangThai, NgayTao)
                    OUTPUT INSERTED.MaTaiKhoan
                    VALUES (@TenDangNhap, @MatKhau, @LoaiTaiKhoan, @TrangThai, GETDATE())", conn, transaction))
                {
                    cmdTaiKhoan.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 50).Value = dto.TenDangNhap;
                    cmdTaiKhoan.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 255).Value = dto.MatKhau;
                    cmdTaiKhoan.Parameters.Add("@LoaiTaiKhoan", SqlDbType.NVarChar, 20).Value = "nong_dan";
                    cmdTaiKhoan.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 20).Value = "hoat_dong";

                    maTaiKhoan = (int)cmdTaiKhoan.ExecuteScalar()!;
                    _logger.LogInformation("Created new account with ID {AccountId} for farmer", maTaiKhoan);
                }

                // Bước 2: Tạo nông dân trong bảng NongDan
                int maNongDan;
                using (var cmdNongDan = new SqlCommand(@"
                    INSERT INTO NongDan (MaTaiKhoan, HoTen, SoDienThoai, Email, DiaChi)
                    OUTPUT INSERTED.MaNongDan
                    VALUES (@MaTaiKhoan, @HoTen, @SoDienThoai, @Email, @DiaChi)", conn, transaction))
                {
                    cmdNongDan.Parameters.Add("@MaTaiKhoan", SqlDbType.Int).Value = maTaiKhoan;
                    cmdNongDan.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = (object?)dto.HoTen ?? DBNull.Value;
                    cmdNongDan.Parameters.Add("@SoDienThoai", SqlDbType.NVarChar, 20).Value = (object?)dto.SoDienThoai ?? DBNull.Value;
                    cmdNongDan.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (object?)dto.Email ?? DBNull.Value;
                    cmdNongDan.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value = (object?)dto.DiaChi ?? DBNull.Value;

                    maNongDan = (int)cmdNongDan.ExecuteScalar()!;
                }

                // Commit transaction
                transaction.Commit();
                _logger.LogInformation("Created new farmer with ID {FarmerId} and account ID {AccountId}", maNongDan, maTaiKhoan);
                
                return maNongDan;
            }
            catch (SqlException ex)
            {
                // Rollback transaction nếu có lỗi
                transaction.Rollback();
                _logger.LogError(ex, "SQL error occurred while creating farmer: {@Farmer}", dto);
                
                // Check for specific SQL errors
                if (ex.Number == 2627 || ex.Number == 2601) // Unique constraint violation
                {
                    throw new Exception("Tên đăng nhập đã tồn tại trong hệ thống", ex);
                }
                
                throw new Exception("Lỗi tạo nông dân trong cơ sở dữ liệu: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                transaction.Rollback();
                _logger.LogError(ex, "Unexpected error occurred while creating farmer: {@Farmer}", dto);
                throw;
            }
        }

        public bool Update(int id, NongDanUpdateDTO dto)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(@"
                    UPDATE NongDan
                    SET HoTen = @HoTen, 
                        SoDienThoai = @SoDienThoai, 
                        Email = @Email, 
                        DiaChi = @DiaChi
                    WHERE MaNongDan = @Id", conn);

                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = (object?)dto.HoTen ?? DBNull.Value;
                cmd.Parameters.Add("@SoDienThoai", SqlDbType.NVarChar, 20).Value = (object?)dto.SoDienThoai ?? DBNull.Value;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (object?)dto.Email ?? DBNull.Value;
                cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value = (object?)dto.DiaChi ?? DBNull.Value;

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Updated farmer with ID {FarmerId}", id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No farmer found with ID {FarmerId} to update", id);
                    return false;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating farmer with ID {FarmerId}: {@Farmer}", id, dto);
                throw new Exception("Lỗi cập nhật nông dân trong cơ sở dữ liệu", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating farmer with ID {FarmerId}: {@Farmer}", id, dto);
                throw;
            }
        }

        public bool Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            
            // Sử dụng transaction để xóa cả NongDan và TaiKhoan
            using var transaction = conn.BeginTransaction();
            
            try
            {
                // Bước 1: Lấy MaTaiKhoan của nông dân
                int? maTaiKhoan = null;
                using (var cmdGetAccount = new SqlCommand(
                    "SELECT MaTaiKhoan FROM NongDan WHERE MaNongDan = @id", conn, transaction))
                {
                    cmdGetAccount.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    var result = cmdGetAccount.ExecuteScalar();
                    if (result != null)
                    {
                        maTaiKhoan = (int)result;
                    }
                }

                if (maTaiKhoan == null)
                {
                    _logger.LogWarning("No farmer found with ID {FarmerId} to delete", id);
                    return false;
                }

                // Bước 2: Xóa nông dân
                using (var cmdDeleteNongDan = new SqlCommand(
                    "DELETE FROM NongDan WHERE MaNongDan = @id", conn, transaction))
                {
                    cmdDeleteNongDan.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmdDeleteNongDan.ExecuteNonQuery();
                }

                // Bước 3: Xóa tài khoản
                using (var cmdDeleteTaiKhoan = new SqlCommand(
                    "DELETE FROM TaiKhoan WHERE MaTaiKhoan = @id", conn, transaction))
                {
                    cmdDeleteTaiKhoan.Parameters.Add("@id", SqlDbType.Int).Value = maTaiKhoan.Value;
                    cmdDeleteTaiKhoan.ExecuteNonQuery();
                }

                transaction.Commit();
                _logger.LogInformation("Deleted farmer with ID {FarmerId} and account ID {AccountId}", id, maTaiKhoan);
                return true;
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "SQL error occurred while deleting farmer with ID {FarmerId}", id);
                
                // Check for foreign key constraint violation
                if (ex.Number == 547)
                {
                    throw new Exception("Không thể xóa nông dân này vì đang có dữ liệu liên quan", ex);
                }
                
                throw new Exception("Lỗi xóa nông dân trong cơ sở dữ liệu", ex);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Unexpected error occurred while deleting farmer with ID {FarmerId}", id);
                throw;
            }
        }

        private static NongDanDTO MapToDTO(SqlDataReader reader)
        {
            return new NongDanDTO
            {
                MaNongDan = reader.GetInt32("MaNongDan"),
                MaTaiKhoan = reader.GetInt32("MaTaiKhoan"),
                HoTen = reader.IsDBNull("HoTen") ? null : reader.GetString("HoTen"),
                SoDienThoai = reader.IsDBNull("SoDienThoai") ? null : reader.GetString("SoDienThoai"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                DiaChi = reader.IsDBNull("DiaChi") ? null : reader.GetString("DiaChi")
            };
        }
    }
}
