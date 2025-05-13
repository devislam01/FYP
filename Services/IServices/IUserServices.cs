using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IUserServices
    {
        
        #region Read Services

        Task<Guid> CheckLoginCredentials(UserLoginRequest login);

        Task<UserDetailResponse> GetUserProfile(Guid CurUserID);

        #endregion

        #region Create Services

        Task RegisterUser(UserRegisterRequest payload, Guid updatedBy);

        #endregion

        #region Update Services

        Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID);
        Task SendTemporilyPassword(string email, Guid CurUserID);
        Task ResetPassword(ResetPasswordRequest payload, Guid CurUserID);

        #endregion
    }
}
