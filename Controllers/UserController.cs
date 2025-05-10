using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices) {
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [HttpPost("register")]
        public async Task<ActionResult<StandardResponse>> Register(UserRegisterRequest payload)
        {
            await _userServices.RegisterUser(payload, CurUserID);

            return SuccessResponse("User Registered Successfully!");
        }
    }
}
