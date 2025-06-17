using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.Server;
using School.Models;    // Account modelini kullanalım
using School.Services; // Email Servisini ekliyoruz
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace School.Controllers
{
	public class AccountController : Controller
	{
		private readonly SchoolContext _context; // Veritabanı bağlantısı
		private readonly IAccountServices _accountService;
		private readonly ILogger<AccountController> _logger;  // ILogger'ı ekliyoruz.

		public AccountController(SchoolContext context, IAccountServices accountService, ILogger<AccountController> logger)
		{
			_context = context;
			_accountService = accountService;
			_logger = logger;
		}

		//Sayfa açılırken → GET çalışır(formu gösterir).
		//Form gönderildiğinde → POST çalışır(bilgileri işler).

		#region Kayıt Olma
		// Kullanıcı kayıt sayfasını yükler
		public IActionResult Register()
		{
			return View();
		}

		// Kullanıcı kaydını işler
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Register(NewUsers model)
		{
			if (ModelState.IsValid)
			{
				// E-posta kontrolü
				if (_accountService.UserIsEmailControl(model.Email))
				{
					ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
					return View(model);
				}
				_accountService.UserRegister(model);
				return RedirectToAction("Login", "Account");
			}

			// Hataları debug etmek için ModelState hatalarını yazdırma
			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				Console.WriteLine(error.ErrorMessage);
				ViewBag.Error = error.ErrorMessage;
			}
			return View(model); // Hataları tekrar görüntülemek için View'a göndeririz           
		}

		//Şifreyi hash'leme fonksiyonu (PBKDF2 ile)

		private string HashPassword(string password, string salt)
		{
			return _accountService.HashPassword(password, salt);
		}

		// Salt oluşturma fonksiyonu (şifre güvenliği için)
		private string GenerateSalt()
		{
			return _accountService.GenerateSalt();
		}
		#endregion

		#region Giriş Yapma
		// Kullanıcı giriş sayfasını yükler
		[HttpGet]//Veriyi almak için kullanılır. Tarayıcıdan URL'ye girildiğinde veya bir sayfa yüklendiğinde çağrılır.
		public IActionResult Login()
		{		
			// Eğer kullanıcı zaten giriş yaptıysa, cookie'den bilgileri alalım
			var userInfo = Request.Cookies["UserInfo"]; //buraya iki if koymamın sebebi şu, çerez dolu olabilir ama kullanıcı bilgileri bir ihtimal değişirse hata mesajını düzgün görmek için		

			if (userInfo != null && _accountService.UserLoginControl(userInfo) is { } user) //Çerez Boş Değilse Ve UserLoginControl null dönmüyorsa yani çerezdeki veri doğruysa ekstra güvenlik kontrolü
			{
				Console.WriteLine("test");
				var userWithRoles = _context._NewUsers
			.Include(u => u.UserRoles)           // UserRoles ilişkisini yükle
			.ThenInclude(ur => ur.Role)          // UserRoles içindeki Role ilişkisini yükle
			.FirstOrDefault(u => u.Id == user.Id);

				var roleName = userWithRoles.UserRoles
			.Select(ur => ur.Role.Name)
			 .First();
				Console.WriteLine("aaa" + roleName);
				SetUserCookie(user.Email, user.Name, user.Surname, true, roleName);
				return RedirectToAction("Index", "Home");
			}

			if (userInfo != null)//Eğer Çerez Doluysa Ve UserLoginControl False Döndüyse Yani Çerezdeki Bİlgiler Hatalı İse Çerezleri Silip Logine Yönlendir
				_accountService.UserLogOutAsync();

			// Eğer cookie yoksa veya geçersizse, normal giriş formunu göster
			return View();//Çerez Boşsa Dİrek Login Formunu Göster
		}

		[HttpPost]//Sunucuya veri göndermek için kullanılır. Genellikle form gönderme işlemlerinde kullanılır.
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(NewUsers model, bool RememberMe)
		{
			// İlk olarak ModelState'i temizliyoruz
			ModelState.Clear();

			//// Kullanıcı adı doğrulaması
			//if (string.IsNullOrEmpty(model.Username))
			//    ModelState.AddModelError("Username", "Kullanıcı adı veya E-posta gereklidir.");

			// Parola doğrulaması
			if (string.IsNullOrEmpty(model.Password))
				ModelState.AddModelError("Password", "Parola gereklidir.");

			// Eğer model geçersizse, hatalarla birlikte geri dön
			if (!ModelState.IsValid)
				return View(model); // Hatalarla geri döneriz

			var user = _accountService.UserLogin(model.Email, model.Password);



			if (user == null || !user.IsActive)
			{
				ModelState.AddModelError("Password", "Kullanıcı adı veya parola hatalı.");
				return View(model); // Hatalarla geri dön
			}

			var roleID = _context._NewUserRoles.Where(ur => ur.UserID == user.Id).Select(ur => ur.RoleID).FirstOrDefault();

			var userWithRoles = _context._NewUsers
			.Include(u => u.UserRoles)           // UserRoles ilişkisini yükle
			.ThenInclude(ur => ur.Role)          // UserRoles içindeki Role ilişkisini yükle
			.FirstOrDefault(u => u.Id == user.Id);

			var roleName = userWithRoles.UserRoles
			.Select(ur => ur.Role.Name)
			 .First();

			Console.WriteLine("Account Controller Login Post "+roleName, userWithRoles);

			// Çerezi manuel olarak ayarlıyoruz
			await SetUserCookie(user.Email, user.Name, user.Surname, RememberMe, roleName);

			///Adminmi Standart Kullanıcımı Onu Kontrol Ediyoruz
			/// 1=Standart,2=Admin
			Console.WriteLine("Account Controller Login Post "+roleID);			

			if (roleID == 1)
				return RedirectToAction("Index", "Home");
			else
				return RedirectToAction("Student", "Home");//ADMİN PANELE YÖNLENDİR
		}

		public async Task SetUserCookie(string email, string name, string surname, bool rememberMe, string role)
		{
			await _accountService.SetUserCookieAsync(email, name, surname, rememberMe, role);			
		}

		public async Task<IActionResult> Logout()
		{
			await _accountService.UserLogOutAsync();
			// Kullanıcıyı giriş sayfasına yönlendiriyoruz
			return RedirectToAction("Index", "Home");
		}
		#endregion

		#region Parola Sıfırlama
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]//Sunucuya veri göndermek için kullanılır. Genellikle form gönderme işlemlerinde kullanılır.
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			ModelState.Clear();

			var result = await _accountService.ForgotPassword(email);

			if (!result)
			{
				ModelState.AddModelError("Email", "Geçerli bir e-posta adresi giriniz.");
				return View();
			}

			ViewBag.SuccessMessage = "Şifre sıfırlama talimatları e-posta adresinize gönderildi.";
			return View();
		}
		#endregion

		#region Parola Değiştirme
		[HttpGet]
		public IActionResult ConfirmPassword(string token)
		{
			var user = _accountService.GetUserByResetToken(token);

			if (user == null)
			{
				ViewBag.ErrorMessage = "Token geçersiz veya süresi dolmuş.";
				return View();
			}

			var model = new NewUsers
			{
				Id = user.UserID,
				ResetHistoriesTokens = new List<NewPasswordHistory> { user }
			};

			Console.WriteLine(user.UserID + " " + user.Token + " Bitiş Süresi " + user.ExpiryDate + " Şuanki Zaman " + DateTime.Now);
			// Token ve user bilgilerini view'e gönder
			ViewBag.Token = token;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmPassword(NewUsers model, NewPasswordHistory tokenn)
		{
			ModelState.Clear();

			if (_accountService.ResetPassword(tokenn.Token, model.Password, model.ConfirmPassword, out string errorMessage))
			{
				ViewBag.SuccessMessage = "Şifreniz başarıyla sıfırlandı.";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				ViewBag.ErrorMessage = errorMessage;
				return View(model);
			}
		}
		#endregion

		#region Arka Planda Çalışan Hesap Aktifleştirme Methodu
		[HttpGet]
		public async Task<IActionResult> ActivateAndRedirect(string email,string token)//Bu Method Bir Sayfaya Bağlı Çalışmaz.Arka Planda Tetiklendiğinde Çalışır
		{
			ViewBag.Token = token;
			var user = _accountService.GetUserIsActiveToken(token);

			if (user == null)
			{
				ViewBag.ErrorMessage = "Token geçersiz veya süresi dolmuş.";
				return View();
			}

			Console.WriteLine("Kullanıcının " + user.UserID);

			var isActivated = await _accountService.ActivateAndRedirect(email);

			Console.WriteLine("Kullanıcı " + user.UserID);

			if (!isActivated)
			{
				_logger.LogInformation("Kullanıcı bulunamadı: {Email}", email);
				return NotFound("Kullanıcı bulunamadı.");
			}

			_logger.LogInformation("Kullanıcı Hesabı Aktifleştirildi: {Email}", email);
			return RedirectToAction("Login", "Account", new { activated = true });
		}
		#endregion

	}
}