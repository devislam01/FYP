using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Services.IServices
{
    public interface ICartServices
    {
        Task<List<ShoppingCartObj>> GetShoppingCart(Guid curUserID);
        Task<List<ShoppingCartObj>> GetShoppingCartByProductIDs(List<int> productIDs, Guid curUserID);
        Task AddToCart(ShoppingCartRequest payload, Guid curUserID);
        Task UpdateShoppingCart(List<ShoppingCartRequest> payload, Guid curUserID);
    }
}
