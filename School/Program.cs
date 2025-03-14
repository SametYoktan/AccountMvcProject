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
//E�er kullan�c� oturumu ge�erli de�ilse:

// DB Context konfig�rasyonu ekleniyor
builder.Services.AddDbContext<SchoolContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext"))
);

// Kimlik do�rulama �erezlerini yap�land�r�yoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Home/Index";
		//options.LogoutPath = "/Home/Index";
		options.AccessDeniedPath = "/Account/AccessDenied";
		options.SlidingExpiration = true;
		//options.ExpireTimeSpan = TimeSpan.FromMinutes(1);Burada S�re Verirsek Kullan�c� Beni Hat�rla Se�sede Se�mesede Ayn� S�rede D��er Buras� Statikdir
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

// Kimlik do�rulama ve yetkilendirme middleware'ini kullan�yoruz
app.UseAuthentication();
app.UseAuthorization();

// Kimlik do�rulama ve sayfa iste�i bilgilerini kontrol eden middleware'� buraya al�yoruz
app.Use(async (context, next) =>
{
	var user = context.User;
	var myCookieValue = context.Request.Cookies["UserInfo"]; // "myCookieKey" yerine kendi �erez ad�n�z� yaz�n
	Console.WriteLine($">>>>>KULLANICI AKT�FM� {user.Identity.IsAuthenticated}");
	Console.WriteLine($">>>>>KULLANICI ADI {user.Identity.Name}");
	Console.WriteLine($">>>>>HANG� SAYFADAYIM {context.Request.Path}");
	Console.WriteLine($">>>>>�EREZDEK� VER� {myCookieValue}"); // �erezdeki veriyi yazd�r
	await next();
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
