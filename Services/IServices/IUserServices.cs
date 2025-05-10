using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Services.IServices
{
    public interface IUserServices
    {
        
        #region Read Services

        Task<Guid> CheckLoginCredentials(UserLoginRequest login);

        #endregion
        
        #region Create Services

        Task RegisterUser(UserRegisterRequest payload, Guid updatedBy);

        #endregion
    }
}
