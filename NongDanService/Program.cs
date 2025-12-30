using NongDanService.Data;
using NongDanService.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Add services
// =====================

// Repository
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
builder.Services.AddScoped<INongDanRepository, NongDanRepository>();

// Service
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<INongDanService, NongDanService.Services.NongDanService>();

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
