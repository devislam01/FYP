using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
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

        public async Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                return await context.Users.AnyAsync(u => u.Email == email);
            }
            catch
            {
                throw;
            }
        }

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

        public async Task<Guid> CheckUserLoginCredentials(UserLoginRequest payload)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Users
                    .Where(u => u.Email.ToLower() == payload.Email.ToLower() && u.Password == payload.Password)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
