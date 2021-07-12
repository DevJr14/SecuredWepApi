using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models.Responses;

namespace WepApi.Maps
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleResponse, IdentityRole>().ReverseMap();
        }
    }
}
