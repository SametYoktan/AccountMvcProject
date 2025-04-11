using Microsoft.EntityFrameworkCore;
using School.Models;
using School.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// SeriLog yapýlandýrmasý
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Tüm log seviyelerini (Debug ve daha yukarý) almak için
    .WriteTo.Console() // Konsola log yazdýrma
    .WriteTo.File("logs/myapp.log", rollingInterval: RollingInterval.Day) // Dosyaya log yazdýrma
    .CreateLogger();

// SeriLog'u DI konteynýrýna ekliyoruz
builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddControllersWithViews();

// DB Context konfigürasyonu ekleniyor
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext"))
);

// Kimlik doðrulama çerezlerini yapýlandýrýyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";  // Giriþ sayfasý
        options.AccessDeniedPath = "/Account/AccessDenied";  // Eriþim reddedildi sayfasý
        options.SlidingExpiration = true;  // Süre bitiminden sonra oturumun yeniden uzatýlmasý
        // options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Burada süreyi belirlemek
    });

// Authorization ekleniyor
builder.Services.AddAuthorization();

// Email ayarlarý
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Account service ve HttpContext eriþimi
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

// Kimlik doðrulama ve yetkilendirme middleware'ini kullanýyoruz
app.UseAuthentication();
app.UseAuthorization();

// Loglama middleware'ini ekliyoruz
app.Use(async (context, next) =>
{
    var user = context.User;
    var myCookieValue = context.Request.Cookies["UserInfo"]; // Çerezdeki veriyi alýyoruz
    Log.Information("Kullanýcý Aktif mi: {IsAuthenticated}", user.Identity.IsAuthenticated);
    Log.Information("Kullanýcý Adý: {UserName}", user.Identity.Name);
    Log.Information("Hangi Sayfada: {RequestPath}", context.Request.Path);
    Log.Information("Çerezdeki Veri: {CookieValue}", myCookieValue); // Çerezdeki veriyi yazdýr
    //Console.WriteLine($">>>>>KULLANICI AKTÝFMÝ {user.Identity.IsAuthenticated}");
    //Console.WriteLine($">>>>>KULLANICI ADI {user.Identity.Name}");
    //Console.WriteLine($">>>>>HANGÝ SAYFADAYIM {context.Request.Path}");
    //Console.WriteLine($">>>>>ÇEREZDEKÝ VERÝ {myCookieValue}");
    await next();
});

// MVC rota ayarlarý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulama baþlatýlmadan önce loglama
Log.Information("Uygulama Baþlatýlýyor...");

app.Run();
