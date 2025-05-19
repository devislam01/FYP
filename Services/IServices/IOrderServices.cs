using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IOrderServices
    {
        Task<ProceedToPaymentResponse> CheckOut(PlaceOrderRequest payload, Guid curUserID);
        Task ConfirmOrder(ProceedPaymentRequest payload, Guid curUserID, string curUserEmail);
        Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter);
    }
}
