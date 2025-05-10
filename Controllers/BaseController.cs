using DemoFYP.Models;
using Microsoft.AspNetCore.Mvc;

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

        public byte[] CurUserID
        {
            get
            {
                Guid.TryParse(HttpContext.User.Claims?.Where(x => x.Type == "CurUserID")?.FirstOrDefault()?.Value, out Guid userId);
                return userId.ToByteArray();
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
