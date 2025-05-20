using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IPaymentServices
    {
        Task<PagedResult<PaymentListResponse>> GetPaymentList(PaymentListFilterRequest filter);
        Task UpdatePayment(UpdatePaymentRequest payload, Guid curUserID);
    }
}
