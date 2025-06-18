namespace DemoFYP.Models.Dto.Response
{
    public class DashboardResponse
    {
        public SalesSummary Sales {  get; set; }
        public UsersSummary Users { get; set; }
        public OrderChartDataDto OrderChartData { get; set; }
    }

    public class SalesSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal LastTotalSales { get; set; }
        public decimal AOV {  get; set; }
    }

    public class UsersSummary
    {
        public int TotalUser { get; set; }
        public decimal TotalNewUser { get; set; }
        public decimal ActiveUser { get; set; }
    }

    public class OrderChartDataDto
    {
        public List<string> Months { get; set; } = new();
        public List<double> SalesRevenue { get; set; } = new();
        public List<int> NumberOfOrders { get; set; } = new();
    }
}
