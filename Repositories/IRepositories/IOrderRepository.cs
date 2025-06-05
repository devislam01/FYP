using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<List<UserOrdersResponse>> GetOrdersByBuyer(Guid curUserID);
        Task<List<SellerOrdersResponse>> GetOrdersBySeller(Guid curUserID);
        Task<CheckoutResponse> CheckOutOrder(PlaceOrderRequest payload, Guid curUserID);
        Task ConfirmPayment(Dictionary<int, string> receipts, Guid curUserID, AppDbContext outerContext = null);
        Task ConfirmOrder(ProceedPaymentRequest payload, Dictionary<int, string> receiptUrl, Guid curUserID, string curUserEmail);
        Task RequestCancelOrderByUser(RequestToCancelOrderRequest payload, Guid curUserID);
        Task RequestCancelOrderItemByUser(RequestToCancelOrderItemRequest payload, Guid curUserID);
        Task ConfirmCancelOrderItemBySeller(ConfirmCancelOrderItemRequest payload, Guid curUserID);
        Task RejectCancelOrderItemBySeller(RejectCancelOrderItemRequest payload, Guid curUserID);
        Task MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload, Guid curUserID);
        Task MarkOrderAsCompleted(MarkOrderCompletedRequest payload, Guid curUserID);
        Task UpdateProductQtyByProductID(int productID, int OrderQty, AppDbContext outerContext);
        Task<Guid> RateProduct(RateProductRequest payload, Guid curUserID);
        Task<PagedResult<FeedbackListResponse>> GetFeedbackList(FeedbackListRequest filter);
        Task CalculateAndUpdateSellerRatingMark(Guid sellerID);

        #region Admin
        Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter);
        Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID);
        Task CancelOrderByAdmin(CancelOrderRequest payload, Guid curUserID);
        Task CancelOrderItemByAdmin(CancelOrderItemRequest payload, Guid curUserID);
        #endregion

        Task SendCancellationRequestEmailToSeller(int orderItemID);
    }
}
