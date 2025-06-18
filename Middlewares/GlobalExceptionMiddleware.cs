using DemoFYP.Exceptions;
using DemoFYP.Models;
using System.Text.Json;

namespace DemoFYP.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }
                context.Response.ContentType = "application/json";

                var response = new StandardResponse
                {
                    Message = ex.Message
                };

                switch (ex)
                {
                    case BadRequestException _:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        response.Code = 400;
                        response.Status = "Bad Request";
                        break;
                    case UnauthorizedAccessException _:
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        response.Code = 401;
                        response.Status = "Unauthorized";
                        break;
                    case ForbiddenException _:
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        response.Code = 403;
                        response.Status = "No Access";
                        break;
                    case ConflictException _:
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        response.Code = 409;
                        response.Status = "Conflict";
                        break;
                    case NotFoundException _:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        response.Code = 404;
                        response.Status = "Not Found";
                        break;
                    case BusinessException _:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        response.Code = 400;
                        response.Status = "Bad Business Request";
                        break;
                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        response.Code = 500;
                        response.Status = "Internal Error";
                        break;
                }

                try
                {
                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);
                }
                catch (Exception jsonEx)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("{\"code\":\"500\",\"status\":\"Critical Error\",\"message\":\"Response serialization failed\"}");
                }
            }
        }
    }

}
