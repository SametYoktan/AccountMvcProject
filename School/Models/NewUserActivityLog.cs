using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserActivityLog
	{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        [ForeignKey("NewUsers")]
        public int UserID { get; set; }

        public NewUsers _newuser { get; set; }  // Navigation property

        [Required(ErrorMessage = "İşlem türü boş bırakılamaz.")]
        [StringLength(200, ErrorMessage = "İşlem türü en fazla 200 karakter olabilir.")]
        public string ActionType { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string ActionDescription { get; set; }

        public DateTime ActionTime { get; set; } = DateTime.Now;
    }
}
