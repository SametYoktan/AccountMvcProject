using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewPasswordHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Token oluşturulma zamanı
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false; // Kullanıldı mı?    }
    }
}
