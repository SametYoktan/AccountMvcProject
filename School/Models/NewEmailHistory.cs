using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewEmailHistory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        [ForeignKey("NewUsers")]
        public int UserID { get; set; }

        public NewUsers _newuser { get; set; }  // Navigation property

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(100, ErrorMessage = "E-posta adresi 100 karakteri geçemez.")]
        public string UserEmail { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama 250 karakteri geçemez.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Mail tipi gereklidir.")]
        [StringLength(50, ErrorMessage = "Mail tipi 50 karakteri geçemez.")]
        public string MailType { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}