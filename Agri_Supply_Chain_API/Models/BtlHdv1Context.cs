using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Agri_Supply_Chain_API.Models;

public partial class BtlHdv1Context : DbContext
{
    public BtlHdv1Context()
    {
    }

    public BtlHdv1Context(DbContextOptions<BtlHdv1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<DaiLy> DaiLies { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonHangDaiLy> DonHangDaiLies { get; set; }

    public virtual DbSet<DonHangSieuThi> DonHangSieuThis { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }

    public virtual DbSet<KiemDinh> KiemDinhs { get; set; }

    public virtual DbSet<LoNongSan> LoNongSans { get; set; }

    public virtual DbSet<NongDan> NongDans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SieuThi> SieuThis { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    public virtual DbSet<TrangTrai> TrangTrais { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DuyThuanzz;Database=BTL_HDV1;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.MaAdmin).HasName("PK__Admin__49341E3894E7F6CA");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__Admin__AD7C6528336147FB").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.MaTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admin__MaTaiKhoa__3D5E1FD2");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.MaDonHang, e.MaLo }).HasName("PK__ChiTietD__60E7D8D817114A00");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuong).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__6FE99F9F");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDon__MaLo__70DDC3D8");
        });

        modelBuilder.Entity<DaiLy>(entity =>
        {
            entity.HasKey(e => e.MaDaiLy).HasName("PK__DaiLy__069B00B35AD5446B");

            entity.ToTable("DaiLy");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__DaiLy__AD7C6528E8023C64").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TenDaiLy).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.DaiLy)
                .HasForeignKey<DaiLy>(d => d.MaTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DaiLy__MaTaiKhoa__44FF419A");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD453005A8");

            entity.ToTable("DonHang");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiDon).HasMaxLength(30);
            entity.Property(e => e.NgayDat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TongGiaTri).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongSoLuong).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("chua_nhan");
        });

        modelBuilder.Entity<DonHangDaiLy>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHangD__129584AD75DBCF5A");

            entity.ToTable("DonHangDaiLy");

            entity.Property(e => e.MaDonHang).ValueGeneratedNever();

            entity.HasOne(d => d.MaDaiLyNavigation).WithMany(p => p.DonHangDaiLies)
                .HasForeignKey(d => d.MaDaiLy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangDa__MaDai__6754599E");

            entity.HasOne(d => d.MaDonHangNavigation).WithOne(p => p.DonHangDaiLy)
                .HasForeignKey<DonHangDaiLy>(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangDa__MaDon__66603565");

            entity.HasOne(d => d.MaNongDanNavigation).WithMany(p => p.DonHangDaiLies)
                .HasForeignKey(d => d.MaNongDan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangDa__MaNon__68487DD7");
        });

        modelBuilder.Entity<DonHangSieuThi>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHangS__129584ADC8B45C99");

            entity.ToTable("DonHangSieuThi");

            entity.Property(e => e.MaDonHang).ValueGeneratedNever();

            entity.HasOne(d => d.MaDaiLyNavigation).WithMany(p => p.DonHangSieuThis)
                .HasForeignKey(d => d.MaDaiLy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangSi__MaDai__6D0D32F4");

            entity.HasOne(d => d.MaDonHangNavigation).WithOne(p => p.DonHangSieuThi)
                .HasForeignKey<DonHangSieuThi>(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangSi__MaDon__6B24EA82");

            entity.HasOne(d => d.MaSieuThiNavigation).WithMany(p => p.DonHangSieuThis)
                .HasForeignKey(d => d.MaSieuThi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangSi__MaSie__6C190EBB");
        });

        modelBuilder.Entity<Kho>(entity =>
        {
            entity.HasKey(e => e.MaKho).HasName("PK__Kho__3BDA9350E3DB6FF4");

            entity.ToTable("Kho");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.LoaiKho).HasMaxLength(20);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenKho).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("hoat_dong");

            entity.HasOne(d => d.MaDaiLyNavigation).WithMany(p => p.Khos)
                .HasForeignKey(d => d.MaDaiLy)
                .HasConstraintName("FK__Kho__MaDaiLy__59FA5E80");

            entity.HasOne(d => d.MaSieuThiNavigation).WithMany(p => p.Khos)
                .HasForeignKey(d => d.MaSieuThi)
                .HasConstraintName("FK__Kho__MaSieuThi__5AEE82B9");
        });

        modelBuilder.Entity<KiemDinh>(entity =>
        {
            entity.HasKey(e => e.MaKiemDinh).HasName("PK__KiemDinh__5C6E570107E3736D");

            entity.ToTable("KiemDinh");

            entity.Property(e => e.ChuKySo).HasMaxLength(255);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.KetQua).HasMaxLength(20);
            entity.Property(e => e.NgayKiemDinh).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NguoiKiemDinh).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("hoan_thanh");

            entity.HasOne(d => d.MaDaiLyNavigation).WithMany(p => p.KiemDinhs)
                .HasForeignKey(d => d.MaDaiLy)
                .HasConstraintName("FK__KiemDinh__MaDaiL__76969D2E");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.KiemDinhs)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KiemDinh__MaLo__75A278F5");

            entity.HasOne(d => d.MaSieuThiNavigation).WithMany(p => p.KiemDinhs)
                .HasForeignKey(d => d.MaSieuThi)
                .HasConstraintName("FK__KiemDinh__MaSieu__778AC167");
        });

        modelBuilder.Entity<LoNongSan>(entity =>
        {
            entity.HasKey(e => e.MaLo).HasName("PK__LoNongSa__2725C7561F7AE841");

            entity.ToTable("LoNongSan");

            entity.Property(e => e.MaQr)
                .HasMaxLength(255)
                .HasColumnName("MaQR");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoChungNhanLo).HasMaxLength(50);
            entity.Property(e => e.SoLuongBanDau).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuongHienTai).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("tai_trang_trai");

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.LoNongSans)
                .HasForeignKey(d => d.MaSanPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoNongSan__MaSan__5535A963");

            entity.HasOne(d => d.MaTrangTraiNavigation).WithMany(p => p.LoNongSans)
                .HasForeignKey(d => d.MaTrangTrai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoNongSan__MaTra__5441852A");
        });

        modelBuilder.Entity<NongDan>(entity =>
        {
            entity.HasKey(e => e.MaNongDan).HasName("PK__NongDan__A4CC49E6DA343779");

            entity.ToTable("NongDan");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__NongDan__AD7C65283F2EDC81").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.NongDan)
                .HasForeignKey<NongDan>(d => d.MaTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NongDan__MaTaiKh__412EB0B6");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSanPham).HasName("PK__SanPham__FAC7442DDFACFDC6");

            entity.ToTable("SanPham");

            entity.Property(e => e.DonViTinh).HasMaxLength(20);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenSanPham).HasMaxLength(100);
        });

        modelBuilder.Entity<SieuThi>(entity =>
        {
            entity.HasKey(e => e.MaSieuThi).HasName("PK__SieuThi__7CF72B9F6A6B90D3");

            entity.ToTable("SieuThi");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__SieuThi__AD7C65282B66BF0E").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TenSieuThi).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.SieuThi)
                .HasForeignKey<SieuThi>(d => d.MaTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SieuThi__MaTaiKh__48CFD27E");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C65291E1DB12F");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC07B06BC9D").IsUnique();

            entity.Property(e => e.LoaiTaiKhoan).HasMaxLength(20);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("hoat_dong");
        });

        modelBuilder.Entity<TonKho>(entity =>
        {
            entity.HasKey(e => new { e.MaKho, e.MaLo }).HasName("PK__TonKho__49A8CF2519594A7D");

            entity.ToTable("TonKho");

            entity.Property(e => e.CapNhatCuoi).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoLuong).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaKhoNavigation).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.MaKho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TonKho__MaKho__5EBF139D");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TonKho__MaLo__5FB337D6");
        });

        modelBuilder.Entity<TrangTrai>(entity =>
        {
            entity.HasKey(e => e.MaTrangTrai).HasName("PK__TrangTra__5C7F79087FA5CAA6");

            entity.ToTable("TrangTrai");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoChungNhan).HasMaxLength(50);
            entity.Property(e => e.TenTrangTrai).HasMaxLength(100);

            entity.HasOne(d => d.MaNongDanNavigation).WithMany(p => p.TrangTrais)
                .HasForeignKey(d => d.MaNongDan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TrangTrai__MaNon__4CA06362");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
