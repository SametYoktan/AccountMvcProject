using School.Models;

namespace School.Services
{
    public interface IAccountServices
    {
        bool UserIsnameControl(string username);//Kullanıcı Adı Db de Varmı Yokmu Kontrol Eder
        bool UserIsEmailControl(string email);//E-Mail Db de Varmı Yokmu Kontrol Eder
        User? UserIsnameAndEmailControl(string username, string email);//Burası Kullanıcı Adı Ve Mail'i Direk Çerezdeki Veriden Çekiyor Textboxtan Değil O yüzden model.Username Yazmadık
        string HashPassword(string password, string salt);
        string GenerateSalt();
        void UserLoginTime(User user);
        Task UserLogOutAsync();

        void UserRegister(User model);

        User? UserLoginControl(string userInfo);
        User? UserLogin(string usernameOrEmail, string password);
        Task SetUserCookieAsync(string username, string email, bool rememberMe);

        Task<bool> ForgotPassword(string email);

        User? GetUserByResetToken(string token);
        bool ResetPassword(string token, string newPassword, string confirmPassword, out string errorMessage);
    }
}
