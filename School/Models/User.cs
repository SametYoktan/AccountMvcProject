using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class User
    {
        public int Id { get; set; } // Id auto-increment, veritabanı otomatik oluşturur

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")] // Bu Alan Boş Olamaz
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta gereklidir.")] // Bu Alan Boş Olamaz
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")] // Bu Alan E Posta Şeklinde Olmalıdır
        public string Email { get; set; }

        public string? PasswordHash { get; set; } //Bu, kullanıcının şifresinin hash'lenmiş halini tutar

        public string? PasswordSalt { get; set; } //Bu, aynı şifrelerin bile farklı hash'lerle şifrelenmesini sağlar

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Bu, kullanıcının hesabının oluşturulma tarihini tutar Varsayılan olarak geçerli tarih
        public DateTime? LastLogin { get; set; } = DateTime.Now; // Bu, kullanıcının son giriş tarihini tutar. DateTime? yazımı, bu alanın null (boş) değer alabileceğini belirtir

        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif
        public bool IsDeleted { get; set; } = false; // Varsayılan olarak silinmemiş
        public bool? PhoneNumber { get; set; } // Varsayılan olarak silinmemiş

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
        public bool? AgreeTerms { get; set; }

        [NotMapped]
        public bool? RememberMe { get; set; }

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }


    }
}
