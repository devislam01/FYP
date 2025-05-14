using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IJwtRepository
    {
        Task AddUserToken(Usertoken usertoken);
        Task<Usertoken?> GetUserTokenByRefreshToken(string refreshToken);
        Task<Usertoken?> GetUserTokenByUserId(Guid curUserID);
        Task RevokeUserTokenByRefreshToken(string refreshToken);
        Task RevokeUserByUserID(Guid userID, Guid curUserID);
    }
}
