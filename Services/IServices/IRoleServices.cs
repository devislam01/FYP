using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Services.IServices
{
    public interface IRoleServices
    {
        Task AddRole(AddRoleRequest payload, Guid curUserID);
    }
}
