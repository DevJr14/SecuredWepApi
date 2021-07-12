using Common.Models.Requests;
using Common.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorUI.Infrastructure.Managers.Identity.Authentication
{
    public interface IAuthenticationManager : IManager
    {
        Task<IResult> Login(LoginRequest model);

        Task<IResult> Logout();

        Task<string> RefreshToken();

        Task<string> TryRefreshToken();

        Task<string> TryForceRefreshToken();

        Task<ClaimsPrincipal> CurrentUser();
    }
}
