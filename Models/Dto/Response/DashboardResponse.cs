namespace DemoFYP.Models.Dto.Response
{
    public class DashboardResponse
    {
        public SalesSummary Sales {  get; set; }
        public UsersSummary Users { get; set; }
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

}
