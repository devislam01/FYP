using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IJwtServices _Jwtservices;
        private readonly IUserServices _userServices;

        public AuthController(IJwtServices jwtServices, IUserServices userServices) {
            _Jwtservices = jwtServices ?? throw new ArgumentNullException(nameof(jwtServices));
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [HttpPost("login")]
        public async Task<ActionResult<StandardResponse<JwtAuthResult>>> Login(UserLoginRequest request)
        {
            await _userServices.CheckLoginCredentials(request);
            var data = await _Jwtservices.GenerateToken(request, CurUserID);

            return SuccessResponse(data, "Login Successfully");
        }
    }
}
