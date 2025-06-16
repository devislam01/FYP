using System.Net.Mail;
using System.Net;
using DemoFYP.Services.IServices;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Models;

namespace DemoFYP.Services
{
    public class EmailService : IEmailServices
    {
        private readonly IConfiguration _config;
        private readonly IEmailRepositories _emailRepo;

        public EmailService(IConfiguration config, IEmailRepositories emailRepo) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _emailRepo = emailRepo ?? throw new ArgumentNullException(nameof(emailRepo));
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["Email:From"];
            var appPassword = _config["Email:AppPassword"];

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
            };

            var mail = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            try
            {
                await smtp.SendMailAsync(mail);

                var emailLog = new EmailLog
                {
                    From = fromEmail,
                    To = toEmail,
                    Subject = subject,
                    Body = body,
                    IsSent = true,
                    SentAt = DateTime.Now
                };

                await _emailRepo.InsertEmailLog(emailLog);
                return true;
            }
            catch (Exception ex)
            {
                var emailLog = new EmailLog
                {
                    From = fromEmail,
                    To = toEmail,
                    Subject = subject,
                    Body = body,
                    IsSent = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.Now
                };

                await _emailRepo.InsertEmailLog(emailLog);

                return false;
            }
        }
    }
}
