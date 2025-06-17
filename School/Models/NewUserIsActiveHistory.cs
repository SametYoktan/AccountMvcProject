using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserIsActiveHistory
	{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public NewUsers User { get; set; }  // Navigation property

		public string? Token { get; set; }

		[Required(ErrorMessage = "Durum aktif mi? bilgisi gereklidir.")]
        public bool IsUsed { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

		public DateTime? ExpiryDate { get; set; }

		public int? IsActiveId { get; set; } // Nullable

		[StringLength(250, ErrorMessage = "Açıklama 250 karakteri geçemez.")]
		public string Description { get; set; }
	}
}