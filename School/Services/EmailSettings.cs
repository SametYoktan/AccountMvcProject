namespace School.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }   // SMTP Sunucusu (Örn: smtp.gmail.com)
        public int SmtpPort { get; set; }       // SMTP Portu (Genelde 587)
        public string SmtpUser { get; set; }    // SMTP Kullanıcı Adı (E-posta)
        public string SmtpPass { get; set; }    // SMTP Şifresi
        public string SenderEmail { get; set; } // Gönderen E-posta
        public string SenderName { get; set; }  // Gönderen İsmi
    }
}