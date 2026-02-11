using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YourApp.Services;

public class EmailService
{
    private readonly string _fromEmail = "youremail@example.com"; // replace with a real sender
    private readonly string _smtpHost = "smtp.example.com";       // replace with your SMTP server
    private readonly int _smtpPort = 587;                         // typical SMTP port
    private readonly string _smtpUsername = "youremail@example.com";
    private readonly string _smtpPassword = "password";

    public async Task SendReminderEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body);
        await client.SendMailAsync(mailMessage);
    }
}
