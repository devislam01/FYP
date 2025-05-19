using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class DropdownController : BaseController
    {
        private readonly IDropdownServices _dropdownServices;

        public DropdownController(IDropdownServices dropdownServices) {
            _dropdownServices = dropdownServices ?? throw new ArgumentNullException(nameof(dropdownServices));
        }

        [Authorize(Policy = "CREATE_PAYMENT_METHOD")]
        [HttpPost("create-payment-methods")]
        public async Task<ActionResult<StandardResponse>> InsertPaymentMethod(PaymentMethodDropdownRequest payload)
        {
            await _dropdownServices.InsertPaymentMethod(payload, CurUserID);

            return SuccessResponse($"Payment Method: '{payload.PaymentMethodName}' Created Successfully!");
        }
    }
}
