using System.ComponentModel.DataAnnotations;

namespace NongDanService.Models.DTOs
{
    public class DonHangDaiLyCreateDTO
    {
        [Required(ErrorMessage = "Mã đại lý là bắt buộc")]
        public int MaDaiLy { get; set; }
        
        [Required(ErrorMessage = "Mã nông dân là bắt buộc")]
        public int MaNongDan { get; set; }
        
        public string? LoaiDon { get; set; }
        
        public DateTime? NgayGiao { get; set; }
        
        public decimal? TongSoLuong { get; set; }
        
        public decimal? TongGiaTri { get; set; }
        
        public string? GhiChu { get; set; }
    }
}
