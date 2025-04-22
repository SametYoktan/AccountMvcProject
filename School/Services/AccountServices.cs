using Azure.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities.Encoders;
using School.Models;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using Azure;
using System.Text.RegularExpressions;


namespace School.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly SchoolContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountServices> _logger;  // ILogger'ı ekliyoruz.

        public AccountServices(SchoolContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService, ILogger<AccountServices> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _logger = logger;
        }

        #region YARDIMCI METOTLAR
        public bool UserIsnameControl(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public bool UserIsEmailControl(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        //Burası Kullanıcı Adı Ve Mail'i Direk Çerezdeki Veriden Çekiyor Textboxtan Değil O yüzden model.Username Yazmadık
        public User? UserIsnameAndEmailControl(string username, string email)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username || u.Email == email);
        }

        public string HashPassword(string password, string salt)
        {
            _logger.LogInformation("Şifre hashleniyor: {Password}, Salt: {Salt}", password, salt); // Loglama ekliyoruz (Tabii ki şifreyi açıkça loglamak istemezsiniz)
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(64));
            }
        }

        public string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        public void UserLoginTime(User user)
        {
            DateTime authTime = DateTime.UtcNow;
            Console.WriteLine(">>>>> GİRİŞ ZAMANI: " + authTime.ToLocalTime());
            user.LastLogin = authTime.ToLocalTime();
            _context.SaveChanges();
            _logger.LogInformation("Kullanıcı giriş yaptı: {Username}, Giriş Zamanı: {AuthTime}", user.Username, authTime);  // Loglama ekliyoruz
        }

        public async Task UserLogOutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Kullanıcıyı çıkış yaptıktan sonra çerezleri temizliyoruz
            httpContext.Response.Cookies.Delete("UserInfo"); // AuthTime ve diğer kullanıcı bilgilerini içeren çerezi sil
            httpContext.Response.Cookies.Delete(".AspNetCore.Cookies"); // Bu çerez, ASP.NET Core kimlik doğrulama çerezi
            _logger.LogInformation("Kullanıcı çıkış yaptı: {Username}", httpContext.User.Identity?.Name); // Loglama ekliyoruz
        }

        void AccountUnlockMail(User user,string usernameOrEmail) //interface olarak tanımlamadık çünkü sadece burada kullanacağız
        {
            _logger.LogWarning("Kilitli Hesabı Açma Talebi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
            var activationLink = $"https://localhost:7070/Account/ActivateAndRedirect?email={user.Email}";
            var emailBody = $"Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:<br><br> <a href='{activationLink}'>Hesabı Aktifleştir</a>";
            //var emailBody = $"Hesabınız Şüpheli Giriş Nedeniyle Kilitlendi Kilidi Açmak için bağlantıya tıklayın:<br><br> <a href=''>Hesabınız Kilitlendi</a>";
            _emailService.SendEmailAsync(user.Email, "Hesabınız Kilitlendi", emailBody);
            _logger.LogWarning("Kilitli Hesaba Ait E-mail Adresine Unlock Maili Gönderildi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
        }
        #endregion

        public void UserRegister(User model)
        {
            try
            {
                string salt = GenerateSalt();
                string hashedPassword = HashPassword(model.Password, salt);
                model.PasswordSalt = salt;
                model.PasswordHash = hashedPassword;
                _context.Users.Add(model);
                _context.SaveChanges();
                _logger.LogInformation("Yeni kullanıcı kaydedildi: {Username}", model.Username);  // Loglama ekliyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu! Kullanıcı adı: {Username}", model.Username);  // Hata loglama
            }
        }

        public User? UserLoginControl(string userInfo)
        {
            if (string.IsNullOrEmpty(userInfo))
                return null;

            var userInfoParts = userInfo.Split('|');
            if (userInfoParts.Length != 2)
                return null;

            string username = userInfoParts[0];
            string email = userInfoParts[1];
            var user = UserIsnameAndEmailControl(username, email);

            if (user == null) // Eğer kullanıcı bulunamazsa yani çerzdeki veri yanlışsa null döndürmeliyiz!
            {
                _logger.LogWarning("Giriş denemesi başarısız: {Username}, {Email}", username, email); // Hata loglama
                return null;
            }

            UserLoginTime(user);
            return user;
        }//LOGİN GET

        public User? UserLogin(string usernameOrEmail, string password)//LOGİN POST
        {
            var user = UserIsnameAndEmailControl(usernameOrEmail, usernameOrEmail);

            if (user == null)//SİSTEMDE OLMAYAN KULLANICI GİRİŞ YAPMAYA ÇALIŞIRSA BU LOG DÖNER
            {
                _logger.LogWarning("Başarısız giriş denemesi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
                return null;
            }

            var hashedPassword = HashPassword(password, user.PasswordSalt);

            if (user.PasswordHash != hashedPassword) // Eğer hash'ler eşleşmezse
            {
                _logger.LogWarning("Yanlış şifre girildi: {UsernameOrEmail}", usernameOrEmail);  // Yanlış şifre loglaması
                user.LoginErrorNumber++;
                _context.SaveChanges();
                _logger.LogWarning("Hatalı giriş sayısı: {user.LoginErrorNumber}", user.LoginErrorNumber);  // Yanlış şifre loglaması
                if (user.LoginErrorNumber >= 5 && user.IsActive)
                {
                    user.IsActive = false;
                    _context.SaveChanges();
                    _logger.LogWarning("Hesap kilitlendi: {UsernameOrEmail}", usernameOrEmail);  // Yanlış şifre loglaması
                    AccountUnlockMail(user, usernameOrEmail);
                }
                return null; // Hatalı şifre, null döndürülür
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Kilitli hesaba giriş denemesi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
                AccountUnlockMail(user, usernameOrEmail);
                return user;
                //KİTLİ HESABA GİRİŞ YAPILMAYA ÇALIŞILIRSA OTOMATİK HESABI AKTİF ETMEK İÇİN MAİL GÖNDERİLSİN
            }

            // Başarılı giriş
            try
            {
                UserLoginTime(user);
                _logger.LogInformation("Başarılı giriş: {UsernameOrEmail}", usernameOrEmail);  // Başarılı giriş
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı giriş hatası: {UsernameOrEmail}", usernameOrEmail);  // Hata loglama
                return null;
            }
        }

        public async Task SetUserCookieAsync(string username, string email, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(10)
            };

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                _logger.LogInformation("Kullanıcı giriş yaptı: {Username}, E-posta: {Email}", username, email);

                if (rememberMe)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    };

                    httpContext.Response.Cookies.Append("UserInfo", $"{username}|{email}", cookieOptions);
                    _logger.LogInformation("UserInfo çerezi oluşturuldu: {Username}, {Email}", username, email);
                }
            }
        }

        public async Task<bool> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                _logger.LogWarning("Geçersiz e-posta formatı: {Email}", email); // Uyarı loglama
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            Console.WriteLine($"DEBUG: Gelen e-posta adresi: {email}");

            if (user == null)
            {
                _logger.LogWarning("Şifre sıfırlama talebi, kullanıcı bulunamadı: {Email}", email); // Hata loglama
                // Kullanıcı bulunamasa bile rastgele gecikme ile mesaj dön
                await Task.Delay(new Random().Next(1500, 3000));
                return true;
            }

            _logger.LogInformation("Şifre sıfırlama talebi: {Email}", email);
            // Şifre sıfırlama token'ı oluştur
            string resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(60);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Şifre sıfırlama token'ı oluşturuldu: {ResetToken} Kullanıcı: {Email}", resetToken, email);

            // Şifre sıfırlama linki oluştur
            var resetLink = $"https://localhost:7070/Account/ConfirmPassword?token={resetToken}";
            var emailBody = $"Şifrenizi sıfırlamak için bağlantıya tıklayın:<br><br> <a href='{resetLink}'>Şifreyi Sıfırla</a>";

            try
            {
                await _emailService.SendEmailAsync(email, "Şifre Sıfırlama", emailBody);
                _logger.LogInformation("Şifre sıfırlama e-postası yollandı: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("E-posta gönderme hatası: {Email}" + " " + email, ex.Message);
                Console.WriteLine("E-posta gönderme hatası: " + ex.Message);
                return false;
            }

            return true;
        }

        public User? GetUserByResetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            return _context.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);
        }

        public bool ResetPassword(string token, string newPassword, string confirmPassword, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                errorMessage = "Geçersiz veya süresi dolmuş token.";
                _logger.LogWarning("Geçersiz veya süresi dolmuş token: {Token}", token);  // Uyarı loglama
                return false;
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);

            Console.WriteLine(user.Username + " " + user.ResetPasswordToken + " " + user.ResetPasswordTokenExpiry + " " + DateTime.UtcNow);

            if (user == null)
            {
                errorMessage = "Geçersiz veya süresi dolmuş token.";
                _logger.LogWarning("Token geçersiz veya süresi dolmuş: {Token}", token);  // Uyarı loglama
                return false;
            }

            // Şifre kontrolleri
            if (string.IsNullOrEmpty(newPassword))
            {
                errorMessage = "Parola gereklidir.";
                return false;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                errorMessage = "Parolayı onaylamak gereklidir.";
                return false;
            }

            if (newPassword.Length < 8)
            {
                errorMessage = "Parola en az 8 karakter uzunluğunda olmalıdır.";
                return false;
            }

            if (!Regex.IsMatch(newPassword, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$"))
            {
                errorMessage = "Parola en az bir büyük harf, bir rakam ve bir özel karakter içermelidir.";
                return false;
            }

            if (newPassword != confirmPassword)
            {
                errorMessage = "Parolalar eşleşmiyor.";
                _logger.LogWarning("Şifre eşleşmiyor: {Token}", token);  // Loglama
                return false;
            }

            // Yeni şifreyi hash'le ve kaydet
            string salt = GenerateSalt();
            string hashedPassword = HashPassword(newPassword, salt);

            user.PasswordHash = hashedPassword;
            user.PasswordSalt = salt;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            _context.SaveChanges();
            errorMessage = "Şifreniz başarıyla sıfırlandı.";
            Console.WriteLine("Şifreniz başarıyla sıfırlandı.");
            _logger.LogInformation("Şifre başarıyla sıfırlandı: {Username}", user.Username);
            return true;
        }
    }
}