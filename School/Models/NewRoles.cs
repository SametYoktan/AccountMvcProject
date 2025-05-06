namespace School.Models
{
	public class NewRoles
	{
		// Rolün benzersiz kimliği
		public int ID { get; set; }

		// Rolün adı (Örnek: Admin, User, Moderator vs.)
		public string Name { get; set; }

		// Rolün açıklaması
		public string Description { get; set; }

		public DateTime CreateDate { get; set; } = DateTime.Now;  // Rolün kayıt tarihi
	}
}
