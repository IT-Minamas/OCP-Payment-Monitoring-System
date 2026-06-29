using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Helpers
{
    public class EmailHelper
    {
        private readonly SMTPSetting _smtp;

        public EmailHelper(IOptions<SMTPSetting> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task SendAsync(
            List<string> to,
            List<string> cc,
            string subject,
            string body)
        {
            MimeMessage email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _smtp.DisplayName,
                    _smtp.UserName));

            foreach (string item in to)
            {
                email.To.Add(
                    MailboxAddress.Parse(item));
            }

            foreach (string item in cc)
            {
                email.Cc.Add(
                    MailboxAddress.Parse(item));
            }

            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using SmtpClient smtp = new();

            await smtp.ConnectAsync(
                _smtp.Host,
                _smtp.Port,
                _smtp.EnableSSL);

            await smtp.AuthenticateAsync(
                _smtp.UserName,
                _smtp.Password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}