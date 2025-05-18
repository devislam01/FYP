using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text.Json;

namespace DemoFYP.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;
        private readonly ICommonServices _commonServices;

        public UserRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper, ICommonServices commonServices)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
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

        public async Task<UserJwtClaims> CheckUserLoginCredentials(UserLoginRequest payload)
        {
            var context = _factory.CreateDbContext();

            try
            {

                var userWithRole = await context.Users
                    .Where(u => u.Email.ToLower() == payload.Email.ToLower() && u.IsActive == 1)
                    .Join(context.Roles,
                        u => u.RoleID,
                        r => r.RoleID,
                        (u, r) => new
                        {
                            u.UserId,
                            u.Email,
                            u.Password,
                            r.RoleID,
                            r.RoleName
                        })
                    .FirstOrDefaultAsync() ?? throw new NotFoundException("User Not Found");

                bool isValid = BCrypt.Net.BCrypt.Verify(payload.Password, userWithRole.Password);
                if (!isValid)
                    throw new NotFoundException("Invalid Password");

                var permissionNames = await context.RolePermissions
                    .Where(rp => rp.RoleID == userWithRole.RoleID)
                    .Join(context.Permissions,
                        rp => rp.PermissionID,
                        p => p.PermissionID,
                        (rp, p) => p.PermissionName)
                    .ToListAsync();

                return new UserJwtClaims
                {
                    UserID = userWithRole.UserId,
                    Email = userWithRole.Email,
                    Role = userWithRole.RoleName,
                    Permissions = permissionNames
                };
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

        public async Task<User> GetUserByLoginID(Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                return await context.Users.FirstOrDefaultAsync(u => u.UserId == curUserID) ?? throw new NotFoundException("User not Found!");
            }
            catch (Exception ex) 
            {
                throw new InvalidOperationException("Failed to Get User Data", ex);
            }
        }

        public async Task<UserPermissionResponse> GetPermissions()
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Permissions.Where(p => !p.PermissionName.StartsWith("AP_")).Select(p => p.PermissionName).ToListAsync();
                
                return new UserPermissionResponse { Permissions = result };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get Permission List", ex);
            }
        }

        public async Task<UserPermissionResponse> GetAdminPermissions()
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Permissions.Where(p => p.PermissionName.StartsWith("AP_")).Select(p => p.PermissionName).ToListAsync();

                return new UserPermissionResponse { Permissions = result };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get Permission List", ex);
            }
        }

        public async Task<UserPermissionResponse> GetUserPermissionsByLoginID(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var permissionNames = await context.Users
                    .Where(u => u.UserId == curUserID)
                    .SelectMany(u => u.Role.RolePermissions.Select(rp => rp.Permission.PermissionName))
                    .ToListAsync();

                return new UserPermissionResponse { Permissions = permissionNames };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get User Permissions", ex);
            }
        }

        public async Task<PagedResult<UserListResponse>> GetUserList(PaginationRequest pagination)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.Users
                    .OrderByDescending(u => u.CreatedDateTime)
                    .Where(u => u.RoleID != (int)UserLevel.Admin && u.IsActive == (sbyte)Status.Active);

                int totalRecord = await query.CountAsync();

                if (!pagination.DisablePagination)
                {
                    query = query
                        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);
                }

                var result = await query.Select(q => new UserListResponse
                {
                    UserID = q.UserId,
                    Email = q.Email,
                    Ratings = q.RatingMark,
                    UserName = q.UserName ?? string.Empty,
                    PhoneNumber = q.PhoneNumber ?? string.Empty,
                    Status = q.IsActive == 1 ? "Active" : "Inactive",
                    QRCode = q.PaymentQRCode,
                    CreatedAt = q.CreatedDateTime,
                    CreatedBy = q.CreatedBy,
                    UpdatedAt = q.UpdatedDateTime,
                    UpdatedBy = q.UpdatedBy,
                }).ToListAsync();

                return new PagedResult<UserListResponse>
                {
                    Data = result,
                    Pagination = new PaginationResponse
                    {
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalRecord = totalRecord
                    }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get User List", ex);
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
                newData.RoleID = (int)UserLevel.User;
                newData.Password = _commonServices.HashPassword(registerData.Password);
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
                curData.PhoneNumber = payload.PhoneNumber;
                curData.UserGender = payload.UserGender;
                curData.Address = payload.Address;
                curData.PaymentQRCode = payload.QRCodePath ?? curData.PaymentQRCode;
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

        public async Task UpdatePassword(string email, Guid CurUserID, string password)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction trans = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new NotFoundException("Email Not Found!");

                curData.Password = password;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = CurUserID;

                await context.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch
            {
                if (trans != null)
                {
                    await trans.RollbackAsync();
                }

                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        #endregion
    }
}
