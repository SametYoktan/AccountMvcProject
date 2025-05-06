namespace School.Models
{
	public class NewUserActivityLog
	{
		public int ID { get; set; }
		public int UserID { get; set; }
		public string ActionType { get; set; }
		public string ActionDescription { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.Now;  // Rolün kayıt tarihi
	}
}
