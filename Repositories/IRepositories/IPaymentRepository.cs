using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IPaymentRepository
    {
        Task<PagedResult<PaymentListResponse>> GetPaymentList(PaymentListFilterRequest filter);
        Task<int> InsertPayment(int orderId, int paymentMethod, double totalAmount, Guid createdBy, AppDbContext outerContext = null);
        Task UpdatePayment(UpdatePaymentRequest payload, Guid curUserID);
    }
}
