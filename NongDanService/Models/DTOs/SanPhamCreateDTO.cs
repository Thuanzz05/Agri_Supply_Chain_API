namespace NongDanService.Models.DTOs
{
    public class SanPhamCreateDTO
    {
        public string TenSanPham { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public string? MoTa { get; set; }
    }
}
