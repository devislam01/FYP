using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderServices _orderServices;
        public OrderController(IOrderServices orderServices) {
            _orderServices = orderServices ?? throw new ArgumentNullException(nameof(orderServices));
        }

        [Authorize]
        [HttpGet("as-buyer")]
        public async Task<ActionResult<StandardResponse<List<UserOrdersResponse>>>> GetOrdersAsBuyer()
        {
            return SuccessResponse<List<UserOrdersResponse>>(await _orderServices.GetOrdersByBuyer(CurUserID));
        }

        [Authorize]
        [HttpGet("as-seller")]
        public async Task<ActionResult<StandardResponse<List<SellerOrdersResponse>>>> GetOrdersAsSeller()
        {
            return SuccessResponse<List<SellerOrdersResponse>>(await _orderServices.GetOrdersBySeller(CurUserID));
        }

        [Authorize]
        [HttpGet("orderSummaries")]
        public async Task<ActionResult<StandardResponse<OrderSummariesResponse>>> GetOrderSummaries()
        {
            return SuccessResponse<OrderSummariesResponse>(await _orderServices.GetOrderSummaries(CurUserID));
        } 

        [Authorize]
        [HttpPost("checkout")]
        public async Task<ActionResult<StandardResponse<ProceedToPaymentResponse>>> Checkout(PlaceOrderRequest payload)
        {
            return SuccessResponse<ProceedToPaymentResponse>(await _orderServices.CheckOut(payload, CurUserID), "Please proceed to Payment");
        }

        [Authorize]
        [HttpPost("confirmOrder")]
        public async Task<ActionResult<StandardResponse>> ConfirmOrder(ProceedPaymentRequest payload)
        {
            await _orderServices.ConfirmOrder(payload, CurUserID, CurUserEmail);

            return SuccessResponse("Order is Confirmed!");
        }

        [Authorize]
        [HttpPost("request_cancelOrder")]
        public async Task<ActionResult<StandardResponse>> RequestToCancelOrder(RequestToCancelOrderRequest payload)
        {
            await _orderServices.RequestToCancelOrder(payload, CurUserID);

            return SuccessResponse("Seller will receive an email to verify your cancellation request!");
        }

        [Authorize]
        [HttpPost("request_cancelOrderItem")]
        public async Task<ActionResult<StandardResponse>> RequestToCancelOrderItem(RequestToCancelOrderItemRequest payload)
        {
            await _orderServices.RequestToCancelOrderItem(payload, CurUserID);

            return SuccessResponse("Seller will receive an email to verify your cancellation request!");
        }

        [Authorize]
        [HttpPost("confirm-cancel")]
        public async Task<ActionResult<StandardResponse>> ConfirmCancelBySeller(ConfirmCancelOrderItemRequest payload)
        {
            await _orderServices.ConfirmCancelOrderItemBySeller(payload, CurUserID);
            return SuccessResponse("Cancellation confirmed.");
        }

        [Authorize]
        [HttpPost("reject-cancel")]
        public async Task<ActionResult<StandardResponse>> RejectCancelBySeller(RejectCancelOrderItemRequest payload)
        {
            await _orderServices.RejectCancelBySeller(payload, CurUserID);
            return SuccessResponse("Cancellation rejected.");
        }

        [Authorize]
        [HttpPost("mark-complete")]
        public async Task<ActionResult<StandardResponse>> MarkOrderItemAsCompleted(MarkOrderItemCompletedRequest payload)
        {
            await _orderServices.MarkOrderItemAsCompleted(payload, CurUserID);
            return SuccessResponse("Order item marked as completed");
        }

        [Authorize]
        [HttpPost("rate-product")]
        public async Task<ActionResult<StandardResponse>> RateProduct(RateProductRequest payload)
        {
            await _orderServices.RateProduct(payload, CurUserID);
            return SuccessResponse("Thanks for your rating!");
        }

        [Authorize]
        [HttpPost("feedback")]
        public async Task<ActionResult<StandardResponse<PagedResult<FeedbackListResponse>>>> GetFeedbackList(FeedbackListRequest filter)
        {
            return SuccessResponse<PagedResult<FeedbackListResponse>>(await _orderServices.GetFeedbackList(filter));
        }
    }
}
