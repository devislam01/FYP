using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Models;
using Microsoft.AspNetCore.Mvc;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Authorization;

namespace DemoFYP.Controllers.AdminPortal
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IProductServices _productServices;

        public ProductController(IProductServices productServices)
        {
            _productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
        }

        [Authorize(Policy = "AP_Read_Product")]
        [HttpPost("productList")]
        public async Task<ActionResult<StandardResponse<PagedResult<AdminProductListResult>>>> GetProductList(AdminProductFilterRequest? filter)
        {
            if (filter == null)
            {
                filter = new AdminProductFilterRequest
                {
                    PageNumber = 1,
                    PageSize = 10,
                    DisablePagination = false
                };
            }

            var result = await _productServices.GetProductListByAdmin(filter);

            return SuccessResponse<PagedResult<AdminProductListResult>>(result);
        }

        [Authorize(Policy = "AP_Read_ProductDetail")]
        [HttpPost("productDetail")]
        public async Task<ActionResult<StandardResponse<ProductDetailResponse>>> GetProductDetail(ProductDetailRequest payload)
        {
            return SuccessResponse<ProductDetailResponse>(await _productServices.GetProductDetailByProductID(payload.ProductID));
        }

        [Authorize(Policy = "AP_Update_Product")]
        [HttpPost("updateProduct")]
        public async Task<ActionResult<StandardResponse>> UpdateProduct(UpdateProductRequest payload)
        {
            await _productServices.UpdateProductByProductID(payload, CurUserID);

            return SuccessResponse($"Product ID: '{payload.ProductID}' Updated Successfully");
        }

        [Authorize(Policy = "AP_Delete_Product")]
        [HttpPost("unpublish-product")]
        public async Task<ActionResult<StandardResponse>> UnpublishProduct(DeleteProductRequest payload)
        {
            await _productServices.UnpublishProductByAdmin(payload.ProductID, CurUserID);

            return SuccessResponse($"Product ID: '{payload.ProductID}' has been unpublished");
        }

        [Authorize(Policy = "AP_Publish_Product")]
        [HttpPost("publish-product")]
        public async Task<ActionResult<StandardResponse>> PublishProduct(PublishProductRequest payload)
        {
            await _productServices.PublishProductByAdmin(payload.ProductID, CurUserID);

            return SuccessResponse($"Product ID: '{payload.ProductID}' has been published");
        }
    }
}
