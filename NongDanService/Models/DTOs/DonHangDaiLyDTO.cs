namespace NongDanService.Models.DTOs
{
    public class DonHangDaiLyDTO
    {
        public int MaDonHang { get; set; }
        public int MaNongDan { get; set; }
        public int? MaDaiLy { get; set; }
        public int? MaLo { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? GhiChu { get; set; }
        
        // Thông tin bổ sung
        public string? TenNongDan { get; set; }
        public string? TenDaiLy { get; set; }
        public string? TenSanPham { get; set; }
        public string? MaQR { get; set; }
    }
}