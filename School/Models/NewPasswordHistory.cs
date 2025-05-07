using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewPasswordHistory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public NewUsers User { get; set; }  // Navigation property

        [Required(ErrorMessage = "Şifre sıfırlama token'ı gereklidir.")][StringLength(250, ErrorMessage = "Token 250 karakteri geçemez.")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Token kullanıldı mı? bilgisi gereklidir.")]
        public bool IsUsed { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Geçerlilik bitiş tarihi gereklidir.")]
        public DateTime ExpiryDate { get; set; }
    }
}