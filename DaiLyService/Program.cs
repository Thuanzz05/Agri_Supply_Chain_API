using DbHelper;
using DaiLyService.Data;
using DaiLyService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ====================
// DEPENDENCY INJECTION
// ====================

// 1. Đăng ký SqlServerHelper: Đăng ký chính lớp triển khai để nó có thể được tạo (và tự nhận IConfiguration)
builder.Services.AddScoped<SqlServerHelper>();


// 2. Đăng ký các Interface trỏ đến cùng một lớp triển khai (SqlServerHelper)
// Fix lỗi: Sau khi SqlServerHelper kế thừa ILegacyDbHelper, dòng này chạy đúng
builder.Services.AddScoped<ILegacyDbHelper>(provider => provider.GetRequiredService<SqlServerHelper>());
builder.Services.AddScoped<IDbHelper>(provider => provider.GetRequiredService<SqlServerHelper>());
// 3. Đăng ký Repository
builder.Services.AddScoped<IDaiLyRepository, DaiLyRepository>();

// 4. Đăng ký Service (Giả định bạn đã đổi tên thành DaiLyBusinessService)
// *Lưu ý: Nếu tên Service của bạn là DaiLyService, bạn cần sửa lại tên trong dòng này.*
builder.Services.AddScoped<IDaiLyService, DaiLyBusinessService>();


// ====================
// SERVICES
// ====================

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DaiLy Service API",
        Version = "v1",
        Description = "Microservice quản lý Đại lý"
    });
});

// CORS cho Microservices
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ====================
// BUILD & MIDDLEWARE
// ====================

var app = builder.Build();

// Swagger (Development & Production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DaiLy Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();