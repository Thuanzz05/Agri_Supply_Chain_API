using System.ComponentModel.DataAnnotations;

namespace NongDanService.Models.DTOs
{
    public class DonHangDaiLyCreateDTO
    {
        [Required(ErrorMessage = "Mã nông dân là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nông dân phải lớn hơn 0")]
        public int MaNongDan { get; set; }

        public int? MaDaiLy { get; set; }

        [Required(ErrorMessage = "Mã lô nông sản là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã lô phải lớn hơn 0")]
        public int MaLo { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal SoLuong { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm")]
        public decimal? DonGia { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? GhiChu { get; set; }
    }
}