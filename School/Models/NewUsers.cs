using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace School.Models
{
    public class NewUsers
    {
        // Kullanıcının benzersiz ID'si
        [Key]
        public int Id { get; set; }

        // E-posta adresi, gerekli ve doğrulama için EmailAddress kullanıldı
        [Required(ErrorMessage = "E-Posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(100, ErrorMessage = "E-posta adresi 100 karakteri geçemez.")]
        public string Email { get; set; }

        // Kullanıcının adı, gerekli ve uzunluk kontrolü yapıldı
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakteri geçemez.")]
        public string Name { get; set; }

        // Kullanıcının soyadı, gerekli ve uzunluk kontrolü yapıldı
        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakteri geçemez.")]
        public string Surname { get; set; }

        // Şifre hash'lenmiş olarak tutulur
        public string? PasswordHash { get; set; }

        // Şifre için kullanılan salt
        public string? PasswordSalt { get; set; }

        // Kullanıcının kayıt olma tarihi, varsayılan olarak şu anki tarih
        [Required(ErrorMessage = "Oluşturulma tarihi gereklidir.")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        // Kullanıcının aktif olup olmadığını belirten bayrak
        public bool IsActive { get; set; } = true;

        // Kullanıcının silinip silinmediğini gösterir (soft delete)
        public bool IsDeleted { get; set; } = false;

        // Hatalı giriş sayısı
        [Range(0, int.MaxValue, ErrorMessage = "Hatalı giriş sayısı geçerli olmalıdır.")]
        public int? LoginErrorNumber { get; set; } = 0;

        // Kullanıcının telefon numarası
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir.")]
        public string PhoneNumber { get; set; }

        // E-posta doğrulama durumu
        public bool IsEmailConfirmed { get; set; } = false;

        // Parola, NotMapped olduğu için yalnızca uygulama tarafında kullanılır
        [NotMapped]
        [Required(ErrorMessage = "Parola gereklidir.")]
        [MinLength(8, ErrorMessage = "Parola en az 8 karakter uzunluğunda olmalıdır.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$", ErrorMessage = "Parola en az bir büyük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string Password { get; set; }

        // Parola onayı, NotMapped olduğu için yalnızca uygulama tarafında kullanılır
        [NotMapped]
        [Required(ErrorMessage = "Parolayı onaylamak gereklidir.")]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        // Kullanım şartlarını kabul etme durumu, NotMapped olduğu için yalnızca uygulama tarafında kullanılır
        [NotMapped]
        [Required(ErrorMessage = "Kullanım şartlarını kabul etmeniz gerekmektedir.")]
        public bool AgreeTerms { get; set; }

        // Kullanıcının giriş-çıkış geçmişi
        public List<NewLoginHistory> LoginHistories { get; set; } = new List<NewLoginHistory>();

        // Kullanıcının yaptığı şifre sıfırlama istekleri
        public List<NewPasswordHistory> ResetHistoriesTokens { get; set; } = new List<NewPasswordHistory>();

        // Kullanıcının yaptığı e-posta işlemleri geçmişi
        public List<NewEmailHistory> EmailHistoriesTokens { get; set; } = new List<NewEmailHistory>();

        // Kullanıcının rol bilgilerini tutar (RoleId üzerinden bağlantı)
        public List<NewUserRoles> UserRoles { get; set; } = new List<NewUserRoles>();

        // Kullanıcının block unblock bilgilerini tutar
        public List<NewUserActionLog> UserActivityLog { get; set; } = new List<NewUserActionLog>();

        // Kullanıcının block unblock bilgilerini tutar
        public List<NewUserIsActiveHistory> UserActivityHistory { get; set; } = new List<NewUserIsActiveHistory>();

		// Kullanıcının block unblock bilgilerini tutar
		public List<NewAccountConfirmationHistory> UserAccountHistory { get; set; } = new List<NewAccountConfirmationHistory>();
	}
}