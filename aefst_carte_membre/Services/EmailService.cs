using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using aefst_carte_membre.Models;

namespace aefst_carte_membre.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAccountCreatedEmail(
            string toEmail,
            string passwordTemp,
            byte[] cartePdfBytes,
            string carteFileName
        )
        {
            var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password
                ),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.From),
                Subject = "Vos accès et votre carte AEFST",
                Body = $@"
Bonjour,

Votre compte AEFST a été créé.

Email : {toEmail}
Mot de passe temporaire : {passwordTemp}

⚠️ Vous devrez changer votre mot de passe à la première connexion.

Votre carte de membre est jointe à cet email (PDF).

AEFST
",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            // 📎 PIÈCE JOINTE PDF
            var pdfStream = new MemoryStream(cartePdfBytes);
            var attachment = new Attachment(
                pdfStream,
                carteFileName,
                "application/pdf"
            );

            message.Attachments.Add(attachment);

            await client.SendMailAsync(message);
        }
    }
}
