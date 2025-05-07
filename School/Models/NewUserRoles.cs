using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
	public class NewUserRoles
{
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID'si boş bırakılamaz.")]
        [ForeignKey("NewUsers")]
        public int UserID { get; set; }

        public NewUsers _newuser { get; set; }  // Navigation property

        [Required(ErrorMessage = "Rol ID'si boş bırakılamaz.")]
        [ForeignKey("NewRoles")]
        public int RoleID { get; set; }

        public NewRoles _role { get; set; }  // Navigation property

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }

}
