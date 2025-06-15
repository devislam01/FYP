using DemoFYP.Models;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers.AdminPortal
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardServices _dashboardServices;

        public DashboardController(IDashboardServices dashboardServices) {
            _dashboardServices = dashboardServices ?? throw new ArgumentNullException(nameof(dashboardServices));
        }


        [HttpGet("dashboard")]
        public async Task<ActionResult<StandardResponse<DashboardResponse>>> GetDashboardRecord()
        {
            return new StandardResponse<DashboardResponse>(await _dashboardServices.GetDashboardRecord());
        }
    }
}
