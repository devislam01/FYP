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

        public async Task AddProduct(AddProductRequest payload, byte[] curUserID)
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

        public async Task<List<ProductListResult>> GetProductList(byte[] curUserID)
        {
            if (curUserID == null) { throw new UnauthorizedAccessException("No access!"); }

            try
            {
                return await _productRepository.GetProductListByLoginID(curUserID);
            }
            catch
            {
                throw;
            }
        }
    }
}
