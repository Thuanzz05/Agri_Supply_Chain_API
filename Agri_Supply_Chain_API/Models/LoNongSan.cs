using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class LoNongSan
{
    public int MaLo { get; set; }

    public int MaTrangTrai { get; set; }

    public int MaSanPham { get; set; }

    public decimal SoLuongBanDau { get; set; }

    public decimal SoLuongHienTai { get; set; }

    public DateOnly? NgayThuHoach { get; set; }

    public DateOnly? HanSuDung { get; set; }

    public string? SoChungNhanLo { get; set; }

    public string? MaQr { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<KiemDinh> KiemDinhs { get; set; } = new List<KiemDinh>();

    public virtual SanPham MaSanPhamNavigation { get; set; } = null!;

    public virtual TrangTrai MaTrangTraiNavigation { get; set; } = null!;

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
