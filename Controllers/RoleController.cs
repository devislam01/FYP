using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IRoleServices _roleServices;
        public RoleController(IRoleServices roleServices)
        {
            _roleServices = roleServices ?? throw new ArgumentNullException(nameof(roleServices));
        }

        [Authorize(Policy = "Create_Role")]
        [HttpPost("addRole")]
        public async Task<ActionResult<StandardResponse>> AddRole(AddRoleRequest payload)
        {
            await _roleServices.AddRole(payload, CurUserID);

            return SuccessResponse($"Role '{ payload.RoleName }' added successfully!");
        }
    }
}
