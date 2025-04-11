using Microsoft.EntityFrameworkCore;
using School.Models;
using School.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// SeriLog yap�land�rmas�
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // T�m log seviyelerini (Debug ve daha yukar�) almak i�in
    .WriteTo.Console() // Konsola log yazd�rma
    .WriteTo.File("logs/myapp.log", rollingInterval: RollingInterval.Day) // Dosyaya log yazd�rma
    .CreateLogger();

// SeriLog'u DI konteyn�r�na ekliyoruz
builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddControllersWithViews();

// DB Context konfig�rasyonu ekleniyor
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext"))
);

// Kimlik do�rulama �erezlerini yap�land�r�yoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";  // Giri� sayfas�
        options.AccessDeniedPath = "/Account/AccessDenied";  // Eri�im reddedildi sayfas�
        options.SlidingExpiration = true;  // S�re bitiminden sonra oturumun yeniden uzat�lmas�
        // options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Burada s�reyi belirlemek
    });

// Authorization ekleniyor
builder.Services.AddAuthorization();

// Email ayarlar�
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Account service ve HttpContext eri�imi
builder.Services.AddScoped<IAccountServices, AccountServices>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik do�rulama ve yetkilendirme middleware'ini kullan�yoruz
app.UseAuthentication();
app.UseAuthorization();

// Loglama middleware'ini ekliyoruz
app.Use(async (context, next) =>
{
    var user = context.User;
    var myCookieValue = context.Request.Cookies["UserInfo"]; // �erezdeki veriyi al�yoruz
    Log.Information("Kullan�c� Aktif mi: {IsAuthenticated}", user.Identity.IsAuthenticated);
    Log.Information("Kullan�c� Ad�: {UserName}", user.Identity.Name);
    Log.Information("Hangi Sayfada: {RequestPath}", context.Request.Path);
    Log.Information("�erezdeki Veri: {CookieValue}", myCookieValue); // �erezdeki veriyi yazd�r
    //Console.WriteLine($">>>>>KULLANICI AKT�FM� {user.Identity.IsAuthenticated}");
    //Console.WriteLine($">>>>>KULLANICI ADI {user.Identity.Name}");
    //Console.WriteLine($">>>>>HANG� SAYFADAYIM {context.Request.Path}");
    //Console.WriteLine($">>>>>�EREZDEK� VER� {myCookieValue}");
    await next();
});

// MVC rota ayarlar�
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulama ba�lat�lmadan �nce loglama
Log.Information("Uygulama Ba�lat�l�yor...");

app.Run();
