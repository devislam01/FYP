using DemoFYP.Enums;

namespace DemoFYP.Models.Dto.Response
{
    public class UserOrdersResponse
    {
        public int OrderID { get; set; }
        public double TotalAmt { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PaymentMethodID { get; set; }
        public List<OrderSellerGroupVO> OrderItems { get; set; } = null!;
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
        public string? Reason { get; set; }
    }

    public class OrderItemVO : OrderItem
    {
        public string ProductImage { get; set; } = null!;
        public int ProductID { get; set; }
        public bool HasRating { get; set; }
    }

    public class OrderSellerGroupVO
    {
        public string SellerName { get; set; } = null!;
        public string SellerPhoneNo { get; set; } = null!;
        public string? Receipt { get; set; }
        public int PaymentID { get; set; }
        public string PaymentQRCode { get; set; } = null!;
        public double TotalAmtForSeller {  get; set; }
        public List<OrderItemVO> Items { get; set; } = new();
    }


    public class SellerOrdersResponse
    {
        public int OrderID { get; set; }
        public double TotalAmt { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? Receipt { get; set; }
        public int PaymentMethodID { get; set; }
        public Guid BuyerID { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhoneNo { get; set; }
        public string? BuyerAddress {  get; set; }
        public string? ResidentialCollege {  get; set; }
        public List<SellerOrderItemVO> OrderItems { get; set; } = null!;
        
    }

    public class SellerOrderItemVO
    {
        public int OrderItemID { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductImage { get; set; } = null!;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = null!;
        public string? Reason { get; set; }
    }

    public class CheckoutResponse
    {
        public List<ProceedToPayment> ProceedToPayments { get; set; } = [];
    }

    public class ProceedToPayment
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public string QRCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public double Price { get; set; }
    }

    public class FeedbackListResponse
    {
        public string? BuyerName { get; set; }
        public DateTime FeedbackAt { get; set; }
        public double Rating { get; set; }
        public string? Feedbacks { get; set; }
    }

    #region Admin Dto

    public class OrderListResponse
    {
        public int OrderID { get; set; }
        public Guid UserID { get; set; }
        public string PaymentID { get; set; }
        public double TotalAmount {  get; set; }
        public string Status {  get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
