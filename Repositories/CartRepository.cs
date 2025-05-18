using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DemoFYP.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IUserRepository _userRepository;

        public CartRepository(IDbContextFactory<AppDbContext> factory, IUserRepository userRepository) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<ShoppingCartObj>> GetShoppingCartByLoginID(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var userData = await context.Users.Where(u => u.UserId == curUserID).FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("User not Found");

                if (string.IsNullOrEmpty(userData.Shopping_Cart)) throw new NotFoundException("Cart is Empty");

                return JsonSerializer.Deserialize<List<ShoppingCartObj>>(userData.Shopping_Cart);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Get Shopping Cart!", ex);
            }
        }

        public async Task UpdateShoppingCart(List<ShoppingCartObj> shoppingCartList, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var curUserData = await _userRepository.GetUserByLoginID(curUserID, context)
                    ?? throw new UnauthorizedAccessException("User not Found!");

                curUserData.UpdatedBy = curUserID;
                curUserData.UpdatedDateTime = DateTime.Now;
                curUserData.Shopping_Cart = JsonSerializer.Serialize(shoppingCartList);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update shopping cart!", ex);
            }
        }
    }
}
