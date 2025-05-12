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

        #endregion

        #region Update APIs

        [Authorize]
        [HttpPost("updateUserProfile")]
        public async Task<ActionResult<StandardResponse>> UpdateUserProfile(UserUpdateDetailRequest payload)
        {
            await _userServices.UpdateUserProfile(payload, CurUserID);

            return SuccessResponse("Updated Successfully!");
        }

        #endregion
    }
}
