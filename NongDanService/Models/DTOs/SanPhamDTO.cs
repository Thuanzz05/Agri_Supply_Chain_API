namespace NongDanService.Models.DTOs
{
    public class SanPhamDTO
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = null!;
        public string DonViTinh { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}
