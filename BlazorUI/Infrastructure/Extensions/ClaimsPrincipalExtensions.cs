using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorUI.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal claimsPrincipal)
               => claimsPrincipal.FindFirstValue(ClaimTypes.Email);

        public static string GetFirstName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.Name);

        public static string GetLastName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.Surname);

        public static string GetPhoneNumber(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.MobilePhone);

        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
           => claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
