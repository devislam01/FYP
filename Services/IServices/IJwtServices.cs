using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IJwtServices
    {
        Task<JwtAuthResult> GenerateToken(UserJwtClaims claims);
        Task<JwtAuthResult> VerifyAndGenerateRefreshToken(RefreshTokenRequest payload, string curUserEmail, string curUserRole);
        Task RevokeUser(Guid userID, Guid curUserID);
        Task ReinstateUser(Guid userID, Guid curUserID);
    }
}
