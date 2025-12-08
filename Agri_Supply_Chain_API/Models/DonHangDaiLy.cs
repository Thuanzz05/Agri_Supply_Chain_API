using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class DonHangDaiLy
{
    public int MaDonHang { get; set; }

    public int MaDaiLy { get; set; }

    public int MaNongDan { get; set; }

    public virtual DaiLy MaDaiLyNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual NongDan MaNongDanNavigation { get; set; } = null!;
}
