using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Services.IServices
{
    public interface IProductServices
    {
        #region Read Services

        Task<PagedResult<ProductListResult>> GetProductList(ProductFilterRequest filter);
        Task<PagedResult<AdminProductListResult>> GetProductListByAdmin(AdminProductFilterRequest filter);
        Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID);
        Task<ProductDetailResponse> GetProductDetailByProductID(int ProductID);

        #endregion

        #region Create Services

        Task AddProduct(AddProductRequest product, Guid curUserID);

        #endregion

        #region Update Services

        Task UpdateProductByProductID(UpdateProductRequest product, Guid curUserID);
        Task DeleteProductByProductID(int productID, Guid curUserID);
        Task UnpublishProductByAdmin(int productID, Guid curUserID);

        #endregion
    }
}
