using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DemoFYP.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        private readonly ICommonServices _commonServices;
        public ProductService(IProductRepository productRepositories, ICommonServices commonServices)
        {
            _productRepository = productRepositories ?? throw new ArgumentNullException(nameof(productRepositories));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }

        #region Read Repositories

        public async Task<PagedResult<ProductListResult>> GetProductList(ProductFilterRequest filter)
        {
            try
            {
                return await _productRepository.GetProductList(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResult<AdminProductListResult>> GetProductListByAdmin(AdminProductFilterRequest filter)
        {
            try
            {
                return await _productRepository.GetProductListByAdmin(filter);
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

        public async Task<ProductDetailResponse> GetProductDetailByProductID(int ProductID)
        {
            if (ProductID == 0) throw new BadRequestException("Missing Product ID");

            try {
                return await _productRepository.GetProductDetailByProductID(ProductID);
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
            if (payload == null) throw new BadRequestException("No payload was found.");
            if (payload.ProductImage == null || payload.ProductImage.Length == 0) throw new BadRequestException("Missing Product Image");

            string ImageURL = await _commonServices.UploadImage(payload.ProductImage, "", FolderName.ProductImages.ToString());

            if (string.IsNullOrEmpty(payload.ProductName)) throw new BadRequestException("Missing Product Name");
            if (string.IsNullOrEmpty(payload.ProductDescription)) throw new BadRequestException("Missing Product Description");
            if (payload.CategoryID == 0) throw new BadRequestException("Missing Category");
            if (string.IsNullOrEmpty(payload.ProductCondition)) throw new BadRequestException("Missing Product Condition");
            if (double.IsNaN(payload.ProductPrice)) throw new BadRequestException("Missing Product Price");

            try
            {
                await _productRepository.InsertProduct(payload, curUserID, ImageURL);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Update Services

        public async Task UpdateProductByProductID(UpdateProductRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.ProductID == 0) throw new BadRequestException("Product ID is required");
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException("Session Expired! Please login again");

            string imageURL;

            if (payload.ProductImageFile != null && payload.ProductImageFile.Length > 0)
            {
                imageURL = await _commonServices.UploadImage(payload.ProductImageFile, "", FolderName.ProductImages.ToString());
            }
            else if (!string.IsNullOrEmpty(payload.ProductImageUrl))
            {
                imageURL = payload.ProductImageUrl;
            }
            else
            {
                throw new BadRequestException("Missing Product Image");
            }

            try
            {
                await _productRepository.UpdateProductByProductID(payload, curUserID, imageURL);
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteProductByProductID(int productID, Guid curUserID)
        {
            if (productID == 0) throw new BadRequestException("Product ID is required");
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException("Session Expired! Please login again");

            try
            {
                await _productRepository.DeleteProductByProductID(productID, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task UnpublishProductByAdmin(int productID, Guid curUserID)
        {
            if (productID == 0) throw new BadRequestException("Product ID is required");
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException("Session Expired! Please login again");

            try
            {
                await _productRepository.UnpublishProductByAdmin(productID, curUserID);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
