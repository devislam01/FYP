using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IProductServices
    {
        #region Read Services

        Task<List<ProductListResult>> GetProductList();
        Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID);
        Task<ProductDetailResult> GetProductDetailByProductID(int ProductID, Guid curUserID);

        #endregion

        #region Create Services

        Task AddProduct(AddProductRequest product, Guid curUserID);
        Task UpdateProductByProductID(UpdateProductRequest product, Guid curUserID);
        Task DeleteProductByProductID(int productID, Guid curUserID);

        #endregion
    }
}
