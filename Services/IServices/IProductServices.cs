using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface IProductServices
    {
        Task AddProduct(AddProductRequest product, byte[] curUserID);
        Task<List<ProductListResult>> GetProductList(byte[] curUserID);
    }
}
