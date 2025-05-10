using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IUserRepository
    {
        #region Read Repositories

        Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext = null);
        Task<Guid> CheckUserLoginCredentials(UserLoginRequest payload);

        #endregion

        #region Create Repositories
        Task RegisterUser(UserRegisterRequest registerDTO, Guid updatedBy, AppDbContext outerContext = null);

        #endregion
    }
}
