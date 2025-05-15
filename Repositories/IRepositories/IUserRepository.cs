using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IUserRepository
    {
        #region Read Repositories

        Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext = null);
        Task<UserJwtClaims> CheckUserLoginCredentials(UserLoginRequest payload);
        Task<UserDetailResponse> GetUserProfileByLoginID(Guid curUserID);
        Task<UserPermissionResponse> GetPermissions();
        Task<UserPermissionResponse> GetUserPermissionsByLoginID(Guid curUserID);

        #endregion

        #region Create Repositories

        Task RegisterUser(UserRegisterRequest registerDTO, Guid updatedBy, AppDbContext outerContext = null);

        #endregion

        #region Update Repositories

        Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID);
        Task UpdatePassword(string email, Guid curUserID, string password);

        #endregion
    }
}
