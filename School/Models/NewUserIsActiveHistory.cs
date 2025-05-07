using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserIsActiveHistory
	{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        [ForeignKey("NewUsers")]
        public int UserID { get; set; }

        public NewUsers _newuser { get; set; }  // Navigation property

        [Required(ErrorMessage = "Durum aktif mi? bilgisi gereklidir.")]
        public bool IsUsed { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
