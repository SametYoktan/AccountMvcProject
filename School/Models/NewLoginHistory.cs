using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewLoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        [ForeignKey("NewUsers")]
        public int UserID { get; set; }

        public NewUsers _newuser { get; set; }  // Navigation property

        public DateTime LoginTime { get; set; } = DateTime.Now;

        public DateTime? LogoutTime { get; set; } // Nullable

        [Required(ErrorMessage = "Giriş tipi boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Giriş tipi en fazla 50 karakter olabilir.")]
        public string Type { get; set; }
    }
}
