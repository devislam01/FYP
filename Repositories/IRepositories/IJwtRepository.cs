using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IJwtRepository
    {
        Task AddUserToken(Usertoken usertoken);
    }
}
