using AutoMapper;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<AddRoleRequest, Role>();
        }
    }
}
