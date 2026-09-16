using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Hubs;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Application.Interfaces.Services;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Infrastructure.Data;
using PharmaSphere.Infrastructure.ExternalService;
using PharmaSphere.Infrastructure.Identity;
using PharmaSphere.Infrastructure.Repositories;
using PharmaSphere.Infrastructure.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. تسجيل قاعدة البيانات (ApplicationDbContext)
// =========================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================================
// 2. تسجيل نظام الهوية (ASP.NET Core Identity)
// =========================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // إعدادات كلمة المرور والتسجيل
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
    // -----------------------------
    // Lockout
    // -----------------------------

    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =========================================================
// Cookie Configuration
// =========================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});

// =========================================================================
// 3. تسجيل أنماط النفاذ للبيانات (Repositories & Unit of Work)
// =========================================================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================================================================
// 4. تسجيل خدمات الأعمال (Application & Infrastructure Services)
// =========================================================================
builder.Services.AddScoped<IPosService, PosService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpClient<IPaymentService, PaymobPaymentService>();

// =========================================================================
// 5. تسجيل SignalR للتحديثات اللحظية (Real-Time Communication)
// =========================================================================
builder.Services.AddSignalR();

// =========================================================================
// 6. تسجيل MVC Controllers وتنسيق الـ JSON المدمج (System.Text.Json)
// =========================================================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // تجنب أخطاء الدوران المتبادل عند تحويل الكيانات (Circular References)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// إعداد مسارات تسجيل الدخول وصلاحيات الوصول
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
// Memory Cache لحجز الشراء الأونلاين
builder.Services.AddMemoryCache();

// تسجيل خدمات الـ Infrastructure
builder.Services.AddScoped<PosService>();
builder.Services.AddSingleton<InventoryLockService>();

// 1. إضافة الخدمة
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await IdentitySeeder.SeedAsync(services);
}

// =========================================================================
// 7. إعداد الـ HTTP Request Pipeline
// =========================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 2. تفعيل الـ Middleware (تأكد أن وضعها قبل app.UseAuthorization())
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================================
// 8. ربط الـ SignalR Hubs بالمسارات الخاصة بها
// =========================================================================
app.MapHub<StockHub>("/stockHub");
app.MapHub<OrderHub>("/orderHub");

// =========================================================================
// 9. إعداد مسارات المناطق (Areas) والمسار الافتراضي (Default Route)
// =========================================================================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// =========================================================================
// 10. تنفيذ الـ Seeding التلقائي للبيانات عند بدء التشغيل
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء إدخال البيانات الأولية لقاعدة البيانات.");
    }
}

app.Run();