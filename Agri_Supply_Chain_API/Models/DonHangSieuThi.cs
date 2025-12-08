using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class DonHangSieuThi
{
    public int MaDonHang { get; set; }

    public int MaSieuThi { get; set; }

    public int MaDaiLy { get; set; }

    public virtual DaiLy MaDaiLyNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual SieuThi MaSieuThiNavigation { get; set; } = null!;
}
