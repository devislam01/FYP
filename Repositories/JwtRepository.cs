using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class JwtRepository : IJwtRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;
        public JwtRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddUserToken(Usertoken userToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var existUserToken = await context.Usertokens.OrderByDescending(ut => ut.TokenId).FirstOrDefaultAsync(ut => ut.UserId == userToken.UserId);

                if (existUserToken == null)
                {
                    await context.Usertokens.AddAsync(userToken);
                }
                else
                {
                    existUserToken.AccessToken = userToken.AccessToken;
                    existUserToken.AccessTokenExpiresAt = userToken.AccessTokenExpiresAt;
                    existUserToken.RefreshToken = userToken.RefreshToken;
                    existUserToken.RefreshTokenExpiresAt = userToken.RefreshTokenExpiresAt;
                    existUserToken.CreatedAt = DateTime.Now;
                    existUserToken.IsRevoked = false;
                }

                await context.SaveChangesAsync();
            }
            catch
            {
                throw new InvalidOperationException("Insert token failed");
            }
        }

        public async Task<Usertoken?> GetUserTokenByRefreshToken(string refreshToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Usertokens
                .FirstOrDefaultAsync(t =>
                    t.RefreshToken == refreshToken &&
                    !t.IsRevoked &&
                    t.RefreshTokenExpiresAt > DateTime.Now);
            }
            catch
            {
                throw new InvalidOperationException("Failed to get refresh-token");
            }
        }

        public async Task<Usertoken?> GetUserTokenByUserId(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Usertokens
                    .OrderByDescending(ut => ut.TokenId)
                    .Where(ut => ut.UserId == curUserID)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw new InvalidOperationException("Failed to get token");
            }
        }

        public async Task RevokeUserTokenByRefreshToken(string refreshToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var token = await context.Usertokens.FirstOrDefaultAsync(t => t.RefreshToken == refreshToken) ?? throw new NotFoundException("Refresh Token not Found");

                token.IsRevoked = true;
                await context.SaveChangesAsync();
            }
            catch
            {
                throw new InvalidOperationException("Failed to Revoked User");
            }
            
        }

        public async Task RevokeUserByUserID(Guid userID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userID) ?? throw new NotFoundException($"{userID} was not found");
                var userToken = await context.Usertokens.OrderByDescending(ut => ut.TokenId).FirstOrDefaultAsync(ut => ut.UserId == userID) ?? throw new NotFoundException("Token not Found");

                user.IsActive = 0;
                userToken.IsRevoked = true;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw new InvalidOperationException("Internal Error happen");
            }
            
        }
    }
}
