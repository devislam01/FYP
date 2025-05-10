using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class ProductController : BaseController
    {
       private readonly IProductServices _productServices;
       public ProductController(IProductServices productServices) {
            _productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
       }

        [HttpPost("addProduct")]
        public async Task<ActionResult<StandardResponse>> AddProduct(AddProductRequest payload)
        {
            await _productServices.AddProduct(payload, CurUserID);

            return SuccessResponse("Added Successfully!");
        }

        [HttpGet("getProductList")]
        public async Task<ActionResult<StandardResponse<List<ProductListResult>>>> GetProductList()
        {
            var result = await _productServices.GetProductList(CurUserID);

            return SuccessResponse<List<ProductListResult>>(result);
        }
    }
}
