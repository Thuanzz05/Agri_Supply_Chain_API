using System;
using System.Collections.Generic;

namespace Agri_Supply_Chain_API.Models;

public partial class Admin
{
    public int MaAdmin { get; set; }

    public int MaTaiKhoan { get; set; }

    public string? HoTen { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}
