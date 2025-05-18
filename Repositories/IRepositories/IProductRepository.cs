using DemoFYP.EF;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using Microsoft.AspNetCore.Mvc;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IProductRepository
    {
        #region Read
        Task<PagedResult<ProductListResult>> GetProductList(ProductFilterRequest filter);
        Task<PagedResult<AdminProductListResult>> GetProductListByAdmin(AdminProductFilterRequest filter);
        Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID);
        Task<ProductDetailResponse> GetProductDetailByProductID(int ProductID, bool GetImageRealPath = false);

        #endregion

        #region Create

        Task InsertProduct(AddProductRequest paylaod, Guid curUserID, string ImageURL);

        #endregion

        #region Update

        Task UpdateProductByProductID(UpdateProductRequest paylaod, Guid curUserID, string ImageURL);
        Task DeleteProductByProductID(int productID, Guid curUserID);
        Task UnpublishProductByAdmin(int productID, Guid curUserID);
        #endregion
    }
}
