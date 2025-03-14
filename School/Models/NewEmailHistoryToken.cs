using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewEmailHistoryToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public NewUser User { get; set; } // Kullanıcı ile ilişkilendirme

        public string Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Token oluşturulma zamanı
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false; // Kullanıldı mı?    }
    }
}