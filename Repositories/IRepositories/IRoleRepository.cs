using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IRoleRepository
    {
        Task AddRole(AddRoleRequest payload, Guid curUserID);

        // Used for development purpose only
        Task CreatePermissions(PermissionRequest payload, Guid curUserID);
        Task BindRoleAndPermissions(BindRolePermissionRequest payload, Guid curUserID);
    }
}
