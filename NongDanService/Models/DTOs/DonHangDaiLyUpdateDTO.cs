using System.ComponentModel.DataAnnotations;

namespace NongDanService.Models.DTOs
{
    public class DonHangDaiLyUpdateDTO
    {
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public decimal? SoLuong { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm")]
        public decimal? DonGia { get; set; }

        [StringLength(30, ErrorMessage = "Trạng thái không được vượt quá 30 ký tự")]
        [RegularExpression(@"^(cho_xu_ly|da_xac_nhan|dang_chuan_bi|da_xuat|da_nhan|da_huy)$", 
            ErrorMessage = "Trạng thái không hợp lệ")]
        public string? TrangThai { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? GhiChu { get; set; }
    }
}