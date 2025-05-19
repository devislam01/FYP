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

        public OrderService(IOrderRepository orderRepository, ICommonServices commonServices) {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }

        public async Task<ProceedToPaymentResponse> CheckOut(PlaceOrderRequest payload, Guid curUserID)
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
            if (payload.PaymentID == 0) throw new BadRequestException("Payment ID is required");
            if (payload.Receipt != null && payload.Receipt.Length == 0) throw new BadRequestException("Receipt is required");

            try
            {
                string receiptUrl = await _commonServices.UploadImage(payload.Receipt, "", "Receipt");

                await _orderRepository.ConfirmOrder(payload, receiptUrl, curUserID, curUserEmail);
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

        #endregion
    }
}
