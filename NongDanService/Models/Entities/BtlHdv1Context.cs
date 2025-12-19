using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NongDanService.Models.Entities;

public partial class BtlHdv1Context : DbContext
{
    public BtlHdv1Context()
    {
    }

    public BtlHdv1Context(DbContextOptions<BtlHdv1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<DonHangDaiLy> DonHangDaiLies { get; set; }

    public virtual DbSet<LoNongSan> LoNongSans { get; set; }

    public virtual DbSet<NongDan> NongDans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TrangTrai> TrangTrais { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NVT;Database=BTL_HDV1;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DonHangDaiLy>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHangD__129584AD75DBCF5A");

            entity.ToTable("DonHangDaiLy");

            entity.Property(e => e.MaDonHang).ValueGeneratedNever();

            entity.HasOne(d => d.MaNongDanNavigation).WithMany(p => p.DonHangDaiLies)
                .HasForeignKey(d => d.MaNongDan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangDa__MaNon__68487DD7");
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
