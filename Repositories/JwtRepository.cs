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

        public async Task AddOrUpdateUserToken(Usertoken userToken)
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Insert Token Failed", ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<UserJwtClaims> GetUserClaimsByRefreshToken(string refreshToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Usertokens
                    .Join(context.Users, ut => ut.UserId, u => u.UserId, (ut, u) => new { u.UserId, u.Email, u.RoleID, ut.RefreshToken, ut.IsRevoked, ut.RefreshTokenExpiresAt })
                    .Join(context.Roles, u => u.RoleID, r => r.RoleID, (u, r) => new {u.UserId, u.Email, u.RefreshToken, u.IsRevoked, u.RefreshTokenExpiresAt, u.RoleID, r.RoleName})
                    .Where(ut =>
                        ut.RefreshToken == refreshToken &&
                        !ut.IsRevoked &&
                        ut.RefreshTokenExpiresAt > DateTime.Now
                    )
                    .Select(u => new { 
                        UserID = u.UserId, 
                        Email = u.Email, 
                        RoleID = u.RoleID,
                        Role = u.RoleName 
                    }).FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("Session Expired.");

                var permissionNames = await context.RolePermissions
                    .Where(rp => rp.RoleID == result.RoleID)
                    .Join(context.Permissions,
                        rp => rp.PermissionID,
                        p => p.PermissionID,
                        (rp, p) => p.PermissionName)
                    .ToListAsync();

                return new UserJwtClaims
                {
                    UserID = result.UserID,
                    Email = result.Email,
                    Role = result.Role,
                    Permissions = permissionNames
                };
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
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
            finally
            {
                await context.DisposeAsync();
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
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<bool> RevokeUserTokenByUserID(Guid userID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var token = await context.Usertokens.FirstOrDefaultAsync(ut => ut.UserId == userID) ?? throw new UnauthorizedAccessException();

                token.IsRevoked = true;
                await context.SaveChangesAsync();

                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task RevokeUserByUserID(Guid userID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userID) ?? throw new NotFoundException($"{userID} was not found");
                var userToken = await context.Usertokens.OrderByDescending(ut => ut.TokenId).FirstOrDefaultAsync(ut => ut.UserId == userID) ?? throw new NotFoundException("This user haven't login yet");

                user.IsActive = 0;
                userToken.IsRevoked = true;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task ReinstateUserByUserID(Guid userID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userID) ?? throw new NotFoundException($"{userID} was not found");

                user.IsActive = 1;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw new InvalidOperationException("Internal Error happen");
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
