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
        public async Task<ActionResult<StandardResponse<PagedResult<ProductListResult>>>> GetProductList(ProductFilterRequest? filter)
        {
            if (filter == null)
            {
                filter = new ProductFilterRequest
                {
                    PageNumber = 1,
                    PageSize = 10,
                    DisablePagination = false
                };
            }

            var result = await _productServices.GetProductList(filter);

            return SuccessResponse<PagedResult<ProductListResult>>(result);
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
        public async Task<ActionResult<StandardResponse<ProductDetailResponse>>> GetProductDetail(ProductDetailRequest payload)
        {
            var result = await _productServices.GetProductDetailByProductID(payload.ProductID);

            return SuccessResponse<ProductDetailResponse>(result);
        }

        #endregion

        #region Create APIs

        [Authorize(Policy = "Create_Product")]
        [HttpPost("addProduct")]
        public async Task<ActionResult<StandardResponse>> AddProduct(AddProductRequest payload)
        {
            await _productServices.AddProduct(payload, CurUserID);

            return SuccessResponse("Added Successfully!");
        }

        [Authorize(Policy = "Update_Product")]
        [HttpPost("updateProduct")]
        public async Task<ActionResult<StandardResponse>> UpdateProduct(UpdateProductRequest payload)
        {
            await _productServices.UpdateProductByProductID(payload, CurUserID);

            return SuccessResponse("Updated Successfully!");
        }

        [Authorize(Policy = "Delete_Product")]
        [HttpPost("deleteProduct")]
        public async Task<ActionResult<StandardResponse>> DeleteProduct(DeleteProductRequest payload)
        {
            await _productServices.DeleteProductByProductID(payload.ProductID, CurUserID);

            return SuccessResponse($"Product ID: { payload.ProductID } Deleted Successfully!");
        }

        #endregion
    }
}
