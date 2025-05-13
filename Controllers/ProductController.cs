using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Controllers
{
    public class ProductController : BaseController
    {
       private readonly IProductServices _productServices;

       public ProductController(IProductServices productServices) {
            _productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
       }

        #region Read APIs

        [HttpPost("getProductList")]
        public async Task<ActionResult<StandardResponse<List<ProductListResult>>>> GetProductList(ProductFilterRequest filter)
        {
            var result = await _productServices.GetProductList(filter);

            return SuccessResponse<List<ProductListResult>>(result);
        }

        [Authorize]
        [HttpGet("getFilteredProductList")]
        public async Task<ActionResult<StandardResponse<List<FilteredProductListResult>>>> GetFilteredProductList()
        {
            var result = await _productServices.GetProductListByLoginID(CurUserID);

            return SuccessResponse<List<FilteredProductListResult>>(result);
        }

        [Authorize]
        [HttpPost("getProductDetail")]
        public async Task<ActionResult<StandardResponse<ProductDetailResult>>> GetProductDetail(ProductDetailRequest payload)
        {
            var result = await _productServices.GetProductDetailByProductID(payload.ProductID, CurUserID);

            return SuccessResponse<ProductDetailResult>(result);
        }

        #endregion

        #region Create APIs

        [Authorize]
        [HttpPost("addProduct")]
        public async Task<ActionResult<StandardResponse>> AddProduct(AddProductRequest payload)
        {
            await _productServices.AddProduct(payload, CurUserID);

            return SuccessResponse("Added Successfully!");
        }

        [Authorize]
        [HttpPost("updateProduct")]
        public async Task<ActionResult<StandardResponse>> UpdateProduct(UpdateProductRequest payload)
        {
            await _productServices.UpdateProductByProductID(payload, CurUserID);

            return SuccessResponse("Updated Successfully!");
        }

        [Authorize]
        [HttpPost("deleteProduct")]
        public async Task<ActionResult<StandardResponse>> DeleteProduct(DeleteProductRequest payload)
        {
            await _productServices.DeleteProductByProductID(payload.ProductID, CurUserID);

            return SuccessResponse($"Product ID: { payload.ProductID } Deleted Successfully!");
        }

        #endregion
    }
}
