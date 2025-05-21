using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoFYP.Controllers.AdminPortal
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    public class OrderController : BaseController
    {
        private readonly IOrderServices _orderServices;
        public OrderController(IOrderServices orderServices) {
            _orderServices = orderServices ?? throw new ArgumentNullException(nameof(orderServices));
        }

        [Authorize(Policy = "AP_Read_Order")]
        [HttpGet]
        public async Task<ActionResult<StandardResponse<PagedResult<OrderListResponse>>>> GetOrderList(OrderListFilterRequest filter)
        {
            var result = await _orderServices.GetOrderList(filter);

            return SuccessResponse<PagedResult<OrderListResponse>>(result);
        }

        [Authorize(Policy = "AP_Update_Order")]
        [HttpGet]
        public async Task<ActionResult<StandardResponse>> UpdateOrder(UpdateOrderRequest payload)
        {
            await _orderServices.UpdateOrder(payload, CurUserID);

            return SuccessResponse($"Order ID: '{payload.OrderID}' Updated Successfully!");
        }

        [Authorize(Policy = "AP_Cancel_Order")]
        [HttpGet]
        public async Task<ActionResult<StandardResponse>> CancelOrder(CancelOrderRequest payload)
        {
            await _orderServices.CancelOrder(payload, CurUserID);

            return SuccessResponse($"Order ID: '{payload.OrderID}' Cancelled Successfully!");
        }

        [Authorize(Policy = "AP_Cancel_Order_Item")]
        [HttpGet]
        public async Task<ActionResult<StandardResponse>> CancelOrderItem(CancelOrderItemRequest payload)
        {
            await _orderServices.CancelOrderItem(payload, CurUserID);

            return SuccessResponse($"Order Item ID: '{payload.OrderItemID}' Cancelled Successfully!");
        }
    }
}
