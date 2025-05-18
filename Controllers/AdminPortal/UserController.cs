using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserServices _userServices;
        private readonly IJwtServices _jwtServices;

        public UserController(IUserServices userServices, IJwtServices jwtServices)
        {
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            _jwtServices = jwtServices ?? throw new ArgumentNullException(nameof(jwtServices));
        }

        [HttpPost("userList")]
        public async Task<ActionResult<StandardResponse<PagedResult<UserListResponse>>>> GetUserList(PaginationRequest pagination)
        {
            return SuccessResponse<PagedResult<UserListResponse>>(await _userServices.GetUserList(pagination));
        }

        [Authorize(Policy = "Admin_Create_User")]
        [HttpPost("createUser")]
        public async Task<ActionResult<StandardResponse>> CreateUser(UserRegisterRequest payload)
        {
            await _userServices.RegisterUser(payload, CurUserID);

            return SuccessResponse("User Created Successfully!");
        }

        [Authorize(Policy = "Admin_Update_User")]
        [HttpPost("updateUser")]
        public async Task<ActionResult<StandardResponse>> UpdateUser(UserUpdateDetailRequest payload)
        {
            await _userServices.UpdateUserProfile(payload, CurUserID);

            return SuccessResponse("User Updated Successfully!");
        }

        [Authorize(Policy = "AP_Revoke_User")]
        [HttpPost("revoke-user")]
        public async Task<ActionResult<StandardResponse>> RevokeUser(RevokeUserRequest payload)
        {
            await _jwtServices.RevokeUser(payload.UserID, CurUserID);

            return SuccessResponse($"User {payload.UserID} has been revoked");
        }

        [Authorize(Policy = "AP_Reinstate_user")]
        [HttpPost("reinstate-user")]
        public async Task<ActionResult<StandardResponse>> ReinstateUser(ReinstateUserRequest payload)
        {
            await _jwtServices.ReinstateUser(payload.UserID, CurUserID);

            return SuccessResponse($"User {payload.UserID} has been reinstated");
        }
    }
}
