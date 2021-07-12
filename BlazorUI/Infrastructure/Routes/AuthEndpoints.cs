using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI.Infrastructure.Routes
{
    public static class AuthEndpoints
    {
        public static string Login = "api/identity/token/login";
        public static string Refresh = "api/identity/token/refresh";
    }
}
