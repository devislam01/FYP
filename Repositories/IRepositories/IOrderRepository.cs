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
        Task<ProceedToPaymentResponse> CheckOutOrder(PlaceOrderRequest payload, Guid curUserID);
        Task ConfirmPayment(int paymentID, Guid curUserID, string receiptUrl, AppDbContext outerContext = null);
        Task ConfirmOrder(ProceedPaymentRequest payload, string receiptUrl, Guid curUserID, string curUserEmail);
        Task RequestCancelOrderByUser(RequestToCancelOrderRequest payload, Guid curUserID);
        Task RequestCancelOrderItemByUser(RequestToCancelOrderItemRequest payload, Guid curUserID);
        Task ConfirmCancelOrderItemBySeller(ConfirmCancelOrderItemRequest payload, Guid curUserID);
        Task RejectCancelOrderItemBySeller(RejectCancelOrderItemRequest payload, Guid curUserID);
        Task MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload, Guid curUserID);
        Task UpdateProductQtyByProductID(int productID, int OrderQty, AppDbContext outerContext);

        #region Admin
        Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter);
        Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID);
        Task CancelOrderByAdmin(CancelOrderRequest payload, Guid curUserID);
        Task CancelOrderItemByAdmin(CancelOrderItemRequest payload, Guid curUserID);
        #endregion

        Task SendCancellationRequestEmailToSeller(int orderItemID);
    }
}
