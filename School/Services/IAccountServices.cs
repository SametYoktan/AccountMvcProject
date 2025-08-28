using School.Models;

namespace School.Services
{
    public interface IAccountServices
    {
        #region YARDIMCI METOTLAR
        bool UserIsnameControl(string username);//Kullanıcı Adı Db de Varmı Yokmu Kontrol Eder
        bool UserIsEmailControl(string email);//E-Mail Db de Varmı Yokmu Kontrol Eder
        NewUsers? UserEmailControl(string email);//Burası Kullanıcı Adı Ve Mail'i Direk Çerezdeki Veriden Çekiyor Textboxtan Değil O yüzden model.Username Yazmadık
        string HashPassword(string password, string salt);
        string GenerateSalt();
        void UserLoginTime(NewUsers user);
        Task UserLogOutAsync();
        #endregion

        void UserRegister(NewUsers model);

		NewUsers? UserLoginControl(string userInfo);
		NewUsers? UserLogin(string usernameOrEmail, string password);
        Task SetUserCookieAsync(string email,string name,string surname, bool rememberMe,string role);

        Task<bool> ForgotPassword(string email);

		NewPasswordHistory? GetUserByResetToken(string token);

		NewUserIsActiveHistory? GetUserIsActiveToken(string token);

		NewAccountConfirmationHistory? GetAccountIsActiveToken(string token);

		bool ResetPassword(string token, string newPassword, string confirmPassword, out string errorMessage);
        Task<bool> ActivateAndRedirect(string email);

		Task<bool> AccountActivateAndRedirect(string email);
	}
}
