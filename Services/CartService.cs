using AutoMapper;
using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class CartService : ICartServices
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper) {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ShoppingCartObj>> GetShoppingCart(Guid curUserID)
        {
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException();

            try
            {
                return await _cartRepository.GetShoppingCartByLoginID(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task AddToCart(ShoppingCartRequest payload, Guid curUserID)
        {
            if (payload.ProductID == 0) throw new BadRequestException("Product ID is required");

            try
            {
                await _cartRepository.AddToCart(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateShoppingCart(List<ShoppingCartRequest> payload, Guid curUserID)
        {
            var cartList = new List<ShoppingCartObj>();

            if (payload.Count > 0)
            {
                foreach (var item in payload)
                {
                    if (item.ProductID == 0)
                        throw new BadRequestException("Product ID is required");

                    var latestProductDetails = await _productRepository
                        .GetProductDetailByProductID(item.ProductID, null, true)
                        ?? throw new NotFoundException($"Product ID {item.ProductID} not found");

                    if (item.Quantity > latestProductDetails.ProductDetail.StockQty)
                    {
                        throw new BusinessException(
                            $"Insufficient stock for Product ID {item.ProductID} — only {latestProductDetails.ProductDetail.StockQty} left"
                        );
                    }

                    var cartItem = _mapper.Map<ShoppingCartObj>(latestProductDetails.ProductDetail);
                    cartItem.Quantity = item.Quantity;

                    cartList.Add(cartItem);
                }
            }

            await _cartRepository.UpdateShoppingCart(cartList, curUserID);
        }
    }
}
