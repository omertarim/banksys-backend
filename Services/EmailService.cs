using MailKit.Net.Smtp;
using MimeKit;

namespace BankSysAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendPasswordResetEmail(string toEmail, string resetLink)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "Password Reset Request";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $"<p>To reset your password, click the link below:</p><p><a href='{resetLink}'>Reset Password</a></p>"
                };

                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_config["Email:From"], _config["Email:AppPassword"]);
                smtp.Send(email);
                smtp.Disconnect(true);

                Console.WriteLine("✔ Email sent successfully to: " + toEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email sending failed: " + ex.Message);
            }
        }
    }
}
