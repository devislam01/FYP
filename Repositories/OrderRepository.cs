using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DemoFYP.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IEmailServices _emailServices;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICartRepository _cartRepository;

        public OrderRepository(IDbContextFactory<AppDbContext> factory, IEmailServices emailServices, IPaymentRepository paymentRepository, ICartRepository cartRepository) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        }

        public async Task<ProceedToPaymentResponse> CheckOutOrder(PlaceOrderRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                double totalAmount = payload.orderSummaries.Sum(item => item.UnitPrice * item.Qty);

                var productIds = payload.orderSummaries.Select(x => x.ProductID).ToList();
                var products = await context.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                foreach (var item in payload.orderSummaries)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == item.ProductID);
                    if (product == null)
                        throw new InvalidOperationException($"Product {item.ProductID} not found.");

                    if (product.StockQty < item.Qty)
                        throw new InvalidOperationException($"Product {product.ProductName} does not have enough stock.");
                }

                var order = new Order
                {
                    UserId = curUserID,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending.ToString(),
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedBy = curUserID
                };

                foreach (var item in payload.orderSummaries)
                {
                    var product = products.First(p => p.ProductId == item.ProductID);

                    product.StockQty -= item.Qty;

                    order.OrderItems.Add(new OrderItems
                    {
                        ProductID = item.ProductID,
                        Qty = item.Qty,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.UnitPrice * item.Qty,
                        CreatedAt = DateTime.Now,
                        CreatedBy = curUserID
                    });
                }

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var paymentID = await _paymentRepository.InsertPayment(order.OrderId, payload.PaymentMethod, order.TotalAmount, curUserID, context);
                order.PaymentId = paymentID;

                context.Orders.Update(order);
                await context.SaveChangesAsync();
                await trans.CommitAsync();

                return new ProceedToPaymentResponse { PaymentID = paymentID, OrderID = order.OrderId };
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task ConfirmPayment(int paymentID, string receiptUrl, Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var curData = await context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentID && p.Status == "Pending") ?? throw new NotFoundException("Payment Record Not Found!");

                curData.Receipt = receiptUrl;
                curData.Status = PaymentStatus.Paid.ToString();
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
                if (outerContext == null)
                {
                    await context.DisposeAsync();
                }
            }
        }

        public async Task ConfirmOrder(ProceedPaymentRequest payload, string receiptUrl, Guid curUserID, string curUserEmail)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                await ConfirmPayment(payload.PaymentID, receiptUrl, curUserID, context);
                var order = await context.Orders.OrderByDescending(o => o.OrderId).FirstOrDefaultAsync(o => o.UserId == curUserID) ?? throw new NotFoundException("Order not Found!");

                order.Status = OrderStatus.Completed.ToString();
                order.UpdatedDateTime = DateTime.Now;
                order.UpdatedBy = curUserID;

                string subject = $"Order {order.OrderId} Confirmed!";
                string body = $"You order {order.OrderId} has been placed successfully, kindly contact with our customer service if facing any issues, thank you!";

                await _emailServices.SendEmailAsync(curUserEmail, subject, body);

                var paidProducts = await context.OrderItems.Where(oi => oi.OrderID == payload.OrderID).Select(oi => oi.ProductID).ToListAsync();

                await _cartRepository.RemovePaidProductFromCart(paidProducts, curUserID);

                await context.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.Orders.OrderByDescending(o => o.CreatedDateTime);

                if (filter.OrderID != null && filter.OrderID > 0)
                {
                    query.Where(q => q.OrderId == filter.OrderID);
                }

                if (filter.UserID != null && filter.UserID != Guid.Empty)
                {
                    query.Where(q => q.UserId == filter.UserID);
                }

                if (filter.PaymentID != null && filter.PaymentID > 0)
                {
                    query.Where(q => q.PaymentId == filter.PaymentID);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query.Where(q => q.Status == filter.Status);
                }

                if (filter.CreatedAt != null)
                {
                    query.Where(q => q.CreatedDateTime == filter.CreatedAt);
                }

                int totalRecord = await query.CountAsync();

                var result = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(q => new OrderListResponse
                    {
                        OrderID = q.OrderId,
                        UserID = q.UserId,
                        PaymentID = q.PaymentId ?? 0,
                        TotalAmount = q.TotalAmount,
                        Status = q.Status,
                        CreatedAt = q.CreatedDateTime,
                    }).ToListAsync();

                return new PagedResult<OrderListResponse>
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

        public async Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var curData = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == payload.OrderID) ?? throw new NotFoundException("Order not Found");

                curData.TotalAmount = payload.TotalAmount ?? curData.TotalAmount;
                curData.Status = payload.Status.ToString();
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;
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
