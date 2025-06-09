using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface ICartRepository
    {
        Task<List<ShoppingCartObj>> GetShoppingCartByLoginID(Guid curUserID);
        Task<List<ShoppingCartObj>> GetShoppingCartByProductIDs(List<int> productIDs, Guid curUserID);
        Task AddToCart(ShoppingCartRequest payload, Guid curUserID);
        Task UpdateShoppingCart(List<ShoppingCartObj> shoppingCartList, Guid curUserID);
        Task RemovePaidProductFromCart(List<int> paidProductIds, Guid curUserID);
    }
}
