using DemoFYP.EF;
using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IConfiguration _config;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderRepository(IDbContextFactory<AppDbContext> factory, IEmailServices emailServices, IPaymentRepository paymentRepository, ICartRepository cartRepository, IConfiguration config, IHubContext<OrderHub> hubContext) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        #region frontend
        public async Task<List<UserOrdersResponse>> GetOrdersByBuyer(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var orders = context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payment)
                    .Where(o => o.UserId == curUserID)
                    .OrderByDescending(o => o.CreatedDateTime)
                    .ToList();

                var allSellerIds = orders
                    .SelectMany(o => o.OrderItems)
                    .Select(oi => oi.Product?.UserId)
                    .Where(id => id.HasValue)
                    .Distinct()
                    .ToList();

                var sellerUserMap = context.Users
                    .Where(u => allSellerIds.Contains(u.UserId))
                    .ToDictionary(u => u.UserId, u => new { u.UserName, u.PhoneNumber });

                return orders.Select(o => new UserOrdersResponse
                {
                    OrderID = o.OrderId,
                    TotalAmt = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedDateTime,
                    PaymentMethodID = o.Payment.Select(p => p.PaymentMethodID).FirstOrDefault(0),

                    OrderItems = o.OrderItems
                        .GroupBy(oi => oi.Product.UserId)
                        .Select(group =>
                        {
                            var sellerId = group.Key;

                            var sellerInfo = sellerUserMap.TryGetValue(sellerId, out var info)
                                ? info
                                : new { UserName = string.Empty, PhoneNumber = string.Empty };

                            var paymentForSeller = o.Payment.FirstOrDefault(p => p.SellerID == sellerId);
                            var receipt = !string.IsNullOrWhiteSpace(paymentForSeller?.Receipt)
                                ? $"{_config["BackendUrl"]}/{paymentForSeller.Receipt}"
                                : null;

                            return new OrderSellerGroupVO
                            {
                                SellerName = sellerInfo.UserName ?? "",
                                SellerPhoneNo = sellerInfo.PhoneNumber ?? "",
                                Receipt = receipt,
                                Items = group.Select(oi => new OrderItemVO
                                {
                                    OrderItemID = oi.OrderItemID,
                                    ProductName = oi.Product.ProductName,
                                    Price = oi.Product?.ProductPrice ?? 0,
                                    Quantity = oi.Qty,
                                    ProductImage = string.IsNullOrWhiteSpace(oi.Product?.ProductImage)
                                        ? string.Empty
                                        : $"{_config["BackendUrl"]}/{oi.Product.ProductImage}",
                                    Status = oi.Status,
                                    ProductID = oi.Product.ProductId
                                }).ToList()
                            };
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
                       .ThenInclude(o => o.Payment)
                   .Include(oi => oi.Product)
                   .Where(oi => oi.Product.UserId == curUserID)
                   .OrderByDescending(oi => oi.Order.CreatedDateTime)
                   .ToListAsync();

                var buyerIds = orderItems.Select(oi => oi.Order.UserId).Distinct().ToList();

                var buyers = await context.Users
                    .Where(u => buyerIds.Contains(u.UserId))
                    .Select(u => new { u.UserId, u.UserName, u.PhoneNumber })
                    .ToDictionaryAsync(u => u.UserId, u => new { u.UserName, u.PhoneNumber });

                var groupedOrders = orderItems.GroupBy(oi => oi.Order.OrderId);

                var result = new List<SellerOrdersResponse>();

                foreach (var group in groupedOrders)
                {
                    var firstOrderItem = group.First();
                    var order = firstOrderItem.Order;

                    double totalAmt = group.Sum(oi => oi.Product.ProductPrice * oi.Qty);

                    var payment = order.Payment.FirstOrDefault(p => p.SellerID == curUserID);

                    string paymentStatus = payment?.Status ?? PaymentStatus.Pending.ToString();

                    var receipt = !string.IsNullOrWhiteSpace(payment?.Receipt)
                            ? $"{_config["BackendUrl"]}/{payment.Receipt}"
                            : string.Empty;

                    var buyerInfo = buyers.TryGetValue(order.UserId, out var b)
                            ? b
                            : new { UserName = string.Empty, PhoneNumber = string.Empty };

                    var orderResponse = new SellerOrdersResponse
                    {
                        OrderID = order.OrderId,
                        TotalAmt = totalAmt,
                        Status = order.Status,
                        CreatedAt = order.CreatedDateTime,
                        PaymentStatus = paymentStatus,
                        Receipt = receipt,
                        BuyerID = order.UserId,
                        BuyerName = buyerInfo.UserName,
                        BuyerPhoneNo = buyerInfo.PhoneNumber,
                        PaymentMethodID = payment?.PaymentMethodID ?? 0,
                        OrderItems = new List<SellerOrderItemVO>()
                    };

                    foreach (var oi in group)
                    {
                        orderResponse.OrderItems.Add(new SellerOrderItemVO
                        {
                            OrderItemID = oi.OrderItemID,
                            ProductName = oi.Product.ProductName,
                            ProductImage = string.IsNullOrWhiteSpace(oi.Product.ProductImage)
                                ? string.Empty
                                : $"{_config["BackendUrl"]}/{oi.Product.ProductImage}",
                            Price = oi.Product.ProductPrice,
                            Quantity = oi.Qty,
                            Status = oi.Status,
                        });
                    }

                    result.Add(orderResponse);
                }

                return result;
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

        public async Task<CheckoutResponse> CheckOutOrder(PlaceOrderRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var productIds = payload.orderSummaries.Select(x => x.ProductID).ToList();
                var products = await context.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                foreach (var item in payload.orderSummaries)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == item.ProductID);
                    if (product == null)
                        throw new NotFoundException($"Product {item.ProductID} not found.");

                    if (product.StockQty < item.Qty)
                        throw new BusinessException($"Product {product.ProductName} does not have enough stock.");

                    if (product.UserId == curUserID)
                        throw new BusinessException($"You cannot purchase your own product! Product: {product.ProductName}");
                }

                double totalAmount = payload.orderSummaries.Sum(x => x.Qty * x.UnitPrice);
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
                        Subtotal = item.Qty * item.UnitPrice,
                        Status = OrderStatus.Pending.ToString(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = curUserID
                    });
                }

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var response = new CheckoutResponse();

                var ordersGroupedBySeller = payload.orderSummaries
                    .GroupBy(x => products.First(p => p.ProductId == x.ProductID).UserId);

                foreach (var group in ordersGroupedBySeller)
                {
                    var sellerId = group.Key;
                    double sellerAmount = group.Sum(x => x.Qty * x.UnitPrice);

                    var paymentID = await _paymentRepository.InsertPayment(order.OrderId, payload.PaymentMethod, sellerId, sellerAmount, curUserID, context);

                    var paymentQRCode = await context.Users
                        .Where(u => u.UserId == sellerId)
                        .Select(u => u.PaymentQRCode)
                        .FirstOrDefaultAsync()
                        ?? throw new NotFoundException($"Product '{string.Join(", ", group.Select(x => $"{products.FirstOrDefault(p => p.ProductId == x.ProductID).ProductName}"))}' doesn't support for QR Payment!");

                    response.ProceedToPayments.Add(new ProceedToPayment
                    {
                        PaymentID = paymentID,
                        OrderID = order.OrderId,
                        QRCode = string.IsNullOrWhiteSpace(paymentQRCode)
                            ? string.Empty
                            : $"{_config["BackendUrl"]}/{paymentQRCode}",
                        ProductName = products.FirstOrDefault(p => p.ProductId == group.First().ProductID)?.ProductName ?? string.Empty,
                        Price = sellerAmount,
                    });
                }

                await context.SaveChangesAsync();
                await trans.CommitAsync();

                return response;
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

        public async Task ConfirmPayment(Dictionary<int, string> receipts, Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var payments = await context.Payments
                    .Where(p => receipts.Keys.ToList().Contains(p.PaymentId))
                    .ToListAsync();

                if (payments == null || payments.Count == 0)
                {
                    throw new NotFoundException("No pending payment records found!");
                }

                foreach (var payment in payments)
                {
                    if (receipts.TryGetValue(payment.PaymentId, out var receiptUrl))
                    {
                        payment.Receipt = receiptUrl;
                    }
                    else
                    {
                        payment.Receipt = null;
                    }

                    payment.Status = PaymentStatus.Paid.ToString();
                    payment.UpdatedDateTime = DateTime.Now;
                    payment.UpdatedBy = curUserID;
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

        public async Task ConfirmOrder(ProceedPaymentRequest payload, Dictionary<int, string> receiptUrls, Guid curUserID, string curUserEmail)
        {
            var context = _factory.CreateDbContext();
            await using var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var payments = await context.Payments
                    .Where(p => payload.ReceiptList.Select(r => r.PaymentID).Contains(p.PaymentId))
                    .Select(p => new { p.PaymentId, p.PaymentMethodID })
                    .ToListAsync();

                foreach(var payment in payments)
                {
                    switch (payment.PaymentMethodID)
                    {
                        case 1:
                            await ConfirmPayment(receiptUrls, curUserID, context);
                            break;

                        case 2:
                            if (receiptUrls == null || receiptUrls.Count == 0)
                                throw new BadRequestException("You have to upload your receipt!");
                            await ConfirmPayment(receiptUrls, curUserID, context);
                            break;

                        default:
                            throw new BadRequestException("Unsupported payment method.");
                    }
                }
                

                await MarkOrderToProcessing(curUserID, context);
                await MarkOrderItemsToProcessing(payload.OrderID, curUserID, context);

                string subject = $"Order {payload.OrderID} Confirmed!";
                string body = $"You order {payload.OrderID} has been placed successfully, kindly contact with our customer service if facing any issues, thank you!";

                await _emailServices.SendEmailAsync(curUserEmail, subject, body);
                await OrderHub.NotifyUserAsync(_hubContext, curUserID, $"Order {payload.OrderID} bought successfully!");

                var paidProducts = await context.OrderItems.Where(oi => oi.OrderID == payload.OrderID).Select(oi => oi.ProductID).ToListAsync();

                await _cartRepository.RemovePaidProductFromCart(paidProducts, curUserID);

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

        public async Task MarkOrderAsCompleted(MarkOrderCompletedRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var order = await context.Orders
                    .FirstOrDefaultAsync(oi => oi.OrderId == payload.OrderID) ?? throw new NotFoundException("Order not found");

                order.Status = OrderStatus.Completed.ToString();
                order.UpdatedDateTime = DateTime.Now;
                order.UpdatedBy = curUserID;

                await context.SaveChangesAsync();

                var orderItems = await context.OrderItems
                    .Where(oi => oi.OrderID == payload.OrderID)
                    .ToListAsync();

                foreach (var item in orderItems)
                {
                    item.Status = OrderStatus.Completed.ToString();
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = curUserID;

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


        public async Task<Guid> RateProduct(RateProductRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var existingReview = await context.SellerReviews
                    .AnyAsync(r => r.OrderItemID == payload.OrderItemID && r.BuyerID == curUserID);

                if (existingReview) throw new BusinessException("You have already submitted a review for this item.");

                var orderItem = await context.OrderItems
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.OrderItemID == payload.OrderItemID) ?? throw new NotFoundException("Order item not found");

                var newData = new SellerReview
                {
                    OrderItemID = orderItem.OrderItemID,
                    SellerID = orderItem.Product.UserId,
                    BuyerID = curUserID,
                    Rating = payload.Rating,
                    Feedback = payload.Feedback,
                    CreatedAt = DateTime.Now,
                    CreatedBy = curUserID,
                };

                await context.SellerReviews.AddAsync(newData);
                await context.SaveChangesAsync();

                return newData.SellerID;
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

        public async Task<PagedResult<FeedbackListResponse>> GetFeedbackList(FeedbackListRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.SellerReviews
                    .Where(sr => sr.OrderItemID == filter.OrderItemID)
                    .Join(context.Users,
                        sr => sr.BuyerID,
                        u => u.UserId,
                        (sr, u) => new FeedbackListResponse
                        {
                            BuyerName = u.UserName,
                            Rating = sr.Rating,
                            Feedbacks = sr.Feedback,
                            FeedbackAt = sr.CreatedAt,
                        });

                int totalRecord = await query.CountAsync();

                var result = await query
                    .OrderByDescending(sr => sr.FeedbackAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PagedResult<FeedbackListResponse>
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

        public async Task MarkOrderToProcessing(Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var order = await context.Orders.OrderByDescending(o => o.OrderId).FirstOrDefaultAsync(o => o.UserId == curUserID) ?? throw new NotFoundException("Order not Found!");

                order.Status = OrderStatus.Processing.ToString();
                order.UpdatedDateTime = DateTime.Now;
                order.UpdatedBy = curUserID;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task MarkOrderItemsToProcessing(int orderID, Guid curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();

            try
            {
                var orderItems = await context.OrderItems.OrderByDescending(o => o.OrderItemID).Where(o => o.OrderID == orderID).ToListAsync() ?? throw new NotFoundException("Order Items not Found!");

                foreach (var item in orderItems)
                {
                    item.Status = OrderStatus.Processing.ToString();
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = curUserID;
                }

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task CalculateAndUpdateSellerRatingMark(Guid sellerID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var avg = await context.SellerReviews
                    .Where(x => x.SellerID == sellerID)
                    .AverageAsync(x => x.Rating);

                var seller = await context.Users.FirstOrDefaultAsync(x => x.UserId == sellerID);

                if (seller != null)
                {
                    seller.RatingMark = Math.Round(avg, 2);
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
    }
}
