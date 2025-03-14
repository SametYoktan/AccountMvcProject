namespace School.Models
{
    public class Role
    {
        // Rolün benzersiz kimliği
        public int Id { get; set; }

        // Rolün adı (Örnek: Admin, User, Moderator vs.)
        public string Name { get; set; }

        // Rolün açıklaması
        public string Description { get; set; }

        // Bu rolün ilişkilendirildiği kullanıcılar (Many-to-Many ilişkisi)
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
