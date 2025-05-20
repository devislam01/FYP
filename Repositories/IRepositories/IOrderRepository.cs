using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<ProceedToPaymentResponse> CheckOutOrder(PlaceOrderRequest payload, Guid curUserID);
        Task ConfirmPayment(int paymentID, string receiptUrl, Guid curUserID, AppDbContext outerContext = null);
        Task ConfirmOrder(ProceedPaymentRequest payload, string receiptUrl, Guid curUserID, string curUserEmail);
        Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter);
        Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID);
    }
}
