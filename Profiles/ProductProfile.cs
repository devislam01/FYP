using AutoMapper;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<AddProductRequest, Product>().ForMember(dest => dest.ProductId, opt => opt.Ignore());
        }
    }
}
