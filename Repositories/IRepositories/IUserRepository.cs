using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext = null);
        Task RegisterUser(UserRegisterRequest registerDTO, byte[] updatedBy, AppDbContext outerContext = null);

        Task<bool> CheckUserLoginCredentials(UserLoginRequest payload);
    }
}
