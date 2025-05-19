using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
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

            return SuccessResponse("Please proceed to Payment");
        }
    }
}
