using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IJwtServices
    {
        Task<JwtAuthResult> GenerateToken(UserJwtClaims claims);
        Task<bool> LogoutUserByRevokeToken(Guid userID);
        Task<JwtAuthResult> VerifyAndGenerateRefreshToken(RefreshTokenRequest payload);
        Task RevokeUser(Guid userID, Guid curUserID);
        Task ReinstateUser(Guid userID, Guid curUserID);
    }
}
