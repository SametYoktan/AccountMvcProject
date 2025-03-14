using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace School.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // 1️⃣ **Elle test için alıcı e-posta adresini buraya yaz**
            toEmail = "yoktan4@gmail.com";
            Console.WriteLine($"DEBUG: E-posta gönderiliyor -> {toEmail}");

            // 2️⃣ **Null veya boş kontrolü**
            if (string.IsNullOrEmpty(toEmail))
            {
                Console.WriteLine("HATA: Alıcı e-posta adresi NULL veya BOŞ!");
                throw new ArgumentNullException(nameof(toEmail), "Alıcı e-posta adresi boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                Console.WriteLine("HATA: Alıcı e-posta adresi sadece boşluk içeriyor!");
                throw new ArgumentException("E-posta adresi boş olamaz veya sadece boşluk içeremez!", nameof(toEmail));
            }

            var emailMessage = new MimeMessage();

            // 3️⃣ **Elle test için gönderen e-posta adresini yaz**
            var senderEmail = _emailSettings.SenderEmail ?? "yoktan81@gmail.com";
            var senderName = _emailSettings.SenderName ?? "Test Gönderici";
            Console.WriteLine($"DEBUG: Gönderen adres -> '{senderEmail}'");

            // 4️⃣ **Gönderici e-posta adresini ekle**
            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));

            // 5️⃣ **Alıcı e-posta adresini ekle**
            emailMessage.To.Add(new MailboxAddress("Admin", toEmail));
            emailMessage.Subject = subject;

            // 6️⃣ **E-posta içeriğini oluştur**
            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    Console.WriteLine($"DEBUG: SMTP Bağlantısı kuruluyor -> {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");

                    // 7️⃣ **SMTP sunucusuna bağlan**
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    Console.WriteLine("DEBUG: SMTP sunucusuna bağlandı.");

                    // 8️⃣ **SMTP kimlik doğrulaması yap**
                    await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                    Console.WriteLine("DEBUG: SMTP kimlik doğrulaması başarılı.");

                    // 9️⃣ **E-postayı gönder**
                    await client.SendAsync(emailMessage);
                    Console.WriteLine("DEBUG: E-posta başarıyla gönderildi!");

                    // 🔟 **SMTP bağlantısını kapat**
                    await client.DisconnectAsync(true);
                }
            }
            catch (MailKit.Net.Smtp.SmtpCommandException smtpEx)
            {
                Console.WriteLine($"HATA: SMTP komut hatası - {smtpEx.Message}");
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException protocolEx)
            {
                Console.WriteLine($"HATA: SMTP protokol hatası - {protocolEx.Message}");
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                Console.WriteLine($"HATA: Bağlantı hatası - {socketEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GENEL HATA: {ex.Message}");
            }
        }

    }
}