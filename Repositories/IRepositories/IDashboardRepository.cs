using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IDashboardRepository
    {
        Task<DashboardResponse> GetDashboardRecord();
    }
}
