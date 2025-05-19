using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Models.Dto.Request
{
    public class PlaceOrderRequest
    {
        public List<OrderSummary> orderSummaries { get; set; } = null!;
        public string ShippingWay { get; set; } = null!;
        public int PaymentMethod {  get; set; } 
    }

    public class OrderSummary
    {
        public int ProductID { get; set; }
        public double UnitPrice {  get; set; }
        public int Qty { get; set; }
    }

    public class ProceedPaymentRequest
    {
        public int PaymentID { get; set; }
        public int OrderID {  get; set; }
        public IFormFile Receipt { get; set; } = null!;
    }

    #region Admin Dto

    public class OrderListFilterRequest : PaginationRequest
    {
        public int? OrderID { get; set; }
        public Guid? UserID { get; set; }
        public int? PaymentID { get; set; }
        public string? Status {  get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    #endregion
}
