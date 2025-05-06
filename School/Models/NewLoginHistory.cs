using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public DateTime? LogoutTime { get; set; }
        public string Type { get; set; }
	}
}
