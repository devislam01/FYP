using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IUserServices
    {
        
        #region Read Services

        Task<UserJwtClaims> CheckLoginCredentials(UserLoginRequest login);

        Task<UserDetailResponse> GetUserProfile(Guid curUserID);

        Task<UserPermissionResponse> GetPermissionsList();

        Task<UserPermissionResponse> GetAdminPermissionsList();

        Task<UserPermissionResponse> GetUserPermissions(Guid curUserID);

        Task<PagedResult<User>> GetUserList(PaginationRequest pagination);

        #endregion

        #region Create Services

        Task RegisterUser(UserRegisterRequest payload, Guid updatedBy);

        #endregion

        #region Update Services

        Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID);
        Task SendTemporilyPassword(string email, Guid curUserID);
        Task ResetPassword(ResetPasswordRequest payload, Guid curUserID);

        #endregion
    }
}
