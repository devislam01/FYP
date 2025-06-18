using DemoFYP.EF;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DemoFYP.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public DashboardRepository(IDbContextFactory<AppDbContext> factory) { 
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<DashboardResponse> GetDashboardRecord()
        {
            var context = _factory.CreateDbContext();

            try
            {
                var sales = await GetSalesSummary(context);
                var users = await GetUsersSummary(context);
                var chart = await GetOrderChartData(context);

                var result = new DashboardResponse()
                {
                    Sales = sales,
                    Users = users,
                    OrderChartData = chart
                };

                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<SalesSummary> GetSalesSummary(AppDbContext outerContext)
        {
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);

            var totalOrders = await outerContext.Orders
                .Where(o => o.Status == "Completed")
                .CountAsync();

            var totalSales = await outerContext.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => (decimal)o.TotalAmount);

            var lastMonthSales = await outerContext.Orders
                .Where(o => o.Status == "Completed" &&
                            o.CreatedDateTime.Year == lastMonth.Year &&
                            o.CreatedDateTime.Month == lastMonth.Month)
                .SumAsync(o => (decimal)o.TotalAmount);

            var aov = totalOrders > 0 ? totalSales / totalOrders : 0;

            return new SalesSummary
            {
                TotalOrders = totalOrders,
                TotalSales = totalSales,
                LastTotalSales = lastMonthSales,
                AOV = aov
            };
        }

        public async Task<UsersSummary> GetUsersSummary(AppDbContext outerContext)
        {
            var now = DateTime.Now;
            var firstDayOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);

            var totalUser = await outerContext.Users.CountAsync(u => u.RoleID != 1);

            var totalNewUser = await outerContext.Users
                .Where(u => u.CreatedDateTime >= firstDayOfLastMonth &&
                            u.CreatedDateTime < firstDayOfThisMonth && u.RoleID != 1)
                .CountAsync();

            var activeUser = await outerContext.Users.Where(u => u.RoleID != 1).CountAsync(u => u.IsActive == 1);

            return new UsersSummary
            {
                TotalUser = totalUser,
                TotalNewUser = totalNewUser,
                ActiveUser = activeUser
            };
        }

        public async Task<OrderChartDataDto> GetOrderChartData(AppDbContext outerContext)
        {
            var chartRawData = await outerContext.Orders
                .Where(o => o.Status == "Completed")
                .GroupBy(o => o.CreatedDateTime.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    SalesRevenue = g.Sum(o => o.TotalAmount),
                    NumberOfOrders = g.Count()
                })
                .ToListAsync();

            var fullChartData = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m),
                    SalesRevenue = chartRawData.FirstOrDefault(d => d.Month == m)?.SalesRevenue ?? 0,
                    NumberOfOrders = chartRawData.FirstOrDefault(d => d.Month == m)?.NumberOfOrders ?? 0
                })
                .ToList();

            return new OrderChartDataDto
            {
                Months = fullChartData.Select(x => x.MonthName).ToList(),
                SalesRevenue = fullChartData.Select(x => x.SalesRevenue).ToList(),
                NumberOfOrders = fullChartData.Select(x => x.NumberOfOrders).ToList()
            };
        }
    }
}
