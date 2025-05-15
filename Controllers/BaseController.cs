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

        public string CurUserEmail
        {
            get
            {
                var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    return userEmail;
                }

                return string.Empty;
            }
        }

        public string CurUserRole
        {
            get
            {
                var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(userRole))
                {
                    return userRole;
                }

                return string.Empty;
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
