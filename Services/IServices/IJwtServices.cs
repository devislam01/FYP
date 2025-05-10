using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IJwtServices
    {
        Task<JwtAuthResult> GenerateToken(UserLoginRequest payload, Guid curUserID);
        /*Task<JwtAuthResult> VerifyAndGenerateToken();*/
    }
}
