using NongDanService.Data;
using NongDanService.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Add services
// =====================

// Repository
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
builder.Services.AddScoped<INongDanRepository, NongDanRepository>();
builder.Services.AddScoped<ITrangTraiRepository, TrangTraiRepository>();
builder.Services.AddScoped<ILoNongSanRepository, LoNongSanRepository>();
builder.Services.AddScoped<IDonHangDaiLyRepository, DonHangDaiLyRepository>();
builder.Services.AddScoped<IChiTietDonHangRepository, ChiTietDonHangRepository>();

// Service
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<INongDanService, NongDanService.Services.NongDanService>();
builder.Services.AddScoped<ITrangTraiService, TrangTraiService>();
builder.Services.AddScoped<ILoNongSanService, LoNongSanService>();
builder.Services.AddScoped<IDonHangDaiLyService, DonHangDaiLyService>();
builder.Services.AddScoped<IChiTietDonHangService, ChiTietDonHangService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =====================
// Middleware
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
