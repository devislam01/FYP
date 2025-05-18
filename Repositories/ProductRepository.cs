using AutoMapper;
using AutoMapper.QueryableExtensions;
using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq;

namespace DemoFYP.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public ProductRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper, IWebHostEnvironment environment, IConfiguration config)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(factory));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Read DB

        public async Task<PagedResult<ProductListResult>> GetProductList(ProductFilterRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.Products
                    .OrderByDescending(p => p.ProductId)
                    .Where(p => p.IsActive == 1);

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(p => p.ProductName.ToLower().StartsWith(filter.Search.ToLower()));
                    filter.DisablePagination = true;
                }

                if (filter.CategoryId != null && filter.CategoryId.Length > 0)
                {
                    query = query.Where(p => filter.CategoryId.Contains(p.CategoryId));
                }

                int totalRecord = await query.CountAsync();

                if (!filter.DisablePagination)
                {
                    query = query
                        .Skip((filter.PageNumber - 1) * filter.PageSize)
                        .Take(filter.PageSize);
                }

                var products = await query.Select(p => new ProductListResult
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    CategoryID = p.CategoryId,
                    ProductCondition = p.ProductCondition,
                    ProductImage = $"{_config["BackendUrl"]}/{p.ProductImage}",
                    ProductPrice = p.ProductPrice
                }).ToListAsync();

                return new PagedResult<ProductListResult>
                {
                    Data = products,
                    Pagination = new PaginationResponse
                    {
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalRecord = totalRecord
                    }
                };
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

        public async Task<PagedResult<AdminProductListResult>> GetProductListByAdmin(AdminProductFilterRequest filter)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var query = context.Products
                    .OrderByDescending(p => p.ProductId)
                    .Where(p => p.IsActive == 1);

                if (filter.ProductID != null && filter.ProductID != 0)
                {
                    query = query.Where(p => p.ProductId == filter.ProductID);
                }

                if (!string.IsNullOrEmpty(filter.ProductName))
                {
                    query = query.Where(p => filter.ProductName.ToLower().StartsWith(p.ProductName.ToLower()));
                }

                if (filter.CategoryID != null && filter.CategoryID != 0)
                {
                    query = query.Where(p => p.CategoryId == filter.CategoryID);
                }

                if (!string.IsNullOrEmpty(filter.ProductCondition))
                {
                    query = query.Where(p => filter.ProductCondition.ToLower().StartsWith(p.ProductCondition.ToLower()));
                }

                if (filter.CreatedAt != null)
                {
                    query = query.Where(p => p.CreatedDateTime == filter.CreatedAt);
                }

                if (filter.IsActive == false)
                {
                    query = query.Where(p => p.IsActive == 0);
                }

                if (filter.BelongsTo != null && filter.BelongsTo != Guid.Empty)
                {
                    query = query.Where(p => p.UserId == filter.BelongsTo);
                }

                int totalRecord = await query.CountAsync();

                var result = await query
                        .Skip((filter.PageNumber - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .Select(p => new AdminProductListResult
                        {
                            ProductID = p.ProductId,
                            ProductName = p.ProductName,
                            ProductDescription = p.ProductDescription,
                            CategoryID = p.CategoryId,
                            ProductCondition = p.ProductCondition,
                            ProductImage = $"{_config["BackendUrl"]}/{p.ProductImage}",
                            ProductPrice = p.ProductPrice,
                            StockQty = p.StockQty,
                            CreatedAt = p.CreatedDateTime,
                            IsActive = p.IsActive == 1,
                            BelongsTo = p.UserId,
                        }).ToListAsync();

                return new PagedResult<AdminProductListResult>
                {
                    Data = result,
                    Pagination = new PaginationResponse
                    {
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalRecord = totalRecord
                    }
                };
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
                        ProductImage = $"{_config["BackendUrl"]}/{pl.ProductImage}",
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

        public async Task<ProductDetailResponse> GetProductDetailByProductID(int ProductID, bool GetImageRealPath)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var productWithCategory = await context.Products
                    .Where(p => p.ProductId == ProductID && p.IsActive == 1)
                    .Join(context.ProductCategories,
                          product => product.CategoryId,
                          pc => pc.CategoryId,
                          (product, category) => new
                          {
                              product,
                              category
                          })
                    .FirstOrDefaultAsync() ?? throw new NotFoundException("Product Not Found");

                var seller = await context.Users
                    .Where(u => u.UserId == productWithCategory.product.UserId)
                    .FirstOrDefaultAsync();

                var completedOrders = await context.Orders
                    .Where(o => o.UserId == productWithCategory.product.UserId)
                    .CountAsync();

                string ImageUrl = GetImageRealPath ? productWithCategory.product.ProductImage : $"{_config["BackendUrl"]}/{productWithCategory.product.ProductImage}";
                var response = new ProductDetailResponse
                {
                    ProductDetail = new ProductDetailResult
                    {
                        ProductID = productWithCategory.product.ProductId,
                        ProductName = productWithCategory.product.ProductName,
                        ProductDescription = productWithCategory.product.ProductDescription,
                        CategoryID = productWithCategory.category.CategoryId,
                        CategoryName = productWithCategory.category.CategoryName,
                        ProductCondition = productWithCategory.product.ProductCondition,
                        ProductImage = ImageUrl,
                        ProductPrice = productWithCategory.product.ProductPrice,
                        StockQty = productWithCategory.product.StockQty
                    },
                    SellerDetail = new SellerDetailResult
                    {
                        SellerName = seller?.UserName ?? "Unknown",
                        RatingMark = seller?.RatingMark,
                        CompletedOrders = completedOrders,
                        JoinTime = seller != null ? (DateTime.Now - seller.CreatedDateTime).Days + " days ago" : "Unknown"
                    }
                };

                return response;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Write DB

        public async Task InsertProduct(AddProductRequest payload, Guid curUserID, string ImageURL)
        {
            var context = _factory.CreateDbContext();
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var newData = _mapper.Map<Product>(payload);

                newData.ProductImage = ImageURL;
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

        public async Task UpdateProductByProductID(UpdateProductRequest payload, Guid curUserID, string ImageURL)
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
                existingData.ProductImage = string.IsNullOrEmpty(ImageURL) ? existingData.ProductImage : ImageURL;
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
                result.UpdatedBy = curUserID;
                result.UpdatedDateTime = DateTime.Now;

                await context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task UnpublishProductByAdmin(int productID, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var result = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productID && p.IsActive == 1) ?? throw new NotFoundException("Product Not Found"); ;

                result.IsActive = 0;
                result.UpdatedBy = curUserID;
                result.UpdatedDateTime = DateTime.Now;

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
