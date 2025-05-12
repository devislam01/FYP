using AutoMapper;
using AutoMapper.QueryableExtensions;
using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DemoFYP.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public ProductRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper)
        {
            _factory = factory;
            _mapper = mapper;
        }

        #region Read DB

        public async Task<List<ProductListResult>> GetProductList()
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Products
                    .Where(p => p.IsActive == 1)
                    .Select(pl => new ProductListResult
                    {
                        ProductID = pl.ProductId,
                        ProductName = pl.ProductName,
                        ProductDescription = pl.ProductDescription,
                        CategoryID = pl.CategoryId,
                        ProductCondition = pl.ProductCondition,
                        ProductImage = pl.ProductImage,
                        ProductPrice = pl.ProductPrice,
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<List<FilteredProductListResult>> GetProductListByLoginID(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Products
                    .Where(p => p.UserId == curUserID && p.IsActive == 1)
                    .Select(pl => new FilteredProductListResult
                    {
                        ProductID = pl.ProductId,
                        ProductName = pl.ProductName,
                        ProductDescription = pl.ProductDescription,
                        CategoryID = pl.CategoryId,
                        ProductCondition = pl.ProductCondition,
                        ProductImage = pl.ProductImage,
                        ProductPrice = pl.ProductPrice,
                        StockQty = pl.StockQty,
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<ProductDetailResult> GetProductDetailByProductID(int ProductID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var product = await context.Products
                            .Where(p => p.ProductId == ProductID && p.IsActive == 1)
                            .Join(context.ProductCategories, pc => pc.CategoryId, product => product.CategoryId, (product, category) => new ProductDetailResult { 
                                ProductName = product.ProductName,
                                ProductDescription = product.ProductDescription,
                                CategoryID = category.CategoryId,
                                CategoryName = category.CategoryName,
                                ProductCondition = product.ProductCondition,
                                ProductImage = product.ProductImage,
                                ProductPrice = product.ProductPrice,
                                StockQty = product.StockQty,
                            })
                            .FirstOrDefaultAsync();

                return _mapper.Map<ProductDetailResult>(product);
            }
            catch
            {
                throw;
            }
        }


        #endregion

        #region Write DB

        public async Task InsertProduct(AddProductRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var newData = _mapper.Map<Product>(payload);

                newData.ProductImage = "test";
                newData.CreatedDateTime = DateTime.Now;
                newData.CreatedBy = curUserID;
                newData.UserId = payload.UserID.HasValue && payload.UserID.Value != Guid.Empty ? payload.UserID.Value : curUserID;

                await context.AddAsync(newData);
                await context.SaveChangesAsync();

                if (tran != null)
                {
                    await tran.CommitAsync();
                }
            }
            catch
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }

                throw;
            }
            finally
            {
                if (tran != null)
                {
                    await tran.DisposeAsync();
                }
            }
        }

        public async Task UpdateProductByProductID(UpdateProductRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var existingData = await context.Products.FirstOrDefaultAsync(p => p.ProductId == payload.ProductID && p.IsActive == 1) ?? throw new NotFoundException("Product Not Found");

                if (existingData.UserId != curUserID) { throw new ForbiddenException(); }

                existingData.ProductName = payload.ProductName ?? existingData.ProductName;
                existingData.ProductDescription = payload.ProductDescription ?? existingData.ProductDescription;
                existingData.CategoryId = payload.CategoryID ?? existingData.CategoryId;
                existingData.ProductCondition = payload.ProductCondition ?? existingData.ProductCondition;
                existingData.ProductImage = payload.ProductImage ?? existingData.ProductImage;
                existingData.ProductPrice = payload.ProductPrice ?? existingData.ProductPrice;
                existingData.StockQty = payload.StockQty ?? existingData.StockQty;
                existingData.UpdatedDateTime = DateTime.Now;
                existingData.UpdatedBy = curUserID;
                existingData.IsActive = payload.IsActive ?? existingData.IsActive;
                existingData.UserId = payload.UserID.HasValue && payload.UserID.Value != Guid.Empty ? payload.UserID.Value : existingData.UserId;


                await context.SaveChangesAsync();

                if (tran != null)
                {
                    await tran.CommitAsync();
                }
            }
            catch
            {
                if (tran != null)
                {
                    await tran.RollbackAsync();
                }

                throw;
            }
            finally
            {
                if (tran != null)
                {
                    await tran.DisposeAsync();
                }
            }
        }

        public async Task DeleteProductByProductID(int productID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productID && p.IsActive == 1) ?? throw new NotFoundException("Product Not Found"); ;

                if (result.UserId != curUserID) { throw new ForbiddenException(); }

                result.IsActive = 0;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
