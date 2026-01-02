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
