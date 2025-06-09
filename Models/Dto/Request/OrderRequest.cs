using DemoFYP.Enums;
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
        public int OrderID {  get; set; }
        public List<PaymentReceipt> ReceiptList { get; set; }
    }

    public class PaymentReceipt
    {
        public int PaymentID { get; set; }
        public IFormFile? Receipt {  get; set; }
    }

    public class PaymentReceiptWithReceiptUrl
    {
        public int PaymentID { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    public class MarkOrderItemCompletedRequest
    {
        public int OrderItemID { get; set; }
    }

    public class MarkOrderCompletedRequest
    {
        public int OrderID { get; set; }
    }

    public class RequestToCancelOrderRequest
    {
        public int OrderID { get; set; }
        public string CancelReason { get; set; } = null!;
        public OrderStatus Status { get; set; } = OrderStatus.RequestCancel;
    }

    public class RequestToCancelOrderItemRequest
    {
        public int OrderItemID { get; set; }
        public string CancelReason { get; set; } = null!;
        public OrderStatus Status { get; set; } = OrderStatus.RequestCancel;
    }

    public class CancelOrderItemRequest
    {
        public int OrderItemID { get; set; }
        public string CancelReason { get; set; } = null!;
        public OrderStatus Status { get; set; } = OrderStatus.Cancelled;
    }

    public class ConfirmCancelOrderItemRequest
    {
        public int OrderItemID { get; set; }
    }

    public class RejectCancelOrderItemRequest
    {
        public int OrderItemID { get; set; }
        public string? Reason { get; set; }
    }

    public class RateProductRequest
    {
        public int OrderItemID { get; set; }
        public double Rating { get; set; }
        public string? Feedback {  get; set; }
    }

    public class FeedbackListRequest : PaginationRequest
    {
        public int ProductID { get; set; }
    }

    #region Admin Dto

    public class OrderListFilterRequest : PaginationRequest
    {
        public int? OrderID { get; set; }
        public Guid? UserID { get; set; }
        public int? PaymentID { get; set; }
        public OrderStatus? Status {  get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateOrderRequest
    {
        public int OrderID { get; set; }
        public double? TotalAmount {  get; set; }
        public OrderStatus Status { get; set; }
    }

    public class CancelOrderRequest
    {
        public int OrderID { get; set; }
        public OrderStatus Status { get; set; }
    }
    #endregion
}
