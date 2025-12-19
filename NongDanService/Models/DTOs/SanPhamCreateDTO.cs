namespace NongDanService.Models.DTOs
{
    public class SanPhamCreateDTO
    {
        public string TenSanPham { get; set; } = null!;
        public string DonViTinh { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}
