using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class DaiLy
{
    public int MaDaiLy { get; set; }

    public int MaTaiKhoan { get; set; }

    public string? TenDaiLy { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<DonHangDaiLy> DonHangDaiLies { get; set; } = new List<DonHangDaiLy>();

    public virtual ICollection<DonHangSieuThi> DonHangSieuThis { get; set; } = new List<DonHangSieuThi>();

    public virtual ICollection<Kho> Khos { get; set; } = new List<Kho>();

    public virtual ICollection<KiemDinh> KiemDinhs { get; set; } = new List<KiemDinh>();

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}
