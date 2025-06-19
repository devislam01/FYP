using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DemoFYP.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IConfiguration _config;

        public PaymentRepository(IDbContextFactory<AppDbContext> factory, IConfiguration config) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<PagedResult<PaymentListResponse>> GetPaymentList(PaymentListFilterRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                IQueryable<Payment> query = context.Payments.OrderByDescending(p => p.PaymentId);

                if (filter.OrderId != null && filter.OrderId > 0)
                {
                    query = query.Where(q => q.OrderId == filter.OrderId);
                }

                if (filter.TotalPaidAmount != null)
                {
                    query = query.Where(q => q.TotalPaidAmount == filter.TotalPaidAmount);
                }

                if (filter.PaymentMethodID != null && filter.PaymentMethodID > 0) 
                {
                    query = query.Where(q => q.PaymentMethodID == filter.PaymentMethodID);
                }

                if (!string.IsNullOrEmpty(filter.Status.ToString()))
                {
                    query = query.Where(q => q.Status == filter.Status.ToString());
                }

                if (filter.CreatedDateTime != null)
                {
                    query = query.Where(q => q.CreatedDateTime == filter.CreatedDateTime);
                }

                int totalRecord = await query.CountAsync();

                var result = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(q => new PaymentListResponse
                    {
                        OrderID = q.OrderId,
                        PaymentID = q.PaymentId,
                        PaymentMethodID = q.PaymentMethodID,
                        Receipt = !string.IsNullOrWhiteSpace(q.Receipt) ? q.Receipt : null,
                        TotalPaidAmount = q.TotalPaidAmount,
                        Status = q.Status,
                        CreatedAt = q.CreatedDateTime,
                    }).ToListAsync();

                return new PagedResult<PaymentListResponse>
                {
                    Data = result,
                    Pagination = new PaginationResponse
                    {
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalRecord = totalRecord
                    }
                };
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<PaymentListResponse> GetPaymentDetail(int paymentID)
        {
            var context = await _factory.CreateDbContextAsync();

            try
            {
                var result = await context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.PaymentId == paymentID);

                return new PaymentListResponse
                {
                    PaymentID = result.PaymentId,
                    OrderID = result.Order.OrderId,
                    TotalPaidAmount = result.TotalPaidAmount,
                    PaymentMethodID = result.PaymentMethodID,
                    Receipt = !string.IsNullOrWhiteSpace(result.Receipt) ? result.Receipt : null,
                    Status = result.Status,
                };
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<int> InsertPayment(int orderId, int paymentMethod, Guid sellerID, double totalAmount, Guid createdBy, AppDbContext outerContext = null)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    PaymentMethodID = paymentMethod,
                    SellerID = sellerID,
                    TotalPaidAmount = totalAmount,
                    Status = PaymentStatus.Pending.ToString(),
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

        public async Task UpdatePayment(UpdatePaymentRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var curData = await context.Payments.Where(p => p.PaymentId == payload.PaymentId).FirstOrDefaultAsync() ?? throw new NotFoundException("Payment not Found");

                curData.TotalPaidAmount = payload.TotalPaidAmount ?? curData.TotalPaidAmount;
                curData.PaymentMethodID = payload.PaymentMethodID ?? curData.PaymentMethodID;
                curData.Receipt = payload.Receipt ?? curData.Receipt;
                curData.Status = payload.Status ?? curData.Status;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
