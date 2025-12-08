using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class TrangTrai
{
    public int MaTrangTrai { get; set; }

    public int MaNongDan { get; set; }

    public string TenTrangTrai { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? SoChungNhan { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<LoNongSan> LoNongSans { get; set; } = new List<LoNongSan>();

    public virtual NongDan MaNongDanNavigation { get; set; } = null!;
}
