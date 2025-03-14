using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewUserLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public NewUser User { get; set; } // Kullanıcı ile ilişkilendirme

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime? LogoutTime { get; set; }
    }
}
