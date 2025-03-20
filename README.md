ASP.NET Core Mvc Servis Mimarili Kullanıcı Yönetim Sistemi Notları
Bu projede ASP.NET Core Mvc kullanarak bir kullanıcı yönetim sistemi geliştirdim. Sistemde kayıt olma, giriş yapma, çıkış yapma, şifre sıfırlama ve çerez yönetimi gibi bir çok web sitsinden kullanılan temel işlemler var.

1. AccountServices.cs
Bütün iş mantığı burada. Kullanıcı işlemleriyle ilgili her şey bu servis içinde çalışıyor.

*Kullanıcı adı ve e-posta kontrolü

UserIsnameControl() → Kullanıcı adı daha önce alınmış mı kontrol ediyor.
UserIsEmailControl() → E-posta adresi sistemde kayıtlı mı bakıyor.

*Şifre İşlemleri

GenerateSalt() → Rastgele salt üretiyor.
HashPassword() → PBKDF2 ile şifreyi hash’liyor.

*Kayıt ve Giriş İşlemleri

UserRegister() → Kullanıcıyı veritabanına kaydediyor.
UserLogin() → Kullanıcı giriş bilgilerini kontrol ediyor.

*Çerez (Cookie) Yönetimi

SetUserCookieAsync() → Kullanıcı giriş yaptığında çerez oluşturuyor.
UserLogOutAsync() → Kullanıcı çıkış yaptığında çerezleri siliyor.

*Şifre Sıfırlama

ForgotPassword() → Kullanıcının şifre sıfırlama isteğini alıyor ve e-posta gönderiyor.
GetUserByResetToken() → Token’a göre kullanıcıyı buluyor.
ResetPassword() → Yeni şifreyi güncelliyor.

2. AccountController.cs
Burada serviste oluşturduğumuz fonksiyonlar çağırılarak işlemler yönlendiriliyor. Bu sayede kod karmaşasından kurtuluyoruz

*Kayıt İşlemleri

Register() → Kayıt formunu gösteriyor.
[HttpPost] Register() → Kullanıcıyı kaydediyor ve giriş sayfasına yönlendiriyor.

*Giriş İşlemleri

Login() → Giriş formunu getiriyor.
[HttpPost] Login() → Kullanıcı bilgilerini doğruluyor ve giriş yapmasını sağlıyor.

*Çıkış İşlemi

Logout() → Kullanıcı oturumunu kapatıyor, çerezleri temizliyor.

*Şifre Sıfırlama

ForgotPassword() → Kullanıcının şifre sıfırlama talebini alıyor.
ConfirmPassword() → Kullanıcı yeni şifre belirlemek için token’ı kontrol ediyor.

3. Güvenlik Önlemleri
Projede güvenlik için şu yöntemleri kullandım:

PBKDF2 ile Şifreleme: Şifreler salt ile birlikte hash’leniyor.
Kimlik Doğrulama: Kullanıcı giriş yaptığında ClaimsIdentity oluşturuluyor.
Şifre Kuralları:
En az 8 karakter
En az 1 büyük harf
En az 1 rakam ve özel karakter
E-posta Doğrulama: Şifre sıfırlama için kullanıcının e-postasına link gidiyor.
Çerez (Cookie) Yönetimi:
RememberMe seçiliyse 30 gün, değilse 10 dakika geçerli.
Çıkışta çerezler temizleniyor.
Kullanıcı işlem yapmazsa oturum otomatik düşüyor.
Textboxların tüm null ve geçerli formatta yazılıp yazılmadığı gibi kontollerin hepsi yapıldı.
