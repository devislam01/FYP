using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DemoFYP.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public UserRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper)
        {
            _factory = factory;
            _mapper = mapper;
        }

        #region Read DB

        public async Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                return await context.Users.AnyAsync(u => u.Email == email && u.IsActive == 1);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Guid> CheckUserLoginCredentials(UserLoginRequest payload)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Users
                    .Where(u => u.Email.ToLower() == payload.Email.ToLower() && u.Password == payload.Password && u.IsActive == 1)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserDetailResponse> GetUserProfileByLoginID(Guid CurUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Users.Where(u => u.UserId == CurUserID && u.IsActive == 1)
                            .Select(u => new UserDetailResponse
                            {
                                UserName = u.UserName ?? string.Empty,
                                Email = u.Email,
                                PhoneNumber = u.PhoneNumber ?? string.Empty,
                                UserGender = u.UserGender ?? string.Empty,
                                Address = u.Address ?? string.Empty
                            })
                            .FirstOrDefaultAsync();

                if (result == null) throw new NotFoundException("No detail was found");

                return result;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Create Action

        public async Task RegisterUser(UserRegisterRequest registerData, Guid updatedBy, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();
            IDbContextTransaction tran = null;

            try
            {
                if (outerContext == null)
                {
                    tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                }

                var newData = _mapper.Map<User>(registerData);
                newData.UserId = Guid.NewGuid();
                newData.CreatedBy = updatedBy;
                newData.CreatedDateTime = DateTime.Now;
                newData.IsActive = 1;

                await context.Users.AddAsync(newData);
                await context.SaveChangesAsync();

                if (tran != null)
                {
                    await tran.CommitAsync();
                }
            }
            catch
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tran != null)
                {
                    await tran.DisposeAsync();
                }
            }
        }

        #endregion

        #region Update DB

        public async Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction tran = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.Users.FirstOrDefaultAsync(u => u.UserId == (payload.UserID ?? curUserID) && u.IsActive == 1) ?? throw new NotFoundException("User Not Found");

                curData.UserName = payload.UserName;
                curData.Email = payload.Email;
                curData.PhoneNumber = payload.PhoneNumber;
                curData.UserGender = payload.UserGender;
                curData.Address = payload.Address;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

                await context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tran != null)
                {
                    await tran.DisposeAsync();
                }
            }
        }

        #endregion
    }
}
