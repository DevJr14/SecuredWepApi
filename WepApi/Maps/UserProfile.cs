using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WepApi.Context;
using Common.Models.Responses;

namespace WepApi.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserResponse, ApplicationUser>().ReverseMap(); 
        }
    }
}
