namespace NongDanService.Models.DTOs
{
    public class SanPhamUpdateDTO
    {
        public string TenSanPham { get; set; } = null!;
        public string DonViTinh { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}
