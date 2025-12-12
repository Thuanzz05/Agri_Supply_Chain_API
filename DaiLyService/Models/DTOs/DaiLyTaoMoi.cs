using System.ComponentModel.DataAnnotations;

namespace DaiLyService.Models.DTOs
{
    public class DaiLyTaoMoi
    {
        [Required(ErrorMessage = "Mã tài khoản là bắt buộc")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đại lý là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên đại lý không được vượt quá 200 ký tự")]
        public string TenDaiLy { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? DiaChi { get; set; }
    }
}