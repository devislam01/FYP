using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IDashboardServices
    {
        Task<DashboardResponse> GetDashboardRecord();
    }
}
