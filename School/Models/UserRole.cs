namespace School.Models
{
    public class UserRole
    {
        public int Id { get; set; }  // Primary key olarak tanımlanmalı

        // Kullanıcı rolünün ilişkilendirildiği kullanıcı ID'si
        public int UserId { get; set; }

        // Bu kullanıcıyı temsil eden User objesi
        public User User { get; set; }

        // Kullanıcı rolünün ilişkilendirildiği rol ID'si
        public int RoleId { get; set; }

        // Bu rolü temsil eden Role objesi
        public Role Role { get; set; }

	}
}
