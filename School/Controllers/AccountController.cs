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

namespace School.Controllers
{
    public class AccountController : Controller
    {
        private readonly SchoolContext _context; // Veritabanı bağlantısı
        private readonly IAccountServices _accountService;

        public AccountController(SchoolContext context,IAccountServices accountService)
        {
            _context = context;
            _accountService = accountService;
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
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı kontrolü
                if (_accountService.UserIsnameControl(model.Username))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                    return View(model);
                }

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
                SetUserCookie(user.Username, user.Email, false);
                return RedirectToAction("Index", "Home");
            }

            if (userInfo != null)//Eğer Çerez Doluysa Ve UserLoginControl False Döndüyse Yani Çerezdeki Bİlgiler Hatalı İse Çerezleri Silip Logine Yönlendir
                _accountService.UserLogOutAsync();

            // Eğer cookie yoksa veya geçersizse, normal giriş formunu göster
            return View();//Çerez Boşsa Dİrek Login Formunu Göster
        }

        [HttpPost]//Sunucuya veri göndermek için kullanılır. Genellikle form gönderme işlemlerinde kullanılır.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model, bool RememberMe)
        {
            // İlk olarak ModelState'i temizliyoruz
            ModelState.Clear();

            // Kullanıcı adı doğrulaması
            if (string.IsNullOrEmpty(model.Username))
                ModelState.AddModelError("Username", "Kullanıcı adı veya E-posta gereklidir.");

            // Parola doğrulaması
            if (string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError("Password", "Parola gereklidir.");

            // Eğer model geçersizse, hatalarla birlikte geri dön
            if (!ModelState.IsValid)
                return View(model); // Hatalarla geri döneriz

            var user = _accountService.UserLogin(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("Password", "Kullanıcı adı veya parola hatalı.");
                return View(model); // Hatalarla geri dön
            }

            // Çerezi manuel olarak ayarlıyoruz
            await SetUserCookie(user.Username, user.Email, RememberMe);
            return RedirectToAction("Index", "Home");
        }

        public async Task SetUserCookie(string username, string email, bool rememberMe)
        {
            await _accountService.SetUserCookieAsync(username, email, rememberMe);
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
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Geçersiz istek.";
                return View();
            }

            // Token ile kullanıcıyı bul
            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Token geçersiz veya süresi dolmuş.";
                return View();
            }
            Console.WriteLine(user.Username + " " + user.ResetPasswordToken + " " + user.ResetPasswordTokenExpiry + " " + DateTime.UtcNow);
            // Token geçerli ise şifre sıfırlama formunu göster
            return View(new User { ResetPasswordToken = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPassword(User model)
        {
            ModelState.Clear();

            // Kullanıcı adı doğrulaması
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Parola gereklidir.");
            }
            else if (model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Parola en az 8 karakter uzunluğunda olmalıdır.");
            }
            else if (!Regex.IsMatch(model.Password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$"))
            {
                ModelState.AddModelError("Password", "Parola en az bir büyük harf, bir rakam ve bir özel karakter içermelidir.");
            }

            if (string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Parolayı onaylamak gereklidir.");
            }
            else if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Parolalar eşleşmiyor.");
            }


            // Geçerli bir token olup olmadığını tekrar kontrol et
            var user = _context.Users
                .FirstOrDefault(u => u.ResetPasswordToken == model.ResetPasswordToken && u.ResetPasswordTokenExpiry > DateTime.UtcNow);

            Console.WriteLine(user.Username + " " + user.ResetPasswordToken + " " + user.ResetPasswordTokenExpiry + " " + DateTime.UtcNow);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Geçersiz veya süresi dolmuş token.";
                Console.WriteLine("Geçersiz veya süresi dolmuş token.");
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                // Yeni şifreyi hash'le ve salt üret
                string salt = GenerateSalt(); // Benzersiz salt oluştur
                string hashedPassword = HashPassword(model.Password, salt); // Model'deki yeni şifreyi hash'le

                // Yeni şifreyi ve salt'ı kaydet
                user.PasswordHash = hashedPassword;
                user.PasswordSalt = salt;

                // Şifre sıfırlama token'ını null yap ve süresini kaldır
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;

                // Veritabanına kaydet
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Şifreniz başarıyla sıfırlandı.";
                Console.WriteLine("Şifreniz başarıyla sıfırlandı.");
                return RedirectToAction("Login", "Account");
            }

            // Model valid değilse, kullanıcıya hata mesajı göster
            ViewBag.SuccessMessage = "Hatalı Bilgiler";
            return View(model);
        }
        #endregion
    }
}