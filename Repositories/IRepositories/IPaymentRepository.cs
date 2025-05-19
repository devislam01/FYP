using DemoFYP.EF;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IPaymentRepository
    {
        Task<int> InsertPayment(int orderId, int paymentMethod, double totalAmount, Guid createdBy, AppDbContext outerContext = null);
    }
}
