using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices) {
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        #region Create APIs

        [HttpPost("register")]
        public async Task<ActionResult<StandardResponse>> Register(UserRegisterRequest payload)
        {
            await _userServices.RegisterUser(payload, CurUserID);

            return SuccessResponse("User Registered Successfully!");
        }

        #endregion

        #region Read APIs

        [Authorize]
        [HttpGet("getUserProfile")]
        public async Task<ActionResult<StandardResponse<UserDetailResponse>>> GetUserProfile()
        {
            var result = await _userServices.GetUserProfile(CurUserID);

            return SuccessResponse<UserDetailResponse>(result);
        }

        [Authorize]
        [HttpGet("permissionList")]
        public async Task<ActionResult<StandardResponse<UserPermissionResponse>>> GetPermissionList()
        {
            return SuccessResponse<UserPermissionResponse>(await _userServices.GetPermissionsList());
        }

        [Authorize]
        [HttpGet("adminPermissionsList")]
        public async Task<ActionResult<StandardResponse<UserPermissionResponse>>> GetAdminPermissionList()
        {
            return SuccessResponse<UserPermissionResponse>(await _userServices.GetAdminPermissionsList());
        }

        [Authorize]
        [HttpGet("permission")]
        public async Task<ActionResult<StandardResponse<UserPermissionResponse>>> GetPermission()
        {
            return SuccessResponse<UserPermissionResponse>(await _userServices.GetUserPermissions(CurUserID));
        }

        #endregion

        #region Update APIs

        [Authorize(Policy = "Update_User")]
        [HttpPost("updateUserProfile")]
        public async Task<ActionResult<StandardResponse>> UpdateUserProfile(UserUpdateDetailRequest payload)
        {
            await _userServices.UpdateUserProfile(payload, CurUserID);

            return SuccessResponse("Updated Successfully!");
        }

        [HttpPost("forgetPassword")]
        public async Task<ActionResult<StandardResponse>> SendTemporilyPassword(ForgotPasswordRequest payload)
        {
            await _userServices.SendTemporilyPassword(payload.Email, CurUserID);

            return SuccessResponse("Email was sent! Please check your mailbox and reset password.");
        }

        [Authorize(Policy = "Reset_Password")]
        [HttpPost("resetPassword")]
        public async Task<ActionResult<StandardResponse>> ResetPassword(ResetPasswordRequest payload)
        {
            await _userServices.ResetPassword(payload, CurUserID);

            return SuccessResponse("Your password was reset!");
        }

        #endregion
    }
}
