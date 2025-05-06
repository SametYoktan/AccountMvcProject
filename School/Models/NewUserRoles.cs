namespace School.Models
{
	public class NewUserRoles
	{
		public int ID { get; set; }  // Primary key olarak tanımlanmalı

		// Kullanıcı rolünün ilişkilendirildiği kullanıcı ID'si
		public int UserID { get; set; }

		// Kullanıcı rolünün ilişkilendirildiği rol ID'si
		public int RoleID { get; set; } = 1; ///// 1=Standart,2=Admin

		public DateTime CreateDate { get; set; } = DateTime.Now;  // Rolün kullanıcıya kayıt tarihi
	}
}
