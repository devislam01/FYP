using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class RoleService : IRoleServices
    {
        private readonly IRoleRepository _roleRepository;
        public RoleService(IRoleRepository roleRepository) {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task AddRole(AddRoleRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (string.IsNullOrEmpty(payload.RoleName)) throw new BadRequestException("Role Name is required");

            try
            {
                await _roleRepository.AddRole(payload, curUserID);
            }
            catch {
                throw;
            }
        }
    }
}
