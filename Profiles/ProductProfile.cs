using AutoMapper;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;

namespace DemoFYP.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<AddProductRequest, Product>().ForMember(dest => dest.ProductId, opt => opt.Ignore());

            CreateMap<Product, ProductDetailResult>();

            CreateMap<UpdateProductRequest, Product>().ForMember(dest => dest.ProductId, opt => opt.Ignore());

            CreateMap<ProductDetailResult, ShoppingCartObj>();
        }
    }
}
