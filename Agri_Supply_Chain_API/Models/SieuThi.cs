using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class SieuThi
{
    public int MaSieuThi { get; set; }

    public int MaTaiKhoan { get; set; }

    public string? TenSieuThi { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<DonHangSieuThi> DonHangSieuThis { get; set; } = new List<DonHangSieuThi>();

    public virtual ICollection<Kho> Khos { get; set; } = new List<Kho>();

    public virtual ICollection<KiemDinh> KiemDinhs { get; set; } = new List<KiemDinh>();

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}
