using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class NewRoles
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol adı boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string Description { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        // İlişki: Bu role ait kullanıcı-rol ilişkileri
        public List<NewUserRoles> UserRoles { get; set; } = new List<NewUserRoles>();

    }
}
