using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IUserRepository
    {
        #region Read Repositories

        Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext = null);
        Task<UserJwtClaims> CheckUserLoginCredentials(UserLoginRequest payload);
        Task<Guid> CheckIfUserLogin(string refreshToken);
        Task<UserDetailResponse> GetUserProfileByLoginID(Guid curUserID);
        Task<User> GetUserByLoginID(Guid curUserID, AppDbContext outerContext = null);
        Task<UserPermissionResponse> GetPermissions();
        Task<UserPermissionResponse> GetAdminPermissions();
        Task<UserPermissionResponse> GetUserPermissionsByLoginID(Guid curUserID);
        Task<PagedResult<UserListResponse>> GetUserList(UserListFilterRequest filter);
        Task<EditUserDetailsResponse> GetUserDetails(Guid userID);

        #endregion

        #region Create Repositories

        Task RegisterUser(UserRegisterRequest registerDTO, Guid updatedBy, AppDbContext outerContext = null);

        #endregion

        #region Update Repositories

        Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID);
        Task UpdateTempPassword(string email, Guid curUserID, string password);
        Task<string> UpdatePassword(Guid curUserID, string password);
        Task<string> UpdatePassword(Guid userID, Guid curUserID, string password);

        #endregion
    }
}
