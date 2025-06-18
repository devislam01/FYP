using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IJwtRepository
    {
        Task AddOrUpdateUserToken(Usertoken usertoken);
        Task<UserJwtClaims> GetUserClaimsByRefreshToken(string refreshToken);
        Task<Usertoken?> GetUserTokenByUserId(Guid curUserID);
        Task RevokeUserTokenByRefreshToken(string refreshToken);
        Task<bool> RevokeUserTokenByUserID(Guid userID);
        Task RevokeUserByUserID(Guid userID, Guid curUserID);
        Task ReinstateUserByUserID(Guid userID, Guid curUserID);
    }
}
