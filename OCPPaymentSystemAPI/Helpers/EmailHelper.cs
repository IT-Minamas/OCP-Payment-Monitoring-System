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
            List<ApproverModel> to,
            List<ApproverModel> cc,
            string subject,
            string body)
        {
            MimeMessage email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _smtp.DisplayName,
                    _smtp.UserName));

            foreach (ApproverModel item in to)
            {
                if (!string.IsNullOrWhiteSpace(item.Email))
                {
                    email.To.Add(
                        MailboxAddress.Parse(item.Email));
                }
            }

            foreach (ApproverModel item in cc)
            {
                if (!string.IsNullOrWhiteSpace(item.Email))
                {
                    email.Cc.Add(
                        MailboxAddress.Parse(item.Email));
                }
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