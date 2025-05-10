using AutoMapper;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;

namespace DemoFYP.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() { 
            CreateMap<UserRegisterRequest, User>();
        }
    }
}
