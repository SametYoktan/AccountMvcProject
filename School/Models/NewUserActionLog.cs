using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserActionLog
	{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public NewUsers User { get; set; }  // Navigation property

        [Required(ErrorMessage = "İşlem türü boş bırakılamaz.")]
        [StringLength(200, ErrorMessage = "İşlem türü en fazla 200 karakter olabilir.")]
        public string ActionType { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string ActionDescription { get; set; }

        public DateTime ActionTime { get; set; } = DateTime.Now;
    }
}
