namespace DemoFYP.Services.IServices
{
    public interface IEmailServices
    {
        Task<bool> SendEmailAsync(string toEmail, string tempPassword, string subject, string body);
    }
}
