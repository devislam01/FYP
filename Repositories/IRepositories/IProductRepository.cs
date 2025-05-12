using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IProductRepository
    {
        #region Read
        Task<List<ProductListResult>> GetProductList();
        Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID);
        Task<ProductDetailResult> GetProductDetailByProductID(int ProductID, Guid curUserID);

        #endregion

        #region Create

        Task InsertProduct(AddProductRequest paylaod, Guid curUserID, string ImageURL);
        Task UpdateProductByProductID(UpdateProductRequest paylaod, Guid curUserID, string ImageURL);

        Task DeleteProductByProductID(int productID, Guid curUserID);

        #endregion
    }
}
