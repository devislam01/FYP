using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Repositories.IRepositories
{
    public interface ICartRepository
    {
        Task<List<ShoppingCartObj>> GetShoppingCartByLoginID(Guid curUserID);
        Task UpdateShoppingCart(List<ShoppingCartObj> shoppingCartList, Guid curUserID);
    }
}
