using DemoFYP.Models;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IEmailRepositories
    {
        Task InsertEmailLog(EmailLog emailLog);
    }
}
