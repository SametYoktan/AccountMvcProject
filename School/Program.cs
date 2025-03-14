using Microsoft.EntityFrameworkCore;
using School.Models;
using School.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//Eðer kullanýcý oturumu geçerli deðilse:

// DB Context konfigürasyonu ekleniyor
builder.Services.AddDbContext<SchoolContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext"))
);

// Kimlik doðrulama çerezlerini yapýlandýrýyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Home/Index";
		//options.LogoutPath = "/Home/Index";
		options.AccessDeniedPath = "/Account/AccessDenied";
		options.SlidingExpiration = true;
		//options.ExpireTimeSpan = TimeSpan.FromMinutes(1);Burada Süre Verirsek Kullanýcý Beni Hatýrla Seçsede Seçmesede Ayný Sürede Düþer Burasý Statikdir
	});

// Authorization ekleniyor
builder.Services.AddAuthorization();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Email Servisini DI Konteynerine Kaydet
builder.Services.AddTransient<IEmailService, EmailService>();
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

// Kimlik doðrulama ve sayfa isteði bilgilerini kontrol eden middleware'ý buraya alýyoruz
app.Use(async (context, next) =>
{
	var user = context.User;
	var myCookieValue = context.Request.Cookies["UserInfo"]; // "myCookieKey" yerine kendi çerez adýnýzý yazýn
	Console.WriteLine($">>>>>KULLANICI AKTÝFMÝ {user.Identity.IsAuthenticated}");
	Console.WriteLine($">>>>>KULLANICI ADI {user.Identity.Name}");
	Console.WriteLine($">>>>>HANGÝ SAYFADAYIM {context.Request.Path}");
	Console.WriteLine($">>>>>ÇEREZDEKÝ VERÝ {myCookieValue}"); // Çerezdeki veriyi yazdýr
	await next();
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
