using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Requests
{
    public class TokenRefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
