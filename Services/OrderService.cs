using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class OrderService : IOrderServices
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICommonServices _commonServices;
        private readonly ICartServices _cartServices;

        public OrderService(IOrderRepository orderRepository, ICommonServices commonServices, ICartServices cartServices)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _cartServices = cartServices ?? throw new ArgumentNullException(nameof(cartServices));
        }

        public async Task<List<UserOrdersResponse>> GetOrdersByBuyer(Guid curUserID)
        {
            try
            {
                return await _orderRepository.GetOrdersByBuyer(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SellerOrdersResponse>> GetOrdersBySeller(Guid curUserID)
        {
            try
            {
                return await _orderRepository.GetOrdersBySeller(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<OrderSummariesResponse> GetOrderSummaries(List<int> productIDs, Guid curUserID)
        {
            try
            {
                List<ShoppingCartObj> shoppingCart = await _cartServices.GetShoppingCartByProductIDs(productIDs, curUserID);

                double total = shoppingCart.Sum(sc => sc.ProductPrice * sc.Quantity);

                return new OrderSummariesResponse
                {
                    shoppingCartObjs = shoppingCart,
                    Total = total
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<CheckoutResponse> CheckOut(PlaceOrderRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.orderSummaries == null) throw new BadRequestException("Your Cart is Empty");
            if (string.IsNullOrEmpty(payload.ShippingWay)) throw new BadRequestException("Shipping Way is required");
            if (payload.PaymentMethod == 0) throw new BadRequestException("Payment Method is required");

            try
            {
                return await _orderRepository.CheckOutOrder(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task ConfirmOrder(ProceedPaymentRequest payload, Guid curUserID, string curUserEmail)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.ReceiptList != null && payload.ReceiptList.Count == 0) throw new BadRequestException("At least 1 receipt is required");

            try
            {
                Dictionary<int, string> receiptUrls = [];

                foreach (var receipt in payload.ReceiptList)
                {
                    if (receipt.Receipt != null && receipt.Receipt.Length > 0)
                    {
                        var url = await _commonServices.UploadImage(receipt.Receipt, "", FolderName.Receipt.ToString());

                        receiptUrls.Add(receipt.PaymentID, url);
                    }
                    else
                    {
                        receiptUrls.Add(receipt.PaymentID, "");
                    }
                }

                await _orderRepository.ConfirmOrder(payload, receiptUrls, curUserID, curUserEmail);
            }
            catch
            {
                throw;
            }
        }

        public async Task RequestToCancelOrder(RequestToCancelOrderRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (string.IsNullOrEmpty(payload.CancelReason)) throw new BadRequestException("Cancel Reason is required");

            try
            {
                await _orderRepository.RequestCancelOrderByUser(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task RequestToCancelOrderItem(RequestToCancelOrderItemRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (string.IsNullOrEmpty(payload.CancelReason)) throw new BadRequestException("Cancel Reason is required");

            try
            {
                await _orderRepository.RequestCancelOrderItemByUser(payload, curUserID);
                await _orderRepository.SendCancellationRequestEmailToSeller(payload.OrderItemID);
            }
            catch
            {
                throw;
            }
        }

        public async Task ConfirmCancelOrderItemBySeller(ConfirmCancelOrderItemRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.OrderItemID == 0) throw new BadRequestException("Order Item ID is required");

            try
            {
                await _orderRepository.ConfirmCancelOrderItemBySeller(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task RejectCancelBySeller(RejectCancelOrderItemRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.OrderItemID == 0) throw new BadRequestException("Order Item ID is required");

            try
            {
                await _orderRepository.RejectCancelOrderItemBySeller(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.OrderItemID == 0) throw new BadRequestException("Order Item ID is required");

            try
            {
                await _orderRepository.MarkOrderItemAsCompleted(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task MarkOrderAsCompleted(MarkOrderCompletedRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.OrderID == 0) throw new BadRequestException("Order ID is required");

            try
            {
                await _orderRepository.MarkOrderAsCompleted(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task RateProduct(RateProductRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.OrderItemID == 0) throw new BadRequestException("Order Item ID is required");
            if (payload.Rating == 0) throw new BadRequestException("You have to rate the product!");

            try
            {
                var sellerID = await _orderRepository.RateProduct(payload, curUserID);
                await _orderRepository.CalculateAndUpdateSellerRatingMark(sellerID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResult<FeedbackListResponse>> GetFeedbackList(FeedbackListRequest filter)
        {
            try
            {
                return await _orderRepository.GetFeedbackList(filter);
            }
            catch
            {
                throw;
            }
        }

        #region Admin Services

        public async Task<PagedResult<OrderListResponse>> GetOrderList(OrderListFilterRequest filter)
        {
            try
            {
                return await _orderRepository.GetOrderList(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateOrder(UpdateOrderRequest payload, Guid curUserID)
        {
            try
            {
                await _orderRepository.UpdateOrder(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task CancelOrder(CancelOrderRequest payload, Guid curUserID)
        {
            try
            {
                await _orderRepository.CancelOrderByAdmin(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task CancelOrderItem(CancelOrderItemRequest payload, Guid curUserID)
        {
            try
            {
                await _orderRepository.CancelOrderItemByAdmin(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
