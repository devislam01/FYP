﻿using AutoMapper;
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
        private readonly IConfiguration _config;

        public UserRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper, ICommonServices commonServices, IConfiguration config)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Read DB

        public async Task<bool> CheckIfEmailExist(string email, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                return await context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive == 1);
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
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<Guid> CheckIfUserLogin(string refreshToken)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Usertokens.FirstOrDefaultAsync(ut => ut.RefreshToken == refreshToken) ?? throw new NotFoundException("This user haven't login yet");

                return result.UserId;
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
                                Address = u.Address ?? string.Empty,
                                ResidentialCollege = u.ResidentialCollege ?? string.Empty,
                                PaymentQRCode = string.IsNullOrWhiteSpace(u.PaymentQRCode) ? string.Empty : u.PaymentQRCode,
                            })
                            .FirstOrDefaultAsync();

                if (result == null) throw new NotFoundException("No detail was found");

                return result;
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
            finally
            {
                if (outerContext == null)
                {
                    await context.DisposeAsync();
                }
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
            finally
            {
                await context.DisposeAsync();
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
            finally
            {
                await context.DisposeAsync();
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
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<PagedResult<UserListResponse>> GetUserList(UserListFilterRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.Users
                    .OrderByDescending(u => u.CreatedDateTime)
                    .Where(u => u.RoleID != (int)UserLevel.Admin);

                if (filter.UserID != null && filter.UserID != Guid.Empty)
                {
                    query = query.Where(u => u.UserId == filter.UserID);
                }

                if (!string.IsNullOrEmpty(filter.Email))
                {
                    query = query.Where(u => u.Email.ToLower().Contains(filter.Email.ToLower()));
                }

                if (filter.Ratings != null)
                {
                    query = query.Where(u => u.RatingMark >= filter.Ratings);
                }

                if (!string.IsNullOrEmpty(filter.UserName))
                {
                    query = query.Where(u => u.UserName.ToLower().Contains(filter.UserName.ToLower()));
                }

                if (!string.IsNullOrEmpty(filter.PhoneNumber))
                {
                    query = query.Where(u => u.PhoneNumber.StartsWith(filter.PhoneNumber));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(u => u.IsActive == (filter.Status == "Active" ? 1 : 0));
                }

                if (filter.CreatedAt != null)
                {
                    query = query.Where(u => u.CreatedDateTime == filter.CreatedAt);
                }

                int totalRecord = await query.CountAsync();

                var result = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(q => new UserListResponse
                    {
                        UserID = q.UserId,
                        Email = q.Email,
                        Ratings = q.RatingMark,
                        UserName = q.UserName ?? string.Empty,
                        Gender = q.UserGender,
                        PhoneNumber = q.PhoneNumber ?? string.Empty,
                        Status = q.IsActive == 1 ? "Active" : "Inactive",
                        QRCode = string.IsNullOrWhiteSpace(q.PaymentQRCode) ? string.Empty : q.PaymentQRCode,
                        CreatedAt = q.CreatedDateTime,
                        CreatedBy = q.CreatedBy,
                    }).ToListAsync();

                return new PagedResult<UserListResponse>
                {
                    Data = result,
                    Pagination = new PaginationResponse
                    {
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalRecord = totalRecord
                    }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get User List", ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<EditUserDetailsResponse> GetUserDetails(Guid userID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Users
                    .Select(u => new EditUserDetailsResponse 
                    { 
                        UserID = u.UserId, 
                        UserName = u.UserName ?? string.Empty, 
                        Email = u.Email, 
                        PhoneNumber = u.PhoneNumber ?? string.Empty, 
                        UserGender = u.UserGender ?? string.Empty, 
                        Address = u.Address ?? string.Empty, 
                        ResidentialCollege = u.ResidentialCollege ?? string.Empty
                    }).FirstOrDefaultAsync(u => u.UserID == userID) ?? new EditUserDetailsResponse();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Failed to Get User Detail", ex);
            }
            finally
            {
                await context.DisposeAsync();
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
                newData.PaymentQRCode = registerData.PaymentQRCode;
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
            catch (Exception ex)
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }
                throw new InvalidOperationException("Failed to Register User", ex);
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
                curData.ResidentialCollege = payload.ResidentialCollege;
                curData.PaymentQRCode = payload.PaymentQRCode ?? curData.PaymentQRCode;
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

        public async Task UpdateTempPassword(string email, Guid curUserID, string password)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction trans = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower()) ?? throw new NotFoundException("Email Not Found!");

                curData.Password = password;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

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

        public async Task<string> UpdatePassword(Guid curUserID, string password)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction trans = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.Users.FirstOrDefaultAsync(u => u.UserId == curUserID) ?? throw new NotFoundException("User Not Found!");

                curData.Password = password;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

                await context.SaveChangesAsync();
                await trans.CommitAsync();

                return curData.Email;
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

        public async Task<string> UpdatePassword(Guid userID, Guid curUserID, string password)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction trans = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.Users.FirstOrDefaultAsync(u => u.UserId == userID) ?? throw new NotFoundException("User Not Found!");

                curData.Password = password;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

                await context.SaveChangesAsync();
                await trans.CommitAsync();

                return curData.Email;
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
