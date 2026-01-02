using Microsoft.Data.SqlClient;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Data
{
    public class DonHangDaiLyRepository : IDonHangDaiLyRepository
    {
        private readonly string _connectionString;

        public DonHangDaiLyRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<DonHangDaiLyDTO> GetAll()
        {
            var list = new List<DonHangDaiLyDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT dh.MaDonHang, dhd.MaDaiLy, dhd.MaNongDan, nd.HoTen as TenNongDan,
                       dh.NgayDat, dh.NgayGiao, dh.TrangThai, dh.TongSoLuong, dh.TongGiaTri, dh.GhiChu
                FROM DonHang dh
                INNER JOIN DonHangDaiLy dhd ON dh.MaDonHang = dhd.MaDonHang
                LEFT JOIN NongDan nd ON dhd.MaNongDan = nd.MaNongDan
                WHERE dh.LoaiDon = 'daily_to_nongdan'
                ORDER BY dh.NgayDat DESC", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public DonHangDaiLyDTO? GetById(int maDonHang)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Lấy thông tin đơn hàng
            using var cmd = new SqlCommand(@"
                SELECT dh.MaDonHang, dhd.MaDaiLy, dhd.MaNongDan, nd.HoTen as TenNongDan,
                       dh.NgayDat, dh.NgayGiao, dh.TrangThai, dh.TongSoLuong, dh.TongGiaTri, dh.GhiChu
                FROM DonHang dh
                INNER JOIN DonHangDaiLy dhd ON dh.MaDonHang = dhd.MaDonHang
                LEFT JOIN NongDan nd ON dhd.MaNongDan = nd.MaNongDan
                WHERE dh.MaDonHang = @MaDonHang", conn);

            cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);

            DonHangDaiLyDTO? donHang = null;
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    donHang = MapToDto(reader);
                }
            }

            if (donHang == null) return null;

            // Lấy chi tiết đơn hàng
            donHang.ChiTietDonHang = GetChiTietDonHang(conn, maDonHang);

            return donHang;
        }

        public List<DonHangDaiLyDTO> GetByMaDaiLy(int maDaiLy)
        {
            var list = new List<DonHangDaiLyDTO>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT dh.MaDonHang, dhd.MaDaiLy, dhd.MaNongDan, nd.HoTen as TenNongDan,
                       dh.NgayDat, dh.NgayGiao, dh.TrangThai, dh.TongSoLuong, dh.TongGiaTri, dh.GhiChu
                FROM DonHang dh
                INNER JOIN DonHangDaiLy dhd ON dh.MaDonHang = dhd.MaDonHang
                LEFT JOIN NongDan nd ON dhd.MaNongDan = nd.MaNongDan
                WHERE dhd.MaDaiLy = @MaDaiLy
                ORDER BY dh.NgayDat DESC", conn);

            cmd.Parameters.AddWithValue("@MaDaiLy", maDaiLy);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapToDto(reader));
            }

            return list;
        }

        public int Create(DonHangDaiLyCreateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Tính tổng số lượng và tổng giá trị
                decimal tongSoLuong = dto.ChiTietDonHang.Sum(x => x.SoLuong);
                decimal tongGiaTri = dto.ChiTietDonHang.Sum(x => x.SoLuong * (x.DonGia ?? 0));

                // 1. Tạo DonHang
                using var cmdDonHang = new SqlCommand(@"
                    INSERT INTO DonHang (LoaiDon, TrangThai, TongSoLuong, TongGiaTri, GhiChu)
                    OUTPUT INSERTED.MaDonHang
                    VALUES ('daily_to_nongdan', 'chua_nhan', @TongSoLuong, @TongGiaTri, @GhiChu)", conn, transaction);

                cmdDonHang.Parameters.AddWithValue("@TongSoLuong", tongSoLuong);
                cmdDonHang.Parameters.AddWithValue("@TongGiaTri", tongGiaTri);
                cmdDonHang.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);

                int maDonHang = (int)cmdDonHang.ExecuteScalar();

                // 2. Tạo DonHangDaiLy
                using var cmdDonHangDaiLy = new SqlCommand(@"
                    INSERT INTO DonHangDaiLy (MaDonHang, MaDaiLy, MaNongDan)
                    VALUES (@MaDonHang, @MaDaiLy, @MaNongDan)", conn, transaction);

                cmdDonHangDaiLy.Parameters.AddWithValue("@MaDonHang", maDonHang);
                cmdDonHangDaiLy.Parameters.AddWithValue("@MaDaiLy", dto.MaDaiLy);
                cmdDonHangDaiLy.Parameters.AddWithValue("@MaNongDan", dto.MaNongDan);
                cmdDonHangDaiLy.ExecuteNonQuery();

                // 3. Tạo ChiTietDonHang
                foreach (var chiTiet in dto.ChiTietDonHang)
                {
                    using var cmdChiTiet = new SqlCommand(@"
                        INSERT INTO ChiTietDonHang (MaDonHang, MaLo, SoLuong, DonGia, ThanhTien)
                        VALUES (@MaDonHang, @MaLo, @SoLuong, @DonGia, @ThanhTien)", conn, transaction);

                    cmdChiTiet.Parameters.AddWithValue("@MaDonHang", maDonHang);
                    cmdChiTiet.Parameters.AddWithValue("@MaLo", chiTiet.MaLo);
                    cmdChiTiet.Parameters.AddWithValue("@SoLuong", chiTiet.SoLuong);
                    cmdChiTiet.Parameters.AddWithValue("@DonGia", (object?)chiTiet.DonGia ?? DBNull.Value);
                    cmdChiTiet.Parameters.AddWithValue("@ThanhTien", chiTiet.SoLuong * (chiTiet.DonGia ?? 0));
                    cmdChiTiet.ExecuteNonQuery();
                }

                transaction.Commit();
                return maDonHang;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateTrangThai(int maDonHang, DonHangDaiLyUpdateDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE DonHang
                SET TrangThai = COALESCE(@TrangThai, TrangThai),
                    NgayGiao = COALESCE(@NgayGiao, NgayGiao),
                    GhiChu = COALESCE(@GhiChu, GhiChu)
                WHERE MaDonHang = @MaDonHang", conn);

            cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)dto.TrangThai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NgayGiao", (object?)dto.NgayGiao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int maDonHang)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Xóa ChiTietDonHang trước
                using var cmdChiTiet = new SqlCommand(
                    "DELETE FROM ChiTietDonHang WHERE MaDonHang = @MaDonHang", conn, transaction);
                cmdChiTiet.Parameters.AddWithValue("@MaDonHang", maDonHang);
                cmdChiTiet.ExecuteNonQuery();

                // Xóa DonHangDaiLy
                using var cmdDonHangDaiLy = new SqlCommand(
                    "DELETE FROM DonHangDaiLy WHERE MaDonHang = @MaDonHang", conn, transaction);
                cmdDonHangDaiLy.Parameters.AddWithValue("@MaDonHang", maDonHang);
                cmdDonHangDaiLy.ExecuteNonQuery();

                // Xóa DonHang
                using var cmdDonHang = new SqlCommand(
                    "DELETE FROM DonHang WHERE MaDonHang = @MaDonHang", conn, transaction);
                cmdDonHang.Parameters.AddWithValue("@MaDonHang", maDonHang);
                int result = cmdDonHang.ExecuteNonQuery();

                transaction.Commit();
                return result > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private List<ChiTietDonHangDTO> GetChiTietDonHang(SqlConnection conn, int maDonHang)
        {
            var list = new List<ChiTietDonHangDTO>();

            using var cmd = new SqlCommand(@"
                SELECT ct.MaLo, sp.TenSanPham, ct.SoLuong, ct.DonGia, ct.ThanhTien
                FROM ChiTietDonHang ct
                LEFT JOIN LoNongSan lo ON ct.MaLo = lo.MaLo
                LEFT JOIN SanPham sp ON lo.MaSanPham = sp.MaSanPham
                WHERE ct.MaDonHang = @MaDonHang", conn);

            cmd.Parameters.AddWithValue("@MaDonHang", maDonHang);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new ChiTietDonHangDTO
                {
                    MaLo = (int)reader["MaLo"],
                    TenSanPham = reader["TenSanPham"] as string,
                    SoLuong = (decimal)reader["SoLuong"],
                    DonGia = reader["DonGia"] as decimal?,
                    ThanhTien = reader["ThanhTien"] as decimal?
                });
            }

            return list;
        }

        private DonHangDaiLyDTO MapToDto(SqlDataReader reader)
        {
            return new DonHangDaiLyDTO
            {
                MaDonHang = (int)reader["MaDonHang"],
                MaDaiLy = (int)reader["MaDaiLy"],
                MaNongDan = (int)reader["MaNongDan"],
                TenNongDan = reader["TenNongDan"] as string,
                NgayDat = reader["NgayDat"] as DateTime?,
                NgayGiao = reader["NgayGiao"] as DateTime?,
                TrangThai = reader["TrangThai"] as string,
                TongSoLuong = reader["TongSoLuong"] as decimal?,
                TongGiaTri = reader["TongGiaTri"] as decimal?,
                GhiChu = reader["GhiChu"] as string
            };
        }
    }
}
