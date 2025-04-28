namespace School.Models
{
	public class NewUserIsActiveHistory
	{
		public int ID { get; set; }
		public int UserID { get; set; }
		public bool IsUsed { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.UtcNow;  // Rolün kayıt tarihi
	}
}
