using Microsoft.EntityFrameworkCore;
using NongDanService.Models.Entities;
using NongDanService.Data;
using NongDanService.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Add services
// =====================

// DbContext
builder.Services.AddDbContext<BtlHdv1Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Repository
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();

// Service
builder.Services.AddScoped<ISanPhamService, SanPhamService>();

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
