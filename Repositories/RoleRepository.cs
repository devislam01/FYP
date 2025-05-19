using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public RoleRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddRole(AddRoleRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var isExists = await context.Roles
                    .AnyAsync(r => r.RoleName.ToLower() == payload.RoleName.ToLower());

                if (isExists)
                    throw new ConflictException($"Role '{payload.RoleName}' already exists.");

                var newData = _mapper.Map<Role>(payload);

                newData.RoleName = payload.RoleName;
                newData.CreatedAt = DateTime.Now;
                newData.CreatedBy = curUserID;
                newData.IsActive = true;

                await context.Roles.AddAsync(newData);
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

        // Used for development purpose only
        public async Task CreatePermissions(PermissionRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var newData = new Permission
                {
                    PermissionName = payload.PermissionName,
                    PermissionDescription = payload.PermissionDescription,
                    CreatedAt = DateTime.Now,
                    CreatedBy = curUserID,
                    IsActive = true
                };

                await context.Permissions.AddAsync(newData);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Create Permission", ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task BindRoleAndPermissions(BindRolePermissionRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            var rolePermissions = new List<RolePermission>();

            try
            {
                foreach (var permissionId in payload.PermissionIDs)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleID = payload.RoleID,
                        PermissionID = permissionId
                    });
                }

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Bind Permissions", ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
