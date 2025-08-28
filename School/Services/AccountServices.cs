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
using School.Models.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


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
		public bool UserIsnameControl(string email)
		{
			return _context._NewUsers.Any(u => u.Email == email);
		}

		public bool UserIsEmailControl(string email)
		{
			return _context._NewUsers.Any(u => u.Email == email);
		}

		//Burası Kullanıcı Adı Ve Mail'i Direk Çerezdeki Veriden Çekiyor Textboxtan Değil O yüzden model.Username Yazmadık
		public NewUsers? UserEmailControl(string email)
		{
			return _context._NewUsers.FirstOrDefault(u => u.Email == email);
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

		public void UserLoginTime(NewUsers user)
		{
			DateTime authTime = DateTime.UtcNow;
			Console.WriteLine(">>>>> GİRİŞ ZAMANI: " + authTime.ToLocalTime());
			//user.LastLogin = authTime.ToLocalTime();
			_context.SaveChanges();
			//_logger.LogInformation("Kullanıcı giriş yaptı: {Username}, Giriş Zamanı: {AuthTime}", user.Username, authTime);  // Loglama ekliyoruz
		}

		public async Task UserLogOutAsync()
		{
			var httpContext = _httpContextAccessor.HttpContext;

			if (httpContext.User.Identity?.IsAuthenticated == true)
			{
				var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

				var user = await _context._NewUsers.FirstOrDefaultAsync(u => u.Email == email);
				if (user != null)
				{
					//Varolan en son giriş kaydını al
					var lastLogin = await _context._NewLoginHistory
				 .Where(l => l.UserID == user.Id && l.LogoutTime == null)
				 .OrderByDescending(l => l.LoginTime)
				 .FirstOrDefaultAsync();

					// Yeni çıkış kaydı oluştur
					var create_logout_history = new NewLoginHistory
					{
						UserID = user.Id,
						LoginTime = lastLogin.LoginTime,
						LogoutTime = DateTime.Now,
						Type = LoginEnum.Cikis.ToString(),
						LoginId = lastLogin.Id
					};
					await _context._NewLoginHistory.AddAsync(create_logout_history);
					await _context.SaveChangesAsync();

					_logger.LogInformation("Kullanıcı için çıkış kaydı oluşturuldu: {Username}", user.Email);
				}

				await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

				// Çerezleri temizle
				httpContext.Response.Cookies.Delete("UserInfo");
				httpContext.Response.Cookies.Delete(".AspNetCore.Cookies");

				_logger.LogInformation("Kullanıcı çıkış yaptı: {Username}", httpContext.User.Identity.Name);
			}
		}

		void AccountUnlockMail(NewUsers user, string usernameOrEmail,string token) //interface olarak tanımlamadık çünkü sadece burada kullanacağız
		{
			_logger.LogWarning("Kilitli Hesabı Açma Talebi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
			var activationLink = $"https://localhost:7070/Account/ActivateAndRedirect?email={user.Email}&token={token}";
			var emailBody = $"Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:<br><br> <a href='{activationLink}'>Hesabı Aktifleştir</a>";
			//var emailBody = $"Hesabınız Şüpheli Giriş Nedeniyle Kilitlendi Kilidi Açmak için bağlantıya tıklayın:<br><br> <a href=''>Hesabınız Kilitlendi</a>";
			_emailService.SendEmailAsync(user.Email, "Hesabınız Kilitlendi", emailBody);

			var create_email_history = new NewEmailHistory
			{
				UserID = user.Id,
				UserEmail = user.Email,
				Description = user.Email + " Hesabının " + EmailDescriptionEnum.Hesap_Kilitlendi_Maili_Gönderildi.ToString(),
				MailType = EmailTypeEnum.Account.ToString()
			};
			_context._NewEmailHistory.Add(create_email_history);
			_context.SaveChanges();

			_logger.LogWarning("Kilitli Hesaba Ait E-mail Adresine Unlock Maili Gönderildi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
		}
		// Bu İkisi Birleştirilcek Şimdilik Böyle Yazdım Parametreye Ve Enuma Bağla
		//Linkin Gidiş Yolunu Düzelt Hesap Onaylandı CsHtml Oluştur
		void AccountConfirmationMail(NewUsers user, string usernameOrEmail, string token) //interface olarak tanımlamadık çünkü sadece burada kullanacağız
		{
			_logger.LogWarning("Hesap Onaylama Talebi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
			var activationLink = $"https://localhost:7070/Account/AccountConfirmationRedirect?email={user.Email}&token={token}";
			var emailBody = $"Hesabınızı onaylamak için aşağıdaki bağlantıya tıklayın:<br><br> <a href='{activationLink}'>Hesabı Onayla</a>";
			//var emailBody = $"Hesabınız Şüpheli Giriş Nedeniyle Kilitlendi Kilidi Açmak için bağlantıya tıklayın:<br><br> <a href=''>Hesabınız Kilitlendi</a>";
			_emailService.SendEmailAsync(user.Email, "Hesabınızı Onaylamanız Gerekmektedir", emailBody);

			var create_email_history = new NewEmailHistory
			{
				UserID = user.Id,
				UserEmail = user.Email,
				Description = user.Email + " Hesabının " + EmailDescriptionEnum.Hesap_Onaylama_Maili_Gönderildi.ToString(),
				MailType = EmailTypeEnum.Account_Confirmation.ToString()
			};
			_context._NewEmailHistory.Add(create_email_history);
			_context.SaveChanges();

			_logger.LogWarning("Hesap Onaylama Talebi E-mail Adresine Unlock Maili Gönderildi: {UsernameOrEmail}", usernameOrEmail);  // Hatalı giriş denemesi
		}
		#endregion

		public void UserRegister(NewUsers model)
		{
			try
			{
				string salt = GenerateSalt();
				string hashedPassword = HashPassword(model.Password, salt);
				model.PasswordSalt = salt;
				model.PasswordHash = hashedPassword;
				_context._NewUsers.Add(model);
				_context.SaveChanges();
				_logger.LogInformation("Yeni kullanıcı kaydedildi: {Username}", model.Email);  // Loglama ekliyoruz

				var create_user_role = new NewUserRoles
				{
					UserID = model.Id,
					RoleID = (int)UserRolesEnum.Standard //Standart Rol İle Kayıt Oluştur
				};
				_context._NewUserRoles.Add(create_user_role);
				_context.SaveChanges();
				_logger.LogInformation("Yeni kullanıcı NewUserRole tablosuna standart rol kayıtı yapıldı: {Username}", model.Email);  // Loglama ekliyoruz
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu! Kullanıcı adı: {Username}", model.Email);  // Hata loglama
			}
		}

		public NewUsers? UserLoginControl(string userInfo)
		{
			if (string.IsNullOrEmpty(userInfo))
				return null;

			// Artık sadece email var, split'e gerek yok
			string email = userInfo.Trim();

			var user = UserEmailControl(email);

			if (user == null)
			{
				_logger.LogWarning("Giriş denemesi başarısız: {Email}", email);
				return null;
			}

			UserLoginTime(user);
			return user;
		}//LOGİN GET

		public NewUsers? UserLogin(string userEmail, string password)//LOGİN POST
		{
			var user = UserEmailControl(userEmail);

			if (user == null)//SİSTEMDE OLMAYAN KULLANICI GİRİŞ YAPMAYA ÇALIŞIRSA BU LOG DÖNER
			{
				_logger.LogWarning("Başarısız giriş denemesi: {UsernameOrEmail}", userEmail);  // Hatalı giriş denemesi
				return null;
			}

			var hashedPassword = HashPassword(password, user.PasswordSalt);

			if (user.PasswordHash != hashedPassword) // Eğer hash'ler eşleşmezse
			{
				_logger.LogWarning("Yanlış şifre girildi: {UsernameOrEmail}", userEmail);  // Yanlış şifre loglaması
				user.LoginErrorNumber++;
				_context.SaveChanges();
				_logger.LogWarning("Hatalı giriş sayısı: {user.LoginErrorNumber}", user.LoginErrorNumber);  // Yanlış şifre loglaması
				if (user.LoginErrorNumber >= 5 && user.IsActive)
				{
					user.IsActive = false;

					string isactiveToken = Guid.NewGuid().ToString();

					var create_IsActive_history = new NewUserIsActiveHistory
					{
						UserID = user.Id,
						IsUsed = false,
						Token = isactiveToken,
						Description = IsUserActiveDescription.Hesap_Kilitlendi.ToString(),
						ExpiryDate = DateTime.Now.AddMinutes(60),
					};
					_context._NewUserIsActiveHistory.Add(create_IsActive_history);
					_context.SaveChanges();
					_logger.LogWarning("Hesap kilitlendi: {UsernameOrEmail}", userEmail);  // Yanlış şifre loglaması
					AccountUnlockMail(user, userEmail, isactiveToken);
				}
				return null; // Hatalı şifre, null döndürülür
			}

			if (!user.IsActive)
			{
				_logger.LogWarning("Kilitli hesaba giriş denemesi: {UsernameOrEmail}", userEmail);  // Hatalı giriş denemesi

				string isUseractiveToken = Guid.NewGuid().ToString();

				var create_IsActive_history = new NewUserIsActiveHistory
				{
					UserID = user.Id,
					IsUsed = false,
					Token = isUseractiveToken,
					Description = IsUserActiveDescription.Hesap_Kilitlendi.ToString(),
					ExpiryDate = DateTime.Now.AddMinutes(60),
				};
				_context._NewUserIsActiveHistory.Add(create_IsActive_history);
				_context.SaveChanges();
				AccountUnlockMail(user, userEmail, isUseractiveToken);
				return null; //BURDA İSTERSEK HATALARDA USERLARI DÖNDÜRÜP DAHA DETAYLI LOG TUTABİLİRİZ AMA ŞUAN DA TUTUYORUZ DETAYLI LOG
				//KİTLİ HESABA GİRİŞ YAPILMAYA ÇALIŞILIRSA OTOMATİK HESABI AKTİF ETMEK İÇİN MAİL GÖNDERİLSİN
			}

			if(!user.IsEmailConfirmed)
			{
				_logger.LogWarning("Hesap Aktifleştirilmemiştir: {UsernameOrEmail}", userEmail);  // Hesap Aktifleştirilmemiştir Loglaması

				string isAccountConfirmationToken = Guid.NewGuid().ToString();

				var create_IsAccountConfirmation_history = new NewAccountConfirmationHistory
				{
					UserID = user.Id,
					IsUsed = false,
					Token = isAccountConfirmationToken,
					Description = IsAccountConfirmationDescription.Hesap_Onaylama.ToString(),
					ExpiryDate = DateTime.Now.AddMinutes(60),
				};
				_context._NewAccountConfirmationHistory.Add(create_IsAccountConfirmation_history);
				_context.SaveChanges();
				AccountConfirmationMail(user, userEmail, isAccountConfirmationToken);
				return null; //User Null DÖnsün
			}

			// Başarılı giriş
			try
			{
				UserLoginTime(user);

				var create_login_history = new NewLoginHistory
				{
					UserID = user.Id,
					Type = LoginEnum.Giris.ToString()
				};
				_context._NewLoginHistory.Add(create_login_history);
				_context.SaveChanges();

				_logger.LogInformation("Başarılı giriş: {UsernameOrEmail}", userEmail);  // Başarılı giriş
				return user;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Kullanıcı giriş hatası: {UsernameOrEmail}", userEmail);  // Hata loglama
				return null;
			}
		}

		public async Task SetUserCookieAsync(string email, string name, string surname, bool rememberMe, string role)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, name+" "+surname),
				new Claim(ClaimTypes.Email, email),
				new Claim(ClaimTypes.Role,role)
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var authProperties = new AuthenticationProperties
			{
				IsPersistent = rememberMe,
				ExpiresUtc = rememberMe ? DateTime.UtcNow.AddMinutes(((int)SessionTimeoutDurationEnum.Long)) : DateTime.UtcNow.AddMinutes(((int)SessionTimeoutDurationEnum.Short))//Sayfada İşlem Yapılmadığında Bu Süre Sonunda Kullanıcıyı At
			};

			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext != null)
			{
				await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
				_logger.LogInformation("Kullanıcı giriş yaptı: E-posta: {Email}", email);

				if (rememberMe)
				{
					var cookieOptions = new CookieOptions
					{
						Expires = DateTime.UtcNow.AddDays(30),
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax
					};

					httpContext.Response.Cookies.Append("UserInfo", $"{email}", cookieOptions);
					_logger.LogInformation("UserInfo çerezi oluşturuldu: {Email}", email);
				}
			}
		}

		public async Task<bool> ForgotPassword(string email)
		{
			if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
			{
				_logger.LogWarning("Geçersiz e-posta formatı: {Email}", email);
				return false;
			}

			var user = await _context._NewUsers.FirstOrDefaultAsync(u => u.Email == email);
			Console.WriteLine($"DEBUG: Gelen e-posta adresi: {email}");

			if (user == null)
			{
				_logger.LogWarning("Şifre sıfırlama talebi, kullanıcı bulunamadı: {Email}", email);
				await Task.Delay(new Random().Next(1500, 3000));
				return true;
			}

			_logger.LogInformation("Şifre sıfırlama talebi: {Email}", email);

			// Yeni şifre sıfırlama token'ı oluşturuluyor
			string resetToken = Guid.NewGuid().ToString();
			var token = new NewPasswordHistory
			{
				UserID = user.Id,
				Token = resetToken,
				ExpiryDate = DateTime.Now.AddMinutes(60)
			};

			_context._NewPasswordHistory.Add(token);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Şifre sıfırlama token'ı oluşturuldu: {ResetToken} Kullanıcı: {Email}", resetToken, email);

			// Şifre sıfırlama linki oluşturuluyor
			var resetLink = $"https://localhost:7070/Account/ConfirmPassword?token={resetToken}";
			var emailBody = $"Şifrenizi sıfırlamak için bağlantıya tıklayın:<br><br> <a href='{resetLink}'>Şifreyi Sıfırla</a>";

			try
			{
				await _emailService.SendEmailAsync(email, "Şifre Sıfırlama", emailBody);

				var create_email_history = new NewEmailHistory
				{
					UserID = user.Id,
					UserEmail = email,
					Description = email + " Hesabının " + EmailDescriptionEnum.Şifre_Sıfırlama_Maili_Gönderildi.ToString(),
					MailType = EmailTypeEnum.PassWord.ToString()
				};
				_context._NewEmailHistory.Add(create_email_history);
				_context.SaveChanges();

				_logger.LogInformation("Şifre sıfırlama e-postası yollandı: {Email}", email);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "E-posta gönderme hatası: {Email}", email);
				Console.WriteLine("E-posta gönderme hatası: " + ex.Message);
				return false;
			}

			return true;
		}

		public NewPasswordHistory? GetUserByResetToken(string token)
		{
			if (string.IsNullOrEmpty(token))
				return null;


			return _context._NewPasswordHistory.FirstOrDefault(u => u.Token == token && u.ExpiryDate > DateTime.Now);
		}

		public NewUserIsActiveHistory? GetUserIsActiveToken(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				Console.WriteLine("token geçersiz");
				return null;
			}
			return _context._NewUserIsActiveHistory.FirstOrDefault(u => u.Token == token && u.ExpiryDate > DateTime.Now);
		}

		public NewAccountConfirmationHistory? GetAccountIsActiveToken(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				Console.WriteLine("token geçersiz");
				return null;
			}
			return _context._NewAccountConfirmationHistory.FirstOrDefault(u => u.Token == token && u.ExpiryDate > DateTime.Now);
		}

		public bool ResetPassword(string token, string newPassword, string confirmPassword, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (string.IsNullOrEmpty(token))
			{
				errorMessage = "Geçersiz veya süresi dolmuş token.";
				Console.WriteLine(errorMessage);
				_logger.LogWarning("Geçersiz veya süresi dolmuş token: {Token}", token);
				return false;
			}

			var PasswordToken = _context._NewPasswordHistory.FirstOrDefault(u => u.Token == token && u.ExpiryDate > DateTime.Now);

			if (PasswordToken == null)
			{
				errorMessage = "Geçersiz veya süresi dolmuş token.";
				_logger.LogWarning("Token geçersiz veya süresi dolmuş: {Token}", token);
				return false;
			}
			var user2 = _context._NewUsers.FirstOrDefault(u => u.Id == PasswordToken.UserID);
			if (user2 == null)
			{
				errorMessage = "Kullanıcı bulunamadı.";
				return false;
			}

			if (PasswordToken.UserID != user2.Id)
			{
				errorMessage = "Token kullanıcıyla eşleşmiyor.";
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
				_logger.LogWarning("Şifre eşleşmiyor: {Token}", token);
				return false;
			}

			// Şifre güncelleme
			string salt = GenerateSalt();
			string hashedPassword = HashPassword(newPassword, salt);

			user2.PasswordHash = hashedPassword;
			user2.PasswordSalt = salt;

			// Token’ı devre dışı bırak
			PasswordToken.ExpiryDate = DateTime.Now.AddMinutes(-1);
			PasswordToken.IsUsed = true;

			_context.SaveChanges();

			_logger.LogInformation("Şifre başarıyla sıfırlandı: {Email}", user2.Email);
			return true;
		}

		public async Task<bool> ActivateAndRedirect(string email)
		{
			var user = await _context._NewUsers.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				_logger.LogInformation("Kullanıcı bulunamadı: {Email}", email);
				return false;
			}

			user.IsActive = true;
			user.LoginErrorNumber = 0;

			var lastisactive = await _context._NewUserIsActiveHistory
				 .Where(l => l.UserID == user.Id && l.ExpiryDate != null)
				 .OrderByDescending(l => l.CreateDate)
				 .FirstOrDefaultAsync();

			lastisactive.IsUsed = true;

			var create_IsActive_history = new NewUserIsActiveHistory
			{
				UserID = user.Id,
				//IsUsed = true,
				Description= IsUserActiveDescription.Hesap_Aktifleştirildi.ToString(),
				IsActiveId=lastisactive.ID,			
			};
			_context._NewUserIsActiveHistory.Add(create_IsActive_history);

			await _context.SaveChangesAsync();

			_logger.LogInformation("Kullanıcı Hesabı Aktifleştirildi: {Email}", email);

			return true;
		}

		public async Task<bool> AccountActivateAndRedirect(string email)
		{
			var user = await _context._NewUsers.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				_logger.LogInformation("Kullanıcı bulunamadı: {Email}", email);
				return false;
			}

			user.IsEmailConfirmed = true;

			var lastisactive = await _context._NewAccountConfirmationHistory
				 .Where(l => l.UserID == user.Id && l.ExpiryDate != null)
				 .OrderByDescending(l => l.CreateDate)
				 .FirstOrDefaultAsync();

			lastisactive.IsUsed = true;

			var create_account_IsActive_history = new NewAccountConfirmationHistory
			{
				UserID = user.Id,
				IsUsed = true,
				Description = IsAccountConfirmationDescription.Hesap_Onaylandı.ToString(),
				IsActiveId = lastisactive.ID,
			};
			_context._NewAccountConfirmationHistory.Add(create_account_IsActive_history);

			await _context.SaveChangesAsync();

			_logger.LogInformation("Kullanıcı Hesabı Onaylandı: {Email}", email);

			return true;
		}
	}
} 