using DemoFYP.Enums;

namespace DemoFYP.Models.Dto.Response
{
    public class UserOrdersResponse
    {
        public int OrderID { get; set; }
        public string? Receipt {  get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; } = null!;
    }

    public class OrderSummariesResponse
    {
        public List<ShoppingCartObj> shoppingCartObjs { get; set; }
        public double Total {  get; set; }
        public int PaymentMethod {  get; set; }
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public string ProductName { get; set; } = null!;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }

    public class SellerOrdersResponse
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public Guid BuyerID { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? Receipt {  get; set; }
        public int PaymentMethodID {  get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProceedToPaymentResponse
    {
        public int PaymentID {  get; set; }
        public int OrderID {  get; set; }
        public string QRCode { get; set; }
    }

    #region Admin Dto

    public class OrderListResponse
    {
        public int OrderID { get; set; }
        public Guid UserID { get; set; }
        public int PaymentID { get; set; }
        public double TotalAmount {  get; set; }
        public string Status {  get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
