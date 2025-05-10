using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Services.IServices
{
    public interface IUserServices
    {
        Task RegisterUser(UserRegisterRequest payload, byte[] updatedBy);
        Task<bool> CheckLoginCredentials(UserLoginRequest login);
    }
}
