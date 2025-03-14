namespace School.Models
{
    public class PasswordReset
    {
        // Şifre sıfırlama işleminin benzersiz kimliği
        public int Id { get; set; }

        // Bu şifre sıfırlama işlemiyle ilişkili kullanıcıyı tanımlar
        public int UserId { get; set; }

        // Şifre sıfırlama token'ı, güvenli bir şekilde sıfırlama işlemi için kullanılır
        public string Token { get; set; }

        // Token'ın ne zaman geçerli olduğunu belirtir
        public DateTime ExpirationDate { get; set; }

        // Şifre sıfırlama işleminin oluşturulma tarihi
        public DateTime CreatedAt { get; set; }
    }
}
