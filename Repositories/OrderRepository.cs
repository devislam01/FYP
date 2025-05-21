using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services;
using DemoFYP.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        #region frontend
        public async Task<List<UserOrdersResponse>> GetOrdersByBuyer(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orders = await context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == curUserID)
                    .OrderByDescending(o => o.CreatedDateTime)
                    .ToListAsync();

                return orders.Select(o => new UserOrdersResponse
                {
                    OrderID = o.OrderId,
                    Status = o.Status,
                    CreatedAt = o.CreatedDateTime,
                    OrderItems = o.OrderItems.Select(oi => new OrderItem
                    {
                        OrderItemID = oi.OrderItemID,
                        ProductName = oi.Product.ProductName,
                        Price = oi.Product.ProductPrice,
                        Quantity = oi.Qty,
                        Status = oi.Status
                    }).ToList()
                }).ToList();
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

        public async Task<List<SellerOrdersResponse>> GetOrdersBySeller(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orderItems = await context.OrderItems
                   .Include(oi => oi.Order)
                   .Include(oi => oi.Product)
                   .Where(oi => oi.Product.UserId == curUserID)
                   .OrderByDescending(oi => oi.Order.CreatedDateTime)
                   .ToListAsync();

                return orderItems.Select(oi => new SellerOrdersResponse
                {
                    OrderItemID = oi.OrderItemID,
                    ProductName = oi.Product.ProductName,
                    Price = oi.Product.ProductPrice,
                    Quantity = oi.Qty,
                    Status = oi.Status,
                    OrderID = oi.Order.OrderId,
                    BuyerID = oi.Order.UserId
                }).ToList();
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
                        Status = OrderStatus.Pending.ToString(),
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

                order.Status = OrderStatus.Processing.ToString();
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

        public async Task RequestCancelOrderByUser(RequestToCancelOrderRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var curData = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == payload.OrderID) ?? throw new NotFoundException("Order not Found");

                if (curData != null && (curData.Status == OrderStatus.Completed.ToString() || curData.Status == OrderStatus.Cancelled.ToString())) throw new BusinessException("This order not able to be cancelled anymore!");
                if (curData != null && curData.UserId != curUserID) throw new ForbiddenException("This is not your order!");

                curData.Status = payload.Status.ToString();
                curData.CancelReason = payload.CancelReason;
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

                await context.SaveChangesAsync();

                await MarkOrderItemsAsRequestCancel(curData.OrderItems, curUserID, context);

                var orderItemIds = curData.OrderItems.Select(oi => oi.OrderItemID).ToList();
                List<string> sellerEmails = await GetSellerEmailsByOrderItemIds(orderItemIds, context);

                foreach(var sellerEmail in sellerEmails)
                {
                    string subject = $"Cancellation Request of Order {payload.OrderID}";
                    string body = $"You have a request for cancelling Order ID '{payload.OrderID}', please have a look and verify whether you accept this cancellation, thank you!";

                    await _emailServices.SendEmailAsync(sellerEmail, subject, body);
                }
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

        public async Task RequestCancelOrderItemByUser(RequestToCancelOrderItemRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var curData = await context.OrderItems.FirstOrDefaultAsync(o => o.OrderItemID == payload.OrderItemID) ?? throw new NotFoundException("Order Item not Found");

                if (curData != null && (curData.Status == OrderStatus.Completed.ToString() || curData.Status == OrderStatus.Cancelled.ToString())) throw new BusinessException("This order item not able to be cancelled anymore!");

                curData.Status = payload.Status.ToString();
                curData.CancelReason = payload.CancelReason;
                curData.UpdatedAt = DateTime.Now;
                curData.UpdatedBy = curUserID;

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

        public async Task ConfirmCancelOrderItemBySeller(ConfirmCancelOrderItemRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orderItem = await context.OrderItems
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.OrderItemID == payload.OrderItemID)
                    ?? throw new NotFoundException("OrderItem not found");

                if (orderItem.Product.UserId != curUserID)
                    throw new ForbiddenException("You are not the owner of this product.");

                if (orderItem.Status != "RequestCancel")
                    throw new InvalidOperationException("This item was not requested to cancel.");

                orderItem.Status = OrderStatus.Cancelled.ToString();
                orderItem.UpdatedAt = DateTime.Now;
                orderItem.UpdatedBy = curUserID;

                await context.SaveChangesAsync();

                await UpdateProductQtyByProductID(orderItem.ProductID, orderItem.Qty, context);

                var allItems = await context.OrderItems
                    .Where(oi => oi.OrderID == orderItem.OrderID)
                    .ToListAsync();

                if (allItems.All(oi => oi.Status == OrderStatus.Cancelled.ToString()))
                {
                    var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderID);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled.ToString();
                        order.UpdatedDateTime = DateTime.Now;
                        order.UpdatedBy = curUserID;
                        await context.SaveChangesAsync();
                    }
                }

                string email = await context.Orders.Where(o => o.OrderId == orderItem.OrderID).Join(context.Users, o => o.UserId, u => u.UserId, (o, u) => u.Email).FirstOrDefaultAsync() ?? "";

                string subject = $"Order ID: '{orderItem.OrderID}' Cancelled!";
                string body = "Your order has been cancelled, please contact us if you have any issues regarding to this action. Thank you!";

                await _emailServices.SendEmailAsync(email, subject, body);
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

        public async Task RejectCancelOrderItemBySeller(RejectCancelOrderItemRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orderItem = await context.OrderItems
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.OrderItemID == payload.OrderItemID)
                    ?? throw new NotFoundException("OrderItem not found");

                if (orderItem.Product.UserId != curUserID)
                    throw new ForbiddenException("You are not the owner of this product.");

                if (orderItem.Status != "RequestCancel")
                    throw new InvalidOperationException("This item is not in RequestCancel status.");

                orderItem.Status = OrderStatus.Processing.ToString();
                orderItem.UpdatedAt = DateTime.Now;
                orderItem.UpdatedBy = curUserID;
                orderItem.CancelReason = null;

                await context.SaveChangesAsync();

                string email = await context.Orders.Where(o => o.OrderId == orderItem.OrderID).Join(context.Users, o => o.UserId, u => u.UserId, (o, u) => u.Email).FirstOrDefaultAsync() ?? "";

                string subject = $"Order ID: '{orderItem.OrderID}' Cancellation has been Rejected!";
                string body = "Your order has been rejected, please contact seller personally if you have any issues regarding to this. Thank you!";

                await _emailServices.SendEmailAsync(email, subject, body);
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

        public async Task MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orderItem = await context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.OrderItemID == payload.OrderItemID) ?? throw new NotFoundException("Order item not found");

                if (orderItem.Product.UserId != curUserID)
                    throw new ForbiddenException("You do not own this product");

                orderItem.Status = OrderStatus.Completed.ToString();
                orderItem.UpdatedAt = DateTime.Now;
                orderItem.UpdatedBy = curUserID;

                await context.SaveChangesAsync();

                var orderId = orderItem.OrderID;
                var allOrderItems = await context.OrderItems
                    .Where(oi => oi.OrderID == orderId)
                    .ToListAsync();

                if (allOrderItems.All(oi => oi.Status == OrderStatus.Completed.ToString()))
                {
                    var order = orderItem.Order;

                    order.Status = OrderStatus.Completed.ToString();
                    order.UpdatedDateTime = DateTime.Now;
                    order.UpdatedBy = curUserID;

                    await context.SaveChangesAsync();
                }
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
        #endregion

        #region Admin
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

                if (!string.IsNullOrEmpty(filter.Status.ToString()))
                {
                    query.Where(q => q.Status == filter.Status.ToString());
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

        public async Task CancelOrderByAdmin(CancelOrderRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                await CancelOrderItems(payload.OrderID, curUserID, context);

                var curData = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == payload.OrderID) ?? throw new NotFoundException("Order not Found");

                curData.Status = OrderStatus.Cancelled.ToString();
                curData.UpdatedDateTime = DateTime.Now;
                curData.UpdatedBy = curUserID;

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

        public async Task CancelOrderItemByAdmin(CancelOrderItemRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var curData = await context.OrderItems.FirstOrDefaultAsync(oi => oi.OrderItemID == payload.OrderItemID);

                curData.CancelReason = payload.CancelReason;
                curData.UpdatedBy = curUserID;
                curData.UpdatedAt = DateTime.Now;

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
        #endregion

        #region Common
        public async Task CancelOrderItems(int orderID, Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var curItems = await context.OrderItems
                    .Where(oi => oi.OrderID == orderID)
                    .ToListAsync();

                foreach (var item in curItems)
                {
                    item.Status = OrderStatus.Cancelled.ToString();
                    item.UpdatedBy = curUserID;
                    item.UpdatedAt = DateTime.Now;
                }

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

        public async Task<List<string>> GetSellerEmailsByOrderItemIds(List<int> orderItemIds, AppDbContext outerContext)
        {
            using var context = outerContext ?? _factory.CreateDbContext();

            var emails = await context.OrderItems
                .Where(oi => orderItemIds.Contains(oi.OrderItemID))
                .Join(context.Products,
                      oi => oi.ProductID,
                      p => p.ProductId,
                      (oi, p) => new { oi, p })
                .Join(context.Users,
                      temp => temp.p.UserId,
                      u => u.UserId,
                      (temp, u) => u.Email)
                .Distinct()
                .ToListAsync();

            return emails;
        }

        public async Task SendCancellationRequestEmailToSeller(int orderItemID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                string sellerEmail = await context.OrderItems
                        .Where(oi => oi.OrderItemID == orderItemID)
                        .Join(context.Products,
                            oi => oi.ProductID,
                            p => p.ProductId,
                            (oi, p) => new { oi, p })
                        .Join(context.Users,
                            temp => temp.p.UserId,
                            u => u.UserId,
                            (temp, u) => u.Email)
                        .FirstOrDefaultAsync() ?? throw new NotFoundException("Seller Email not Found");

                string subject = $"Cancellation Request of Order {orderItemID}";
                string body = $"You have a request for cancelling Order ID '{orderItemID}', please have a look and verify whether you accept this cancellation, thank you!";

                bool isSucess = await _emailServices.SendEmailAsync(sellerEmail, subject, body);

                if (!isSucess) throw new BusinessException("Seller's email not found. Please contact seller manually!");
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

        private static async Task MarkOrderItemsAsRequestCancel(List<OrderItems> orderItems, Guid userId, AppDbContext context)
        {
            foreach (var item in orderItems)
            {
                item.Status = OrderStatus.RequestCancel.ToString();
                item.UpdatedAt = DateTime.Now;
                item.UpdatedBy = userId;
            }

            await context.SaveChangesAsync();
        }

        public async Task UpdateProductQtyByProductID(int productID, int orderQty, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var product = await context.Products.FindAsync(productID) ?? throw new NotFoundException($"Product ID '{productID}' not Found");

                product.StockQty += orderQty;

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
        #endregion
    }
}
