using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepositories) {
            _productRepository = productRepositories ?? throw new ArgumentNullException(nameof(productRepositories));
        }

        #region Read Repositories

        public async Task<List<ProductListResult>> GetProductList()
        {
            try
            {
                return await _productRepository.GetProductList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID)
        {
            try
            {
                return await _productRepository.GetProductListByLoginID(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProductDetailResult> GetProductDetailByProductID(int ProductID, Guid curUserID)
        {
            if (ProductID == 0) { throw new NotFoundException("Missing Product ID"); }

            try {
                return await _productRepository.GetProductDetailByProductID(ProductID, curUserID);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Create Repositories

        public async Task AddProduct(AddProductRequest payload, Guid curUserID)
        {
            if (payload == null) { throw new NotFoundException("No payload was found."); }
            if (string.IsNullOrEmpty(payload.ProductName)) { throw new NotFoundException("Missing Product Name"); }
            if (string.IsNullOrEmpty(payload.ProductDescription)) { throw new NotFoundException("Missing Product Description"); }
            if (payload.CategoryID == 0) { throw new NotFoundException("Missing Category"); }
            if (string.IsNullOrEmpty(payload.ProductCondition)) { throw new NotFoundException("Missing Product Condition"); }
            if (string.IsNullOrEmpty(payload.ProductImage)) { throw new NotFoundException("Missing Product Image"); }
            if (double.IsNaN(payload.ProductPrice)) { throw new NotFoundException("Missing Product Price"); }

            try
            {
                await _productRepository.InsertProduct(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateProductByProductID(UpdateProductRequest payload, Guid curUserID)
        {
            if (payload == null) { throw new NotFoundException("No Payload Was Found."); }
            if (payload.ProductID == 0) { throw new NotFoundException("No Product ID Was Found"); }

            try
            {
                await _productRepository.UpdateProductByProductID(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteProductByProductID(int productID, Guid curUserID)
        {
            if (productID == 0) { throw new NotFoundException("No Product ID Was Found"); }

            try
            {
                await _productRepository.DeleteProductByProductID(productID, curUserID);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
