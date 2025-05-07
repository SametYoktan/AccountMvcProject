using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserRoles
{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public NewUsers User { get; set; }  // Navigation property

        [Required(ErrorMessage = "Rol ID'si boş bırakılamaz.")]
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public NewRoles Role { get; set; }  // Navigation property

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
