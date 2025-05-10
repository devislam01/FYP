using DemoFYP.EF;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface IProductRepository
    {
        Task InsertProduct(AddProductRequest paylaod, byte[] curUserID, AppDbContext outerContext = null);
        Task<List<ProductListResult>> GetProductListByLoginID(byte[] curUserID);
    }
}
