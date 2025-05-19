using DemoFYP.EF;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public PaymentRepository(IDbContextFactory<AppDbContext> factory) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<int> InsertPayment(int orderId, int paymentMethod, double totalAmount, Guid createdBy, AppDbContext outerContext = null)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    PaymentMethodID = paymentMethod,
                    TotalPaidAmount = totalAmount,
                    Status = "Pending",
                    CreatedDateTime = DateTime.Now,
                    CreatedBy = createdBy
                };

                context.Payments.Add(payment);
                await context.SaveChangesAsync();

                return payment.PaymentId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Insert Payment Record", ex);
            }
            finally
            {
                if (outerContext == null) {
                    await context.DisposeAsync();
                }
                
            }
        }
    }
}
