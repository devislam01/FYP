using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IJwtServices _Jwtservices;
        private readonly IUserServices _userServices;

        public AuthController(IJwtServices jwtServices, IUserServices userServices, IJwtRepository jwtRepository) {
            _Jwtservices = jwtServices ?? throw new ArgumentNullException(nameof(jwtServices));
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<StandardResponse<JwtAuthResult>>> Login(UserLoginRequest request)
        {
            UserJwtClaims jwtClaims = await _userServices.CheckLoginCredentials(request);
            var data = await _Jwtservices.GenerateToken(jwtClaims);

            return SuccessResponse(data, "Login Successfully");
        }

        [HttpPost("logout")]
        public async Task<ActionResult<StandardResponse>> Logout(UserLogoutRequest request)
        {
            Guid userID = await _userServices.CheckIsLogin(request.RefreshToken);
            bool isLogout = await _Jwtservices.LogoutUserByRevokeToken(userID);

            if (!isLogout) return SuccessResponse("Log out Failed");

            return SuccessResponse("Logged out Successfully");
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<StandardResponse<JwtAuthResult>>> RefreshToken([FromBody] RefreshTokenRequest payload)
        {
            var data = await _Jwtservices.VerifyAndGenerateRefreshToken(payload);
                
            return SuccessResponse(data);
        }
    }
}
