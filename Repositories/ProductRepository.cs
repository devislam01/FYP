using AutoMapper;
using DemoFYP.EF;
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

        public ProductRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) {
            _factory = factory;
            _mapper = mapper;
        }

        public async Task InsertProduct(AddProductRequest payload, byte[] curUserID, AppDbContext outerContext)
        {
            var context = outerContext ?? _factory.CreateDbContext();
            IDbContextTransaction tran = null;

            try
            {
                if (outerContext == null)
                {
                    tran = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                }

                var newData = _mapper.Map<Product>(payload);

                newData.ProductImage = "test";
                newData.CreatedDateTime = DateTime.Now;
                newData.CreatedBy = curUserID;
                newData.UserId = payload.UserID ?? curUserID;

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

        public async Task<List<ProductListResult>> GetProductListByLoginID(byte[] curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                return await context.Products
                    .Where(p => p.UserId == curUserID)
                    .Select(pl => new ProductListResult
                    {
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
    }
}
