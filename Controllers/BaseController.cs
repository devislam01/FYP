using DemoFYP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoFYP.Controllers
{
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
        }

        public string IPAddress
        {
            get
            {
                return HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            }
        }

        public Guid CurUserID
        {
            get
            {
                var userIdStr = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdStr, out Guid userId))
                {
                    return userId;
                }

                return Guid.Empty;
            }
        }

        protected ActionResult<StandardResponse<T>> SuccessResponse<T>(T data, string message = "Request successful")
        {
            var response = new StandardResponse<T>(data)
            {
                Status = "Success",
                Message = message
            };
            return Ok(response);
        }

        protected ActionResult<StandardResponse> SuccessResponse(string message = "Request successful")
        {
            var response = new StandardResponse
            {
                Status = "Success",
                Message = message
            };
            return Ok(response);
        }
    }
}
