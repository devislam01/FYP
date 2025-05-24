using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IOrderServices
    {
        Task<List<UserOrdersResponse>> GetOrdersByBuyer(Guid curUserID);
        Task<List<SellerOrdersResponse>> GetOrdersBySeller(Guid curUserID);
        Task<OrderSummariesResponse> GetOrderSummaries(Guid curUserID);
        Task<ProceedToPaymentResponse> CheckOut(PlaceOrderRequest payload, Guid curUserID);
        Task ConfirmOrder(ProceedPaymentRequest payload, Guid curUserID, string curUserEmail);
        Task RequestToCancelOrder(RequestToCancelOrderRequest payload, Guid curUserID);
        Task RequestToCancelOrderItem(RequestToCancelOrderItemRequest payload, Guid curUserID);
        Task ConfirmCancelOrderItemBySeller(ConfirmCancelOrderItemRequest payload, Guid curUserID);
        Task RejectCancelBySeller(RejectCancelOrderItemRequest payload, Guid curUserID);
        Task MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload, Guid curUserID);

        #region Admin
        Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter);
        Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID);
        Task CancelOrder(CancelOrderRequest payload, Guid curUserID);
        Task CancelOrderItem(CancelOrderItemRequest payload, Guid curUserID);
        #endregion
    }
}
