namespace DemoFYP.Services.IServices
{
    public interface IEmailServices
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
