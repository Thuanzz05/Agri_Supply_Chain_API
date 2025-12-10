using System;
using System.Collections.Generic;

namespace NongDanService.Models.Entities;

public partial class DonHangDaiLy
{
    public int MaDonHang { get; set; }

    public int MaDaiLy { get; set; }

    public int MaNongDan { get; set; }

    public virtual NongDan MaNongDanNavigation { get; set; } = null!;
}
