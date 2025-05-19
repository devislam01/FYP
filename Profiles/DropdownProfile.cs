using AutoMapper;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Profiles
{
    public class DropdownProfile : Profile
    {
        public DropdownProfile() {
            CreateMap<PaymentMethodDropdownRequest, PaymentMethod>().ForMember(dest => dest.PaymentMethodID, opt => opt.Ignore());
        }
    }
}
