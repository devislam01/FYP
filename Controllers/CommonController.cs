using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Repositories;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace DemoFYP.Controllers
{
    public class CommonController : BaseController
    {
        private readonly ICommonServices _commonServices;
        private readonly IRoleRepository _roleRepository;
        public CommonController(ICommonServices commonServices, IRoleRepository roleRepository) {
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<ActionResult<StandardResponse<string>>> UploadImages(IFormFile file)
        {
            return SuccessResponse<string>(await _commonServices.UploadImage(file, ""));
        }


        // Used for development purpose only
        [Authorize]
        [HttpPost("addPermission")]
        public async Task<ActionResult<StandardResponse>> CreatePermissions(PermissionRequest payload)
        {
            await _roleRepository.CreatePermissions(payload, CurUserID);

            return SuccessResponse($"Create Permission '{payload.PermissionName}' successfully!");
        }

        [Authorize]
        [HttpPost("addRolePermission")]
        public async Task<ActionResult<StandardResponse>> BindRoleAndPermissions(BindRolePermissionRequest payload)
        {
            await _roleRepository.BindRoleAndPermissions(payload, CurUserID);

            return SuccessResponse($"Bind Permission '{payload.RoleID}' with Permissions: '[{ payload.PermissionIDs }]' successfully!");
        }

        [HttpGet("debug/claims")]
        public IActionResult GetClaims()
        {
            var claims = DecodeJwtClaims(User);
            return Ok(claims);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public Dictionary<string, List<string>> DecodeJwtClaims(ClaimsPrincipal user)
        {
            var claimsDict = user.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => c.Value).ToList()
                );

            return claimsDict;
        }
    }
}
