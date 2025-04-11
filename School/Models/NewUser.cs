using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewUser
    {
        public int Id { get; set; }  // Kullanıcının benzersiz ID'si

        [Required(ErrorMessage = "Kullanıcı adı gereklidir."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; }  // Kullanıcının e-posta adresi

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string Name { get; set; }  // Kullanıcının e-posta adresi

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string Surname { get; set; }  // Kullanıcının e-posta adresi

        public string? PasswordHash { get; set; } //Bu, kullanıcının şifresinin hash'lenmiş halini tutar

        public string? PasswordSalt { get; set; } //Bu, aynı şifrelerin bile farklı hash'lerle şifrelenmesini sağlar

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Kullanıcının kayıt olma tarihi

        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif

        public bool IsDeleted { get; set; } = false; // Varsayılan olarak silinmemiş

        public int LoginErrorNumber { get; set; }

        [Phone(ErrorMessage = "Geçerli bir numara girin.")]
        public string? PhoneNumber { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;  // Kullanıcının e-posta doğrulandı mı?

        public string Role { get; set; } = "Customer"; // Varsayılan olarak müşteri (Müşteri (Customer),Satıcı (Seller),Yönetici (Admin))

        //[NotMapped] özelliği, bu alanın veritabanında bir sütun olarak tutulmayacağını belirtir.
        //Yani bu alan sadece uygulama tarafında kullanılır. Bu örnekte, kullanıcının şifresini tutar ve genellikle şifreyi doğrulamak amacıyla kullanılır.
        [NotMapped]
        [Required(ErrorMessage = "Parola gereklidir.")]
        [MinLength(8, ErrorMessage = "Parola en az 8 karakter uzunluğunda olmalıdır.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$", ErrorMessage = "Parola en az bir büyük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Parolayı onaylamak gereklidir.")]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Kullanım şartlarını kabul etmeniz gerekmektedir.")]
        public bool AgreeTerms { get; set; }


        // Kullanıcının giriş-çıkış geçmişi
        public List<NewUserLoginHistory> LoginHistories { get; set; } = new List<NewUserLoginHistory>();

        // Kullanıcının yaptığı şifre sıfırlama istekleri
        public List<NewPasswordResetHistoryToken> ResetHistoriesTokens { get; set; } = new List<NewPasswordResetHistoryToken>();

        // Kullanıcının yaptığı şifre sıfırlama istekleri
        public List<NewEmailHistoryToken> EmailHistoriesTokens { get; set; } = new List<NewEmailHistoryToken>();
    }
}