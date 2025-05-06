using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class NewEmailHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string Description { get; set; }
        public string MailType { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.Now;  // Rolün kayıt tarihi
	}
}