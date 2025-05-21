using AutoMapper;
using DemoFYP.EF;
using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoFYP.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartRepository(IDbContextFactory<AppDbContext> factory, IUserRepository userRepository, IProductRepository productRepository, IMapper mapper) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ShoppingCartObj>> GetShoppingCartByLoginID(Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var userData = await context.Users.Where(u => u.UserId == curUserID).FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("User not Found");

                if (!string.IsNullOrEmpty(userData.Shopping_Cart))
                {
                    return JsonSerializer.Deserialize<List<ShoppingCartObj>>(userData.Shopping_Cart);
                }

                return new List<ShoppingCartObj>();
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

        public async Task AddToCart(ShoppingCartRequest payload, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var productDetail = await _productRepository.GetProductDetailByProductID(payload.ProductID, context, true);

                if (payload.Quantity > productDetail.ProductDetail.StockQty)
                {
                    throw new BusinessException(
                        $"Insufficient stock for Product ID {payload.ProductID} — only {productDetail.ProductDetail.StockQty} left"
                    );
                }

                var newCartItem = _mapper.Map<ShoppingCartObj>(productDetail.ProductDetail);
                newCartItem.Quantity = payload.Quantity;

                var curUserData = await _userRepository.GetUserByLoginID(curUserID, context)
                    ?? throw new UnauthorizedAccessException("User not Found!");

                var currentCartJson = curUserData.Shopping_Cart ?? "[]";
                var cartItems = JsonSerializer.Deserialize<List<ShoppingCartObj>>(currentCartJson) ?? new List<ShoppingCartObj>();

                var existingItem = cartItems.FirstOrDefault(c => c.ProductID == newCartItem.ProductID);
                if (existingItem != null)
                {
                    existingItem.Quantity += newCartItem.Quantity;
                }
                else
                {
                    cartItems.Add(newCartItem);
                }

                curUserData.Shopping_Cart = JsonSerializer.Serialize(cartItems);
                curUserData.UpdatedBy = curUserID;
                curUserData.UpdatedDateTime = DateTime.Now;

                await context.SaveChangesAsync();
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
            catch
            {
                throw;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task RemovePaidProductFromCart(List<int> paidProductIds, Guid curUserID)
        {
            var context = _factory.CreateDbContext();

            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == curUserID) ?? throw new NotFoundException("User not Found");

                var cartItems = JsonSerializer.Deserialize<List<ShoppingCartObj>>(user.Shopping_Cart);

                if (cartItems == null)
                    return;

                cartItems.RemoveAll(item => paidProductIds.Contains(item.ProductID));

                if (cartItems.Any())
                {
                    user.Shopping_Cart = JsonSerializer.Serialize(cartItems);
                }
                else
                {
                    user.Shopping_Cart = null;
                }
               

                await context.SaveChangesAsync();
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
