using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IUserRepository
    {
        #region Read Repositories

        Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext = null);
        Task<Guid> CheckUserLoginCredentials(UserLoginRequest payload);
        Task<UserDetailResponse> GetUserProfileByLoginID(Guid CurUserID);

        #endregion

        #region Create Repositories

        Task RegisterUser(UserRegisterRequest registerDTO, Guid updatedBy, AppDbContext outerContext = null);

        #endregion

        #region Update Repositories

        Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID);
        Task UpdatePassword(string email, Guid CurUserID, string password);

        #endregion
    }
}
