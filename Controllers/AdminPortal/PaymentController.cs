using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers.AdminPortal
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentServices _paymentServices;

        public PaymentController(IPaymentServices paymentServices) { 
            _paymentServices = paymentServices ?? throw new ArgumentNullException(nameof(paymentServices));
        }

        [Authorize(Policy = "AP_Read_Payment")]
        [HttpPost("paymentList")]
        public async Task<ActionResult<StandardResponse<PagedResult<PaymentListResponse>>>> GetPaymentList(PaymentListFilterRequest filter)
        {
            return SuccessResponse<PagedResult<PaymentListResponse>>(await _paymentServices.GetPaymentList(filter));
        }

        [Authorize(Policy = "AP_Read_PaymentDetail")]
        [HttpPost("paymentDetail")]
        public async Task<ActionResult<StandardResponse<PaymentListResponse>>> GetPaymentDetail(PaymentDetailRequest payload)
        {
            return SuccessResponse<PaymentListResponse>(await _paymentServices.GetPaymentDetail(payload.PaymentID));
        }

        [Authorize(Policy = "AP_Update_Payment")]
        [HttpPost("updatePayment")]
        public async Task<ActionResult<StandardResponse>> UpdatePaymentRecord(UpdatePaymentRequest payload)
        {
            await _paymentServices.UpdatePayment(payload, CurUserID);

            return SuccessResponse($"Payment ID: '{payload.PaymentId}' Updated Successfully!");
        }
    }
}
