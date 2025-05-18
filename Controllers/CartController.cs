using DemoFYP.Models.Dto.Request;
using DemoFYP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemoFYP.Services.IServices;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartServices _cartServices;
        public CartController(ICartServices cartServices) {
            _cartServices = cartServices ?? throw new ArgumentNullException(nameof(cartServices));
        }

        [Authorize(Policy = "Read_Cart")]
        [HttpGet("shoppingCart")]
        public async Task<ActionResult<StandardResponse<List<ShoppingCartObj>>>> GetShoppingCart()
        {
            return SuccessResponse<List<ShoppingCartObj>>(await _cartServices.GetShoppingCart(CurUserID));
        }

        [Authorize(Policy = "Update_Cart")]
        [HttpPost("updateCart")]
        public async Task<ActionResult<StandardResponse>> UpdateShoppingCart(List<ShoppingCartRequest> payload)
        {
            await _cartServices.UpdateShoppingCart(payload, CurUserID);

            return SuccessResponse("Shopping Cart updated!");
        }
    }
}
