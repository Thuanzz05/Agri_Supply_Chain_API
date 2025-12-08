using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class NongDan
{
    public int MaNongDan { get; set; }

    public int MaTaiKhoan { get; set; }

    public string? HoTen { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<DonHangDaiLy> DonHangDaiLies { get; set; } = new List<DonHangDaiLy>();

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;

    public virtual ICollection<TrangTrai> TrangTrais { get; set; } = new List<TrangTrai>();
}
